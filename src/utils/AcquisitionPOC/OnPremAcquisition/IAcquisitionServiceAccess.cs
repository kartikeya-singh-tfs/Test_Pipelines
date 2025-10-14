using System;
using ThermoFisher.Foundation.Acquisition;

namespace OnPremAcquisition
{
    public interface IAcquisitionServiceAccess : IDisposable
    {
        DeviceStatusInfo[] Devices { get; }
        string[] GetDeviceNames();
        bool InsertSample(
            string instrumentName,
            Guid sequenceId,
            int index,
            AcquisitionSample acquisitionSample
        );
        bool IsQueueEmpty();
        bool RemoveSamples(string instrumentName, Guid sequenceId, Guid[] sampleIds);
        bool StopRun(string instrumentName);
        bool SubmitSequence(
            string instrumentName,
            AcquisitionSequence sequence,
            bool isHighPriority,
            bool enforceUser,
            CfrAuditData auditData
        );

        event Action<RunEventEventArgs> RunEventNotify;
        event Action<DeviceStatusInfo[]> DeviceStatusChanged;
        event Action<RunStateEventArgs> RunStateChanged;
        event Action<RunEventEventArgs, AcquisitionSample> SampleComplete;
    }
}
