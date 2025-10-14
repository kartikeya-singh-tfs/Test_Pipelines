using Grpc.Core;
using System;
using ThermoFisher.AcquisitionModule.Contracts;

// Mock host that exposes BOTH lifecycle (Acquisition) and real-time plot (RealTimePlotService) gRPC services.
// This runs independently of the real Opal API host (which listens on 51640) so tests can target either:
//  - Real server:   localhost:51640
//  - Mock server:   localhost:50051 (default here)
// Adjust port via env var MOCK_ACQ_PORT if needed.
class Program
{
    public static void Main(string[] args)
    {
        var portEnv = Environment.GetEnvironmentVariable("MOCK_ACQ_PORT");
        int port = 50051;
        if (!string.IsNullOrWhiteSpace(portEnv) && int.TryParse(portEnv, out var parsed))
        {
            port = parsed;
        }

        var server = new Server
        {
            Services =
            {
                // Plot data upload mock (sparkline / chromatogram)
                RealTimePlotService.BindService(new MockAcquisitionService()),
                // Lifecycle acquisition mock (sequence / acquisition / device events + sample stream)
                Acquisition.BindService(new MockAcquisitionLifecycleService())
            },
            Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
        };

        server.Start();
        Console.WriteLine($"Mock Acquisition services listening on port {port}. Press ENTER to stop.");
        Console.WriteLine("Services: Acquisition, RealTimePlotService");
        Console.ReadLine();
        server.ShutdownAsync().Wait();
    }
}
