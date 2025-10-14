using System;
using Microsoft.Extensions.Logging;
using Thermofisher.Opal.IpcDataAccessModule.DataAccess;

namespace OnPremAcquisition
{
    internal class Program
    {
        private static IAcquisitionGrpc _grpc;

        static void Main(string[] args)
        {
            DataAccessModule dataAccessModule = null;

            try
            {
                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    //builder.AddDebug();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                _grpc = new AcquisitionGrpc(loggerFactory);
                dataAccessModule = new DataAccessModule(loggerFactory);
                dataAccessModule.RegisterGetSequenceIdCallback(
                    _grpc.GetAcquisitionAdapter().GetOpalSequenceIdForFoundationSequenceId
                );
                _grpc.Start();

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                dataAccessModule?.Dispose();
                _grpc.Dispose();
            }
        }
    }
}
