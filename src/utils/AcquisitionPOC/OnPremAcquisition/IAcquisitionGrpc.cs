using System;

namespace OnPremAcquisition
{
    public interface IAcquisitionGrpc : IDisposable
    {
        void Start();

        IAcquisitionAdapter GetAcquisitionAdapter();
    }
}
