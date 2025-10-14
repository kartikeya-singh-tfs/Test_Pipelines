using System.Collections.Concurrent;
using System.Threading.Channels;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ThermoFisher.AcquisitionModule.Contracts;

namespace ThermoFisher.AcquisitionModule.Grpc;

public class RealTimePlotDataAccessService : RealTimePlotService.RealTimePlotServiceBase
{
    // Logger for diagnostic and error messages
    private readonly ILogger<RealTimePlotDataAccessService> _logger;

    // private readonly IServerRawDataDispatcher _serverRawDataDispatcher;

    // Stores channels for Server-Sent Events (SSE) with their associated data type key
    private readonly ConcurrentDictionary<Channel<string>, string> _sseChannels = new();

    // Constructor with dependency injection for logger
    public RealTimePlotDataAccessService(ILogger<RealTimePlotDataAccessService> logger) //, IServerRawDataDispatcher serverRawDataDispatcher)
    {
        _logger = logger;
        //_serverRawDataDispatcher = serverRawDataDispatcher;
    }

    /// <summary>
    /// Adds a new SSE channel to the dictionary if it does not already exist.
    /// </summary>
    /// <param name="key">The data type key associated with the channel.</param>
    /// <param name="inChannel">The channel to add.</param>
    public void AddSseChannel(string key, Channel<string> inChannel)
    {
        if (!_sseChannels.TryGetValue(inChannel, out _))
        {
            _sseChannels.TryAdd(inChannel, key);
        }
    }

    /// <summary>
    /// Removes an SSE channel from the dictionary.
    /// </summary>
    /// <param name="key">The data type key associated with the channel.</param>
    /// <param name="inChannel">The channel to remove.</param>
    public void RemoveSseChannel(string key, Channel<string> inChannel)
    {
        _sseChannels.TryRemove(inChannel, out _);
    }

    /// <summary>
    /// Streams SparklineData from the client, serializes it, and writes it to all matching SSE channels.
    /// Sends a response for each data item received.
    /// </summary>
    public override async Task StreamSparklineData(
        IAsyncStreamReader<SparklineData> sparklineData,
        IServerStreamWriter<PlotDataResponse> responseStream,
        ServerCallContext context
    )
    {
        try
        {
            // The type name used as a key for channel matching
            var typeName = typeof(SparklineDataDto).ToString();

            // Read all incoming SparklineData asynchronously
            await foreach (var data in sparklineData.ReadAllAsync(context.CancellationToken))
            {
                // Serialize the data to JSON using the DTO wrapper
                var json = JsonFormatter.Default.Format(
                    new SparklineDataDto { Type = data.GetType().Name, Data = data }
                );

                // Write the serialized data to all matching SSE channels
                foreach (var item in _sseChannels)
                {
                    if (
                        string.Compare(item.Value, typeName, StringComparison.OrdinalIgnoreCase)
                        != 0
                    )
                        continue;

                    item.Key.Writer.TryWrite(json);

                    // Send a response back to the client for each data item
                    var response = new PlotDataResponse
                    {
                        Status = UploadStatusType.Ok,
                        Message =
                            $"SparklineStream: Received: scan number range: {data.StartScanNumber}-{data.EndScanNumber}, count: {data.Intensities.Count}",
                    };
                    await responseStream.WriteAsync(response);
                }
            }
        }
        catch (Exception e)
        {
            // Log errors if any occur during streaming
            _logger?.LogError(e.Message);
            _logger?.LogError($"Server: SparklineStream shutdown: {e.Message}");
        }
        finally
        {
            // Log stream shutdown for diagnostics
            _logger?.LogInformation($"Server: SparklineStream shutdown for client:{context.Peer}");
        }
    }

    /// <summary>
    /// Streams ChromatogramSvgData from the client, serializes it, and writes it to all matching SSE channels.
    /// Sends a response for each data item received.
    /// </summary>
    public override async Task StreamChromatogramSvgData(
        IAsyncStreamReader<ChromatogramSvgData> chromatogramSvgData,
        IServerStreamWriter<PlotDataResponse> responseStream,
        ServerCallContext context
    )
    {
        try
        {
            // The type name used as a key for channel matching
            var typeName = typeof(ChromatogramSvgDataDto).ToString();

            // Read all incoming ChromatogramSvgData asynchronously
            await foreach (var data in chromatogramSvgData.ReadAllAsync(context.CancellationToken))
            {
                // Serialize the data to JSON using the DTO wrapper
                var json = JsonFormatter.Default.Format(
                    new ChromatogramSvgDataDto { Type = data.GetType().Name, Data = data }
                );

                // Write the serialized data to all matching SSE channels
                foreach (var item in _sseChannels)
                {
                    if (
                        string.Compare(item.Value, typeName, StringComparison.OrdinalIgnoreCase)
                        != 0
                    )
                        continue;

                    item.Key.Writer.TryWrite(json);

                    // Send a response back to the client for each data item
                    var response = new PlotDataResponse
                    {
                        Status = UploadStatusType.Ok,
                        Message =
                            $"StreamChromatogramSvgData: Received: scan number range: {data.StartScanNumber}-{data.EndScanNumber}",
                    };
                    await responseStream.WriteAsync(response);
                }
            }
        }
        catch (Exception e)
        {
            // Log errors if any occur during streaming
            _logger?.LogError(e.Message);
            _logger?.LogError($"Server: StreamChromatogramSvgData shutdown: {e.Message}");
        }
        finally
        {
            // Log stream shutdown for diagnostics
            _logger?.LogInformation(
                $"Server: StreamChromatogramSvgData shutdown for client:{context.Peer}"
            );
        }
    }

    /// <summary>
    /// Handles a single SparklineData upload, serializes it, and writes it to all matching SSE channels.
    /// Returns a response to the client.
    /// </summary>
    public override Task<PlotDataResponse> UploadSparklineData(
        SparklineData request,
        ServerCallContext context
    )
    {
        try
        {
            // The type name used as a key for channel matching
            var typeName = typeof(SparklineDataDto).ToString();
            // Serialize the data to JSON using the DTO wrapper
            var json = JsonFormatter.Default.Format(
                new SparklineDataDto { Type = request.GetType().Name, Data = request }
            );

            // Write the serialized data to all matching SSE channels
            foreach (var item in _sseChannels)
            {
                if (string.Compare(item.Value, typeName, StringComparison.OrdinalIgnoreCase) != 0)
                    continue;

                item.Key.Writer.TryWrite(json);
            }
        }
        catch (Exception e)
        {
            // Log errors if any occur during upload
            _logger?.LogError(e.Message);
            _logger?.LogError($"Server: UploadSparkline shutdown: {e.Message}");
        }
        finally
        {
            // Log upload shutdown for diagnostics
            _logger?.LogInformation($"Server: UploadSparkline shutdown for client:{context.Peer}");
        }

        // Build and return the response to the client
        var response = new PlotDataResponse
        {
            Status = UploadStatusType.Ok,
            Message =
                $"UploadSparkline: Received: scan number range: {request.StartScanNumber}-{request.EndScanNumber}, count: {request.Intensities.Count}",
        };

        return Task.FromResult(response);
    }

    /// <summary>
    /// Handles a single ChromatogramSvgData upload, serializes it, and writes it to all matching SSE channels.
    /// Returns a response to the client.
    /// </summary>
    public override Task<PlotDataResponse> UploadChromatogramSvgData(
        ChromatogramSvgData request,
        ServerCallContext context
    )
    {
        try
        {
            // The type name used as a key for channel matching
            var typeName = typeof(ChromatogramSvgDataDto).ToString();
            // Serialize the data to JSON using the DTO wrapper
            var json = JsonFormatter.Default.Format(
                new ChromatogramSvgDataDto { Type = request.GetType().Name, Data = request }
            );

            // Write the serialized data to all matching SSE channels
            foreach (var item in _sseChannels)
            {
                if (string.Compare(item.Value, typeName, StringComparison.OrdinalIgnoreCase) != 0)
                    continue;

                item.Key.Writer.TryWrite(json);
            }
        }
        catch (Exception e)
        {
            // Log errors if any occur during upload
            _logger?.LogError(e.Message);
            _logger?.LogError($"Server: UploadChromatogramSvgData shutdown: {e.Message}");
        }
        finally
        {
            // Log upload shutdown for diagnostics
            _logger?.LogInformation(
                $"Server: UploadChromatogramSvgData shutdown for client:{context.Peer}"
            );
        }

        // Build and return the response to the client
        var response = new PlotDataResponse
        {
            Status = UploadStatusType.Ok,
            Message =
                $"UploadChromatogramSvgData: Received: scan number range: {request.StartScanNumber}-{request.EndScanNumber}",
        };

        return Task.FromResult(response);
    }
}
