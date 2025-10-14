using System;
using ThermoFisher.Foundation.Acquisition;

namespace OnPremAcquisition
{
    public interface IAcquisitionAdapter : IDisposable
    {
        bool SubmitSequence(SequenceData sequenceData);
        Guid GetOpalSequenceIdForFoundationSequenceId(Guid foundationSequenceId);

        event Action<RunEventEventArgs> RunEventNotify;
        event Action<DeviceStatusInfo[]> DeviceStatusChanged;
        event Action<RunStateEventArgs> RunStateChanged;
        event Action<RunEventEventArgs, AcquisitionSample> SampleComplete;
    }
}
