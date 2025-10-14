using System;
using ThermoFisher.Foundation.Acquisition;

namespace Thermofisher.Opal.IpcDataAccessModule.DataAccess
{
    /// <summary>
    /// Interface for monitoring acquisition events and managing acquisition service connections.
    /// </summary>
    public interface IAcqMonitor : IDisposable
    {
        /// <summary>
        /// Event triggered when a sample is being acquired.
        /// </summary>
        event Action<RunStateEventArgs, AcquisitionSample> SampleAcquire;

        /// <summary>
        /// Event triggered when a sample acquisition is complete.
        /// </summary>
        event Action<RunEventEventArgs, AcquisitionSample> SampleComplete;

        /// <summary>
        /// Establishes a connection to the acquisition service.
        /// </summary>
        /// <param name="computerName">The name of the computer hosting the acquisition service.</param>
        /// <param name="tcpPort">The TCP port for the acquisition service connection.</param>
        /// <returns>True if the connection is established successfully; otherwise, false.</returns>
        bool EstablishAcquisitionServiceConnection(string computerName, int tcpPort);

        /// <summary>
        /// Subscribes to acquisition events.
        /// </summary>
        void Subscribe();
    }
}
