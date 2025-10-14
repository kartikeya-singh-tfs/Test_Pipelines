
package com.example.grpc;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import org.testng.Assert;
import org.testng.annotations.Test;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse;

public class MockAcquisitionServiceTest {
    @Test
    public void testDeviceStateEvent() {
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 50051)
            .usePlaintext()
            .build();
        // Uncomment and use the generated gRPC stubs
        AcquisitionGrpc.AcquisitionBlockingStub stub = AcquisitionGrpc.newBlockingStub(channel);
        DeviceEvent request = DeviceEvent.newBuilder().build();
        EmptyResponse response = stub.deviceStateEvent(request);
        Assert.assertNotNull(response);
        System.out.println("MockAcquisitionService response: " + response);
        channel.shutdown();
    }
}
