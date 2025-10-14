using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Utils;
using Microsoft.Extensions.Logging;
using ThermoFisher.Foundation.Acquisition;
using static Thermofisher.Opal.IpcDataAccessModule.DataAccess.DataAccessModule;

namespace Thermofisher.Opal.IpcDataAccessModule.DataAccess
{
    public struct DataAccessContext
    {
        public AcquisitionSample AcqSample;
        public Guid SequenceId;
        public string GrpcServer;
        public CancellationTokenSource Cts;
        public Task DataAccessWorker;
        public GetOpalSequenceIdForFoundationSequenceId GetOpalSequenceId;
    }

    // We use the client to send data to the server, even in what feels like a "reverse" scenario —
    // as long as the client initiates the call, it's allowed.
    // Use cases:
    // Unary Call (upload single item) - Client sends one message to the server:
    // Client Streaming (upload many items) - Client sends a stream of messages to the server
    // Bidirectional Streaming - Client sends data continuously; server can also respond (or not):
    //
    public class DataAccessModule : IDisposable
    {
        private readonly ConcurrentDictionary<string, DataAccessContext> _dataContexts =
            new ConcurrentDictionary<string, DataAccessContext>();
        private readonly PlotDataManager _plotDataManager;
        private IAcqMonitor _acqMonitor;
        private readonly string _grpcServer;
        private readonly ILogger _logger;
        private ILoggerFactory _loggerFactory;

        private bool _isDisposed;

        private GetOpalSequenceIdForFoundationSequenceId _getOpalSequenceIdCallback;

        // ----------------------------------------------------
        // reference the same delegate type (ideally defined in a shared library).
        // can also use Action<T>, Func<T>, or custom delegates as needed
        // Delegates can point to instance methods, static methods, or even lambdas
        public delegate Guid GetOpalSequenceIdForFoundationSequenceId(Guid foundationSequenceId);

        public void RegisterGetSequenceIdCallback(GetOpalSequenceIdForFoundationSequenceId callback)
        {
            _getOpalSequenceIdCallback = callback;
        }

        // Constructor for DataAccessModule.
        // Initializes logger, reads gRPC server address from config, checks validity, sets up acquisition monitor, and creates PlotDataManager.
        public DataAccessModule(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger("DataAccessModule");

            _grpcServer = ConfigurationManager.AppSettings["GrpcServer"];
            GrpcPreconditions.CheckArgument(
                !string.IsNullOrWhiteSpace(_grpcServer),
                "gRPC Service address cannot be null or empty"
            );

            InitializeAcquisitionMonitor();

            _plotDataManager = new PlotDataManager(loggerFactory);
        }

        /// <summary>
        /// Registers an acquisition sample for data access processing.
        /// Validates input, creates a DataAccessContext, starts a worker task, and adds the context to the dictionary.
        /// </summary>
        /// <param name="runState">The run state event arguments containing sequence information.</param>
        /// <param name="acqSample">The acquisition sample to register.</param>
        public void AcquisitionSampleRegister(
            RunStateEventArgs runState,
            AcquisitionSample acqSample
        )
        {
            try
            {
                // Validate required fields
                GrpcPreconditions.CheckArgument(
                    !string.IsNullOrWhiteSpace(acqSample.SampleId),
                    "Sample ID cannot be null or empty"
                );
                GrpcPreconditions.CheckArgument(
                    !string.IsNullOrWhiteSpace(acqSample.RawFileNameFull),
                    "Raw Data URI cannot be null or empty"
                );
                GrpcPreconditions.CheckArgument(
                    !string.IsNullOrWhiteSpace(runState.RunState.SequenceId.ToString()),
                    "Raw Data URI cannot be null or empty"
                );

                _logger?.LogInformation(
                    $"Registering sample with ID: {acqSample.SampleId} and Raw Data URI: {acqSample.RawFileNameFull}"
                );

                // Create a new data context for this sample
                var dataContext = new DataAccessContext
                {
                    AcqSample = acqSample,
                    SequenceId = runState.RunState.SequenceId,
                    GrpcServer = _grpcServer,
                    Cts = Utilities.CreateCancellationTokenSource(),
                    GetOpalSequenceId = _getOpalSequenceIdCallback,
                };

                // Start the data access worker task
                dataContext.DataAccessWorker = Task.Run(
                    async () => await ProcessDataAccessStartAsync(dataContext),
                    dataContext.Cts.Token
                );

                // Add the context to the dictionary for tracking
                _dataContexts.TryAdd(acqSample.SampleId, dataContext);
            }
            catch (Exception e)
            {
                // Log any exceptions encountered during registration
                _logger?.LogError(e, "AcquisitionSampleRegister error");
            }
        }

        /// <summary>
        /// Handles the completion of an acquisition sample.
        /// Validates the sample ID, logs completion, removes the data context, and ensures the worker task is finished.
        /// </summary>
        /// <param name="runEvent">The run event arguments containing event information.</param>
        /// <param name="acqSample">The acquisition sample that has completed.</param>
        public void AcquisitionSampleComplete(
            RunEventEventArgs runEvent,
            AcquisitionSample acqSample
        )
        {
            // Extract the sample ID from the acquisition sample
            var sampleId = acqSample.SampleId;

            // Validate that the sample ID is not null or empty
            GrpcPreconditions.CheckArgument(
                !string.IsNullOrWhiteSpace(sampleId),
                "Sample ID cannot be null or empty"
            );

            // Log the completion of the sample
            _logger?.LogInformation($"Completed sample with ID: {sampleId}");

            // Try to retrieve and remove the data context for the completed sample
            if (_dataContexts.TryGetValue(sampleId, out var job))
            {
                _dataContexts.TryRemove(sampleId, out _);

                // Run a task to wait for the data access worker to complete
                _ = Task.Run(() =>
                {
                    try
                    {
                        // If the worker is already completed or canceled, log the status
                        if (job.DataAccessWorker.IsCompleted || job.DataAccessWorker.IsCanceled)
                        {
                            _logger?.LogInformation(
                                $"Completed sample with ID: {sampleId}, {job.DataAccessWorker.Status}"
                            );
                        }
                        else
                        {
                            // Otherwise, wait up to 60 seconds for the worker to finish
                            _logger?.LogInformation(
                                $"Waiting for sample ID completed: {sampleId}, {job.DataAccessWorker.Status}"
                            );
                            if (!job.DataAccessWorker.Wait(TimeSpan.FromSeconds(60.0)))
                            {
                                // If the wait times out, cancel the task and log a timeout message
                                job.Cts.Cancel(false);
                                _logger?.LogInformation(
                                    $"Timeout while waiting for sample ID to complete: {sampleId}, {job.DataAccessWorker.Status}"
                                );
                                return;
                            }

                            // Log the successful completion of the worker
                            _logger?.LogInformation(
                                $"Sample completed with ID: {sampleId}, {job.DataAccessWorker.Status}"
                            );
                        }
                    }
                    catch (Exception e)
                    {
                        // Log any exceptions encountered while waiting for the worker to complete
                        _logger?.LogError(e, "AcquisitionSampleComplete error");
                    }
                });
            }
        }

        /// <summary>
        /// Starts the data access process for a given data context.
        /// Attempts to connect to the gRPC server and start the plot data manager.
        /// Retries on failure, with a delay between attempts, until the operation is completed or cancelled.
        /// </summary>
        /// <param name="dataContext">The data access context containing sample and connection information.</param>
        public async Task ProcessDataAccessStartAsync(DataAccessContext dataContext)
        {
            var isCompleted = false;

            // Continue attempting to process data access until completed or cancellation is requested
            while (!dataContext.Cts.Token.IsCancellationRequested && !isCompleted)
            {
                Channel channel = null;

                try
                {
                    // Attempt to connect to the gRPC server with retry logic
                    channel = await ConnectionProvider.ConnectWithRetryAsync(
                        dataContext.GrpcServer,
                        ChannelCredentials.Insecure,
                        _logger
                    );

                    // Start the plot data manager for this data context
                    await _plotDataManager.StartAsync(dataContext, channel);

                    // Mark as completed if successful
                    isCompleted = true;
                }
                catch (RpcException ex)
                {
                    // Log gRPC-specific errors
                    _logger?.LogError(ex, $"[Error] gRPC Error: {ex.Status}");
                }
                catch (Exception ex)
                {
                    // Log general errors
                    _logger?.LogError(ex, $"[Error] {ex.Message}");
                }

                // Ensure the channel is properly shut down after use
                if (channel != null)
                {
                    await channel.ShutdownAsync();
                }

                // If completed, skip the reconnect delay
                if (isCompleted)
                    continue;

                // Log and wait before retrying connection
                _logger?.LogInformation("Reconnecting in 2 seconds...");
                await Task.Delay(2000);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            // stop listening to the acquisition service
            _acqMonitor?.Dispose();

            // cleanup the data contexts
            foreach (var kvp in _dataContexts)
            {
                kvp.Value.Cts.Cancel();
                if (!kvp.Value.DataAccessWorker.Wait(TimeSpan.FromSeconds(30.0)))
                {
                    kvp.Value.Cts.Cancel(false);
                    _logger?.LogInformation(
                        $"Timeout while waiting for sample ID to complete: {kvp.Value.AcqSample.SampleId}, {kvp.Value.DataAccessWorker.Status}"
                    );
                }
            }

            _dataContexts.Clear();
            _plotDataManager?.Dispose();
        }

        /// <summary>
        /// Initializes the acquisition monitor, subscribes to acquisition events, and establishes a connection
        /// to the acquisition service. Handles exceptions and logs errors if initialization fails.
        /// </summary>
        private void InitializeAcquisitionMonitor()
        {
            // Create a new instance of AcqMonitor using the provided logger factory
            _acqMonitor = new AcqMonitor(_loggerFactory);

            // Subscribe to acquisition sample events
            _acqMonitor.SampleAcquire += AcquisitionSampleRegister;
            _acqMonitor.SampleComplete += AcquisitionSampleComplete;

            try
            {
                // Attempt to establish a connection to the acquisition service on localhost
                if (_acqMonitor.EstablishAcquisitionServiceConnection("localhost", -1))
                {
                    // Subscribe to acquisition events if the connection is successful
                    _acqMonitor.Subscribe();
                }
            }
            catch (Exception e)
            {
                // Log any exceptions that occur during initialization
                _logger?.LogError(e, "InitializeAcquisitionMonitor error");
            }
        }
    }
}
