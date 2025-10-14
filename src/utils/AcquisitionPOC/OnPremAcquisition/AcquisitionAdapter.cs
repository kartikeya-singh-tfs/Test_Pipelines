using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ThermoFisher.Foundation.Acquisition;

namespace OnPremAcquisition
{
    public class AcquisitionAdapter : IAcquisitionAdapter
    {
        private static string _sequenceFolder;

        private ILogger _logger;
        private IAcquisitionServiceAccess _acquisitionServiceAccess;
        private ConcurrentDictionary<Guid, Tuple<SampleData, SampleData>> _idSampleData =
            new ConcurrentDictionary<Guid, Tuple<SampleData, SampleData>>();
        private ConcurrentDictionary<Guid, string> _seqIdFolder =
            new ConcurrentDictionary<Guid, string>();
        private ConcurrentDictionary<Guid, ConcurrentDictionary<string, string>> _seqIdMethods =
            new ConcurrentDictionary<Guid, ConcurrentDictionary<string, string>>();
        private ConcurrentDictionary<Guid, Guid> _foundationSeqToOpalSeqId =
            new ConcurrentDictionary<Guid, Guid>();

        private string _msSimulatorInputRawFile;
        private bool _copyFiles;

        public event Action<RunEventEventArgs> RunEventNotify = delegate { };
        public event Action<DeviceStatusInfo[]> DeviceStatusChanged = delegate { };
        public event Action<RunStateEventArgs> RunStateChanged = delegate { };
        public event Action<RunEventEventArgs, AcquisitionSample> SampleComplete = delegate { };

        static AcquisitionAdapter()
        {
            var commonAppDir = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData
            );
            _sequenceFolder = Path.Combine(
                commonAppDir,
                "Thermo Scientific",
                "Opal",
                "Acquisition",
                "Sequences"
            );
        }

        public AcquisitionAdapter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AcquisitionAdapter>();

            _acquisitionServiceAccess = new AcquisitionServiceAccess(loggerFactory);
            _acquisitionServiceAccess.RunEventNotify += OnRunEventNotify;
            _acquisitionServiceAccess.DeviceStatusChanged += OnDeviceStatusChanged;
            _acquisitionServiceAccess.RunStateChanged += OnRunStateChanged;
            _acquisitionServiceAccess.SampleComplete += OnSampleComplete;

            _msSimulatorInputRawFile = System.Configuration.ConfigurationManager.AppSettings[
                "MsSimulatorInputRawFile"
            ];
            _copyFiles = Convert.ToBoolean(
                System.Configuration.ConfigurationManager.AppSettings["CopyFiles"]
            );
        }

        public void Dispose()
        {
            _acquisitionServiceAccess.Dispose();
        }

        public bool SubmitSequence(SequenceData sequenceData)
        {
            _logger.LogInformation(
                "Submitting sequence: {SequenceId}, {SampleCount}",
                sequenceData.Id,
                sequenceData.Samples.Count
            );

            string seqPath = Path.Combine(_sequenceFolder, sequenceData.Id.ToString());
            Directory.CreateDirectory(seqPath);
            _seqIdFolder[sequenceData.Id] = seqPath;
            CopyMethodFiles(sequenceData);

            var sequence = GetAcquisitionSequence(sequenceData);
            return _acquisitionServiceAccess.SubmitSequence(null, sequence, false, false, null);
        }

        public Guid GetOpalSequenceIdForFoundationSequenceId(Guid foundationSequenceId)
        {
            return _foundationSeqToOpalSeqId.TryGetValue(
                foundationSequenceId,
                out var opalSequenceId
            )
                ? opalSequenceId
                : Guid.Empty;
        }

        private void CopyMethodFiles(SequenceData sequenceData)
        {
            _seqIdMethods[sequenceData.Id] = new ConcurrentDictionary<string, string>();
            var uniqueFiles = sequenceData.Samples.Select(sd => sd.MethodFilePath).Distinct();

            foreach (var item in uniqueFiles)
            {
                if (_copyFiles)
                {
                    string destFilePath = Path.Combine(
                        _seqIdFolder[sequenceData.Id],
                        Path.GetFileName(item)
                    );
                    _logger.LogInformation(
                        "Copying method file: {Source} to {Destination}",
                        item,
                        destFilePath
                    );
                    File.Copy(item, destFilePath, true);
                    _seqIdMethods[sequenceData.Id][item] = destFilePath;
                }
                else
                {
                    _seqIdMethods[sequenceData.Id][item] = item;
                }
            }
        }

        private AcquisitionSequence GetAcquisitionSequence(SequenceData sequenceData)
        {
            var seq = new AcquisitionSequence(
                sequenceData.Id.ToString(),
                GetAcquisitionSampleCollection(sequenceData),
                GetAcquisitionParameters()
            );
            _foundationSeqToOpalSeqId[seq.Id] = sequenceData.Id;
            return seq;
        }

        private AcquisitionParameters GetAcquisitionParameters()
        {
            return new AcquisitionParameters(
                Environment.UserName,
                true,
                DeviceOperatingMode.On,
                _acquisitionServiceAccess.GetDeviceNames(),
                0,
                string.Empty,
                null
            );
        }

        private AcquisitionSampleCollection GetAcquisitionSampleCollection(
            SequenceData sequenceData
        )
        {
            var retVal = new AcquisitionSampleCollection();

            sequenceData.Samples.ForEach(sd =>
            {
                var tempSD = GetTempSampleData(sequenceData.Id, sd);
                var sample = GetSample(tempSD);
                retVal.Add(sample);
                _idSampleData.TryAdd(sd.Id, new Tuple<SampleData, SampleData>(sd, tempSD));
            });

            return retVal;
        }

        private SampleData GetTempSampleData(Guid sequenceId, SampleData sampleData)
        {
            var tempSampleData = new SampleData
            {
                Id = sampleData.Id,
                SampleName = sampleData.SampleName,
                MethodFilePath = _seqIdMethods[sequenceId][sampleData.MethodFilePath],
                RawFilePath = Path.Combine(_seqIdFolder[sequenceId], _msSimulatorInputRawFile),
            };
            return tempSampleData;
        }

        private AcquisitionSample GetSample(SampleData sampleData)
        {
            var sample = new AcquisitionSample(sampleData.MethodFilePath, string.Empty, 0)
            {
                SampleName = sampleData.SampleName,
                Path = Path.GetDirectoryName(sampleData.RawFilePath),
                RawFileName = Path.GetFileName(sampleData.RawFilePath),
                Id = sampleData.Id,
                SampleId = sampleData.Id.ToString(),
            };

            return sample;
        }

        private void OnDeviceStatusChanged(DeviceStatusInfo[] obj)
        {
            _logger.LogInformation(
                "Device status changed: {Status}",
                string.Join(", ", obj.Select(d => $"{d.Name}: {d.Status}"))
            );
            DeviceStatusChanged(obj);
        }

        private void OnRunEventNotify(RunEventEventArgs args)
        {
            _logger.LogInformation(
                "Run event received: RunEvent: {RunEvent}, Sequence ID: {SequenceId}, Sample ID: {SampleId}",
                args.RunEvent.RunEvent,
                args.RunEvent.SequenceId,
                args.RunEvent.SampleId
            );
            RunEventNotify(GetRunEventEventArgsWithProperSequenceId(args));
        }

        private void OnRunStateChanged(RunStateEventArgs args)
        {
            _logger.LogInformation(
                "Run state changed: RunState: {State}, Sequence ID: {SequenceId}",
                args.RunState.RunState,
                args.RunState.SequenceId
            );
            RunStateChanged(GetRunStateEventArgsWithProperSequenceId(args));
        }

        private void OnSampleComplete(RunEventEventArgs args, AcquisitionSample sample)
        {
            _logger.LogInformation(
                "Sample complete: Sequence ID: {SequenceId} Sample ID: {SampleId}",
                args.RunEvent.SequenceId,
                args.RunEvent.SampleId
            );

            if (_idSampleData.TryGetValue(sample.Id, out var sampleDataTuple))
            {
                sampleDataTuple.Item2.RawFilePath = sample.RawFileNameFull;

                if (_copyFiles)
                {
                    _logger.LogInformation(
                        "Copying raw file from {Source} to {Destination}",
                        sampleDataTuple.Item2.RawFilePath,
                        sampleDataTuple.Item1.RawFilePath
                    );
                    File.Copy(
                        sampleDataTuple.Item2.RawFilePath,
                        sampleDataTuple.Item1.RawFilePath,
                        true
                    );
                }
            }

            SampleComplete(GetRunEventEventArgsWithProperSequenceId(args), sample);
        }

        private RunEventEventArgs GetRunEventEventArgsWithProperSequenceId(RunEventEventArgs args)
        {
            if (_foundationSeqToOpalSeqId.TryGetValue(args.RunEvent.SequenceId, out var opalSeqId))
            {
                var info = new RunEventInfo(
                    args.RunEvent.RunEvent,
                    args.RunEvent.IsError,
                    opalSeqId,
                    args.RunEvent.SampleId,
                    args.RunEvent.FailedDeviceNames
                );
                return new RunEventEventArgs(args.InstrumentName, info);
            }

            return args;
        }

        private RunStateEventArgs GetRunStateEventArgsWithProperSequenceId(RunStateEventArgs args)
        {
            if (_foundationSeqToOpalSeqId.TryGetValue(args.RunState.SequenceId, out var opalSeqId))
            {
                var info = new RunStateInfo(
                    args.RunState.RunState,
                    args.RunState.RunStateAlias,
                    opalSeqId,
                    args.RunState.SampleId
                );
                return new RunStateEventArgs(args.InstrumentName, info);
            }

            return args;
        }
    }
}
