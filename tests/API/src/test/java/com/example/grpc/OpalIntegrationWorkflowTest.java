package com.example.grpc;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import org.testng.Assert;
import org.testng.annotations.Test;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse;

/**
 * Integration test: gRPC call -> OPAL server -> MockAcquisitionService response.
 * Assumes OPAL server and MockAcquisitionService are running and connected.
 */
public class OpalIntegrationWorkflowTest {
    @Test
    public void testDeviceStateEventIntegration() {
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 50051)
            .usePlaintext()
            .build();
        try {
            AcquisitionGrpc.AcquisitionBlockingStub stub = AcquisitionGrpc.newBlockingStub(channel);
            // Build minimal DeviceEvent request
            DeviceEvent request = DeviceEvent.newBuilder().build();
            EmptyResponse response = stub.deviceStateEvent(request);
            Assert.assertNotNull(response, "Response from MockAcquisitionService should not be null");
            System.out.println("Integration response: " + response);
        } finally {
            channel.shutdown();
        }
    }
}
