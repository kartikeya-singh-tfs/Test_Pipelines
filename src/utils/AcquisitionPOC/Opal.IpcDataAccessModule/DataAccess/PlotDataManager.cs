using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Utils;
using Microsoft.Extensions.Logging;
using ThermoFisher.AcquisitionModule.Contracts;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;

namespace Thermofisher.Opal.IpcDataAccessModule.DataAccess
{
    public struct PlotDataContext
    {
        public DataAccessContext Context;
        public CancellationTokenSource Cts;
        public Task UploadPlotDataWorker;
        public Task UploadSparklineDataWorker;
    }

    internal class PlotDataManager : IDisposable
    {
        // ConcurrentDictionary<(string sequenceId, string sampleId), PlotDataContext>
        private readonly ConcurrentDictionary<(string, string), PlotDataContext> _plotDataContexts =
            new ConcurrentDictionary<(string, string), PlotDataContext>();
        private readonly ILogger<PlotDataManager> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private bool _isDisposed;

        public PlotDataManager(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PlotDataManager>();
        }

        public async Task StartAsync(DataAccessContext dataAccessContext, Channel channel)
        {
            var sampleId = dataAccessContext.AcqSample.SampleId;
            var sequenceId = dataAccessContext.SequenceId.ToString();

            try
            {
                GrpcPreconditions.CheckArgument(
                    !string.IsNullOrWhiteSpace(sampleId),
                    "Sample ID cannot be null or empty"
                );
                GrpcPreconditions.CheckArgument(
                    !string.IsNullOrWhiteSpace(sequenceId),
                    "Sequence ID cannot be null or empty"
                );

                if (_plotDataContexts.TryRemove((sequenceId, sampleId), out var plotDataContext))
                {
                    plotDataContext.Cts.Cancel();
                }

                plotDataContext = new PlotDataContext
                {
                    Context = dataAccessContext,
                    Cts = new CancellationTokenSource(),
                };

                if (_plotDataContexts.TryAdd((sequenceId, sampleId), plotDataContext))
                {
                    var tasks = new Task[2];
                    var client = new RealTimePlotService.RealTimePlotServiceClient(channel);

                    tasks[0] = plotDataContext.UploadPlotDataWorker =
                        //Task.Run(async () => await UploadChromatogramSvgDataAsync(client, dataAccessContext, plotDataContext.Cts.Token));
                        Task.Run(async () =>
                            await StreamChromatogramSvgDataAsync(
                                client,
                                dataAccessContext,
                                plotDataContext.Cts.Token
                            )
                        );

                    tasks[1] = plotDataContext.UploadSparklineDataWorker =
                        //Task.Run(async () => await UploadSparklineDataAsync(client, dataAccessContext, plotDataContext.Cts.Token));
                        Task.Run(async () =>
                            await StreamSparklineDataAsync(
                                client,
                                dataAccessContext,
                                plotDataContext.Cts.Token
                            )
                        );

                    await Task.WhenAll(tasks);
                }

                _logger?.LogInformation(
                    plotDataContext.UploadPlotDataWorker.Status == TaskStatus.RanToCompletion
                        ? $"StartAsync: Done: {sampleId}"
                        : $"StartAsync: {sampleId}, {plotDataContext.UploadPlotDataWorker.Status}"
                );
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "StartAsync error");
                throw;
            }
            finally
            {
                _plotDataContexts.TryRemove((sequenceId, sampleId), out var plotDataContext);
            }
        }

        private static async Task DummyStreamAsync(
            RealTimePlotService.RealTimePlotServiceClient client,
            DataAccessContext dataAccessContext,
            CancellationToken token
        )
        {
            await Task.Delay(1000, token);
        }

        private async Task UploadSparklineDataAsync(
            RealTimePlotService.RealTimePlotServiceClient client,
            DataAccessContext dataAccessContext,
            CancellationToken token
        )
        {
            try
            {
                if (
                    !InitializeRawFileManager(
                        dataAccessContext,
                        out var rawFileManager,
                        out var rawDataAccessor,
                        out var detectorReader
                    )
                )
                {
                    return;
                }

                var nextStartSpectrum = 0;
                var prevEndSpectrum = 0;

                var opalSeqId = dataAccessContext.GetOpalSequenceId?.Invoke(
                    dataAccessContext.SequenceId
                );
                var sparklineData = new SparklineData
                {
                    SampleId = dataAccessContext.AcqSample.SampleId,
                    SequenceId = opalSeqId.ToString(),
                    StartScanNumber = 1,
                };

                while (!token.IsCancellationRequested && rawDataAccessor.InAcquisition)
                {
                    rawDataAccessor.RefreshViewOfFile();
                    var runHeader = detectorReader.RunHeaderEx;

                    if (runHeader.FirstSpectrum < 1)
                        continue;

                    var startSpectrum = GetStartAndEndSpectrumNumbers(
                        nextStartSpectrum,
                        runHeader,
                        out var endSpectrum
                    );

                    if (prevEndSpectrum == startSpectrum && prevEndSpectrum == endSpectrum)
                        continue;

                    UpdateSparklineDataBuffer(
                        detectorReader,
                        startSpectrum,
                        endSpectrum,
                        sparklineData
                    );
                    var response = await client.UploadSparklineDataAsync(sparklineData);

                    // check response
                    _logger?.LogInformation(
                        $"UploadSparklineDataAsync (A): received from server: {response.Message}, Status: {response.Status}"
                    );
                    await Task.Delay(1000, token);

                    nextStartSpectrum = endSpectrum;
                    prevEndSpectrum = endSpectrum;
                }

                if (!rawDataAccessor.InAcquisition)
                {
                    detectorReader?.Dispose();
                    rawDataAccessor?.Dispose();
                    rawFileManager?.Dispose();

                    if (
                        !InitializeRawFileManager(
                            dataAccessContext,
                            out rawFileManager,
                            out rawDataAccessor,
                            out detectorReader
                        )
                    )
                    {
                        return;
                    }

                    var runHeader = detectorReader.RunHeaderEx;
                    var startSpectrum = GetStartAndEndSpectrumNumbers(
                        nextStartSpectrum,
                        runHeader,
                        out var endSpectrum
                    );

                    if (prevEndSpectrum == startSpectrum && prevEndSpectrum == endSpectrum)
                    {
                        _logger?.LogInformation(
                            $"UploadSparklineDataAsync (B): Done: {dataAccessContext.AcqSample.SampleId}"
                        );
                    }
                    else
                    {
                        UpdateSparklineDataBuffer(
                            detectorReader,
                            startSpectrum,
                            endSpectrum,
                            sparklineData
                        );
                        var response = await client.UploadSparklineDataAsync(sparklineData);

                        // check response
                        _logger?.LogInformation(
                            $"UploadSparklineDataAsync: received from server: {response.Message}, Status: {response.Status}"
                        );
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger?.LogError("UploadSparklineDataAsync: shutdown requested.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"UploadSparklineDataAsync: {ex.Message}.");
            }
        }

        private async Task UploadChromatogramSvgDataAsync(
            RealTimePlotService.RealTimePlotServiceClient client,
            DataAccessContext dataAccessContext,
            CancellationToken token
        )
        {
            try
            {
                if (
                    !InitializeRawFileManager(
                        dataAccessContext,
                        out var rawFileManager,
                        out var rawDataAccessor,
                        out var detectorReader
                    )
                )
                {
                    return;
                }

                var chromatogramSvgGenerator = new ChromatogramSvgGenerator(_loggerFactory);
                var nextStartSpectrum = 0;
                var prevEndSpectrum = 0;

                var opalSeqId = dataAccessContext.GetOpalSequenceId?.Invoke(
                    dataAccessContext.SequenceId
                );
                var chromSvgData = new ChromatogramSvgData()
                {
                    SampleId = dataAccessContext.AcqSample.SampleId,
                    SequenceId = opalSeqId.ToString(),
                    StartScanNumber = 1,
                };

                while (!token.IsCancellationRequested && rawDataAccessor.InAcquisition)
                {
                    rawDataAccessor.RefreshViewOfFile();
                    var runHeader = detectorReader.RunHeaderEx;

                    if (runHeader.FirstSpectrum < 1)
                        continue;

                    var startSpectrum = GetStartAndEndSpectrumNumbers(
                        nextStartSpectrum,
                        runHeader,
                        out var endSpectrum
                    );

                    if (prevEndSpectrum == startSpectrum && prevEndSpectrum == endSpectrum)
                        continue;

                    var realtimePlotData = chromatogramSvgGenerator.GetRealtimePlotDataSvg(
                        dataAccessContext,
                        opalSeqId.ToString(),
                        detectorReader,
                        1,
                        endSpectrum
                    );
                    chromSvgData.EndScanNumber = endSpectrum;
                    chromSvgData.ChromatogramSvg = realtimePlotData;

                    var response = await client.UploadChromatogramSvgDataAsync(chromSvgData);

                    // check response
                    _logger?.LogInformation(
                        $"UploadChromatogramSvgDataAsync (A): received from server: {response.Message}, Status: {response.Status}"
                    );
                    await Task.Delay(1000, token);

                    nextStartSpectrum = endSpectrum;
                    prevEndSpectrum = endSpectrum;
                }

                if (!rawDataAccessor.InAcquisition)
                {
                    detectorReader?.Dispose();
                    rawDataAccessor?.Dispose();
                    rawFileManager?.Dispose();

                    if (
                        !InitializeRawFileManager(
                            dataAccessContext,
                            out rawFileManager,
                            out rawDataAccessor,
                            out detectorReader
                        )
                    )
                    {
                        return;
                    }

                    var runHeader = detectorReader.RunHeaderEx;
                    var startSpectrum = GetStartAndEndSpectrumNumbers(
                        nextStartSpectrum,
                        runHeader,
                        out var endSpectrum
                    );

                    if (prevEndSpectrum == startSpectrum && prevEndSpectrum == endSpectrum)
                    {
                        _logger?.LogInformation(
                            $"UploadChromatogramSvgDataAsync (B): Done: {dataAccessContext.AcqSample.SampleId}"
                        );
                    }
                    else
                    {
                        var realtimePlotData = chromatogramSvgGenerator.GetRealtimePlotDataSvg(
                            dataAccessContext,
                            opalSeqId.ToString(),
                            detectorReader,
                            1,
                            endSpectrum
                        );

                        chromSvgData.EndScanNumber = endSpectrum;
                        chromSvgData.ChromatogramSvg = realtimePlotData;

                        var response = await client.UploadChromatogramSvgDataAsync(chromSvgData);

                        // check response
                        _logger?.LogInformation(
                            $"UploadChromatogramSvgDataAsync: received from server: {response.Message}, Status: {response.Status}"
                        );
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger?.LogError("UploadChromatogramSvgDataAsync: shutdown requested.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"UploadChromatogramSvgDataAsync: {ex.Message}.");
            }
        }

        private async Task StreamSparklineDataAsync(
            RealTimePlotService.RealTimePlotServiceClient client,
            DataAccessContext dataAccessContext,
            CancellationToken token
        )
        {
            var call = client.StreamSparklineData(cancellationToken: token);

            try
            {
                if (
                    !InitializeRawFileManager(
                        dataAccessContext,
                        out var rawFileManager,
                        out var rawDataAccessor,
                        out var detectorReader
                    )
                )
                {
                    return;
                }

                var nextStartSpectrum = 0;
                var prevEndSpectrum = 0;

                var opalSeqId = dataAccessContext.GetOpalSequenceId?.Invoke(
                    dataAccessContext.SequenceId
                );
                var sparklineData = new SparklineData
                {
                    SampleId = dataAccessContext.AcqSample.SampleId,
                    SequenceId = opalSeqId.ToString(),
                    StartScanNumber = 1,
                };

                while (!token.IsCancellationRequested && rawDataAccessor.InAcquisition)
                {
                    rawDataAccessor.RefreshViewOfFile();
                    var runHeader = detectorReader.RunHeaderEx;

                    if (runHeader.FirstSpectrum < 1)
                        continue;

                    var startSpectrum = GetStartAndEndSpectrumNumbers(
                        nextStartSpectrum,
                        runHeader,
                        out var endSpectrum
                    );

                    if (prevEndSpectrum == startSpectrum && prevEndSpectrum == endSpectrum)
                        continue;

                    UpdateSparklineDataBuffer(
                        detectorReader,
                        startSpectrum,
                        endSpectrum,
                        sparklineData
                    );
                    await call.RequestStream.WriteAsync(sparklineData);

                    // check response
                    await call.ResponseStream.MoveNext(cancellationToken: token);
                    var response = call.ResponseStream.Current;
                    _logger?.LogInformation(
                        $"StreamSparklineDataAsync (A): received from server: {response.Message}, Status: {response.Status}"
                    );

                    await Task.Delay(1000, token);

                    nextStartSpectrum = endSpectrum;
                    prevEndSpectrum = endSpectrum;
                }

                if (!rawDataAccessor.InAcquisition)
                {
                    detectorReader?.Dispose();
                    rawDataAccessor?.Dispose();
                    rawFileManager?.Dispose();

                    if (
                        !InitializeRawFileManager(
                            dataAccessContext,
                            out rawFileManager,
                            out rawDataAccessor,
                            out detectorReader
                        )
                    )
                    {
                        return;
                    }

                    var runHeader = detectorReader.RunHeaderEx;

                    var startSpectrum = GetStartAndEndSpectrumNumbers(
                        nextStartSpectrum,
                        runHeader,
                        out var endSpectrum
                    );

                    if (prevEndSpectrum == startSpectrum && prevEndSpectrum == endSpectrum)
                    {
                        _logger?.LogInformation(
                            $"StreamSparklineDataAsync (B): Done: {dataAccessContext.AcqSample.SampleId}"
                        );
                    }
                    else
                    {
                        UpdateSparklineDataBuffer(
                            detectorReader,
                            startSpectrum,
                            endSpectrum,
                            sparklineData
                        );
                        await call.RequestStream.WriteAsync(sparklineData);

                        // check response
                        await call.ResponseStream.MoveNext(cancellationToken: token);
                        var response = call.ResponseStream.Current;
                        _logger?.LogInformation(
                            $"StreamSparklineDataAsync: received from server: {response.Message}, Status: {response.Status}"
                        );
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger?.LogError("StreamSparklineDataAsync: shutdown requested.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"StreamSparklineDataAsync: {ex.Message}.");
            }
            finally
            {
                // Complete the stream
                await call.RequestStream.CompleteAsync();
            }
        }

        private async Task StreamChromatogramSvgDataAsync(
            RealTimePlotService.RealTimePlotServiceClient client,
            DataAccessContext dataAccessContext,
            CancellationToken token
        )
        {
            var call = client.StreamChromatogramSvgData(cancellationToken: token);

            try
            {
                if (
                    !InitializeRawFileManager(
                        dataAccessContext,
                        out var rawFileManager,
                        out var rawDataAccessor,
                        out var detectorReader
                    )
                )
                {
                    return;
                }

                var _chromatogramSvgGenerator = new ChromatogramSvgGenerator(_loggerFactory);
                var nextStartSpectrum = 0;
                var prevEndSpectrum = 0;

                var opalSeqId = dataAccessContext.GetOpalSequenceId?.Invoke(
                    dataAccessContext.SequenceId
                );
                var chromSvgData = new ChromatogramSvgData()
                {
                    SampleId = dataAccessContext.AcqSample.SampleId,
                    SequenceId = opalSeqId.ToString(),
                    StartScanNumber = 1,
                };

                while (!token.IsCancellationRequested && rawDataAccessor.InAcquisition)
                {
                    rawDataAccessor.RefreshViewOfFile();
                    var runHeader = detectorReader.RunHeaderEx;

                    if (runHeader.FirstSpectrum < 1)
                        continue;

                    var startSpectrum = GetStartAndEndSpectrumNumbers(
                        nextStartSpectrum,
                        runHeader,
                        out var endSpectrum
                    );

                    if (prevEndSpectrum == startSpectrum && prevEndSpectrum == endSpectrum)
                        continue;

                    var realtimePlotData = _chromatogramSvgGenerator.GetRealtimePlotDataSvg(
                        dataAccessContext,
                        opalSeqId.ToString(),
                        detectorReader,
                        1,
                        endSpectrum
                    );
                    chromSvgData.EndScanNumber = endSpectrum;
                    chromSvgData.ChromatogramSvg = realtimePlotData;

                    await call.RequestStream.WriteAsync(chromSvgData);

                    // check response
                    await call.ResponseStream.MoveNext(cancellationToken: token);
                    var response = call.ResponseStream.Current;
                    _logger?.LogInformation(
                        $"StreamChromatogramSvgDataAsync (A): received from server: {response.Message}, Status: {response.Status}"
                    );

                    await Task.Delay(1000, token);

                    nextStartSpectrum = endSpectrum;
                    prevEndSpectrum = endSpectrum;
                }

                if (!rawDataAccessor.InAcquisition)
                {
                    detectorReader?.Dispose();
                    rawDataAccessor?.Dispose();
                    rawFileManager?.Dispose();

                    if (
                        !InitializeRawFileManager(
                            dataAccessContext,
                            out rawFileManager,
                            out rawDataAccessor,
                            out detectorReader
                        )
                    )
                    {
                        return;
                    }

                    var runHeader = detectorReader.RunHeaderEx;

                    var startSpectrum = GetStartAndEndSpectrumNumbers(
                        nextStartSpectrum,
                        runHeader,
                        out var endSpectrum
                    );

                    if (prevEndSpectrum == startSpectrum && prevEndSpectrum == endSpectrum)
                    {
                        _logger?.LogInformation(
                            $"StreamChromatogramSvgDataAsync (B): Done: {dataAccessContext.AcqSample.SampleId}"
                        );
                    }
                    else
                    {
                        var realtimePlotData = _chromatogramSvgGenerator.GetRealtimePlotDataSvg(
                            dataAccessContext,
                            opalSeqId.ToString(),
                            detectorReader,
                            1,
                            endSpectrum
                        );

                        chromSvgData.EndScanNumber = endSpectrum;
                        chromSvgData.ChromatogramSvg = realtimePlotData;

                        await call.RequestStream.WriteAsync(chromSvgData);

                        // check response
                        await call.ResponseStream.MoveNext(cancellationToken: token);
                        var response = call.ResponseStream.Current;
                        _logger?.LogInformation(
                            $"StreamChromatogramSvgDataAsync: received from server: {response.Message}, Status: {response.Status}"
                        );
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger?.LogError("StreamChromatogramSvgDataAsync: shutdown requested.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"StreamChromatogramSvgDataAsync: {ex.Message}.");
            }
            finally
            {
                // Complete the stream
                await call.RequestStream.CompleteAsync();
            }
        }

        private bool InitializeRawFileManager(
            DataAccessContext dataAccessContext,
            out IRawFileThreadManager rawFileManager,
            out IRawDataExtended rawDataAccessor,
            out IDetectorReader detectorReader
        )
        {
            var ok = true;
            rawDataAccessor = null;
            detectorReader = null;
            rawFileManager = RawFileReaderAdapter.ThreadedFileFactory(
                dataAccessContext.AcqSample.RawFileNameFull
            );

            if (rawFileManager == null)
            {
                _logger?.LogError(
                    $"OpenFile: Error - RawFileThreadManager is null, file: {dataAccessContext.AcqSample.RawFileNameFull}"
                );
                ok = false;
            }
            else
            {
                rawDataAccessor = rawFileManager.CreateThreadAccessor();
                _logger?.LogInformation(
                    $"reader: IsOpen: {rawDataAccessor.IsOpen}, IsError: {rawDataAccessor.IsError}"
                );

                if (!rawDataAccessor.HasMsData)
                {
                    _logger?.LogError(
                        $"OpenFile: Error - There is no MS data, file: {dataAccessContext.AcqSample.RawFileNameFull}"
                    );
                    ok = false;
                }
                else
                {
                    detectorReader = rawDataAccessor.GetDetectorReader(
                        new InstrumentSelection(1, Device.MS)
                    );
                }
            }

            return ok;
        }

        private static void UpdateSparklineDataBuffer(
            IDetectorReader detectorReader,
            int startSpectrum,
            int endSpectrum,
            SparklineData sparklineData
        )
        {
            // Get chromatogram data, TIC
            IChromatogramSettings[] settings = { new ChromatogramTraceSettings() };
            var chromatogramData = detectorReader.GetChromatogramData(
                settings,
                startSpectrum,
                endSpectrum
            );
            sparklineData.EndScanNumber = endSpectrum;
            sparklineData.Intensities.AddRange(chromatogramData.IntensitiesArray[0]);
        }

        private static int GetStartAndEndSpectrumNumbers(
            int nextStartSpectrum,
            IRunHeader runHeader,
            out int endSpectrum
        )
        {
            var startSpectrum =
                nextStartSpectrum > runHeader.FirstSpectrum
                    ? nextStartSpectrum + 1
                    : runHeader.FirstSpectrum;

            if (startSpectrum > runHeader.LastSpectrum)
                startSpectrum = runHeader.LastSpectrum;

            endSpectrum = runHeader.LastSpectrum;
            return startSpectrum;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
        }
    }
}
