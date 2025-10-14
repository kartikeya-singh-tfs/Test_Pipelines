package com.example.grpc;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import org.testng.Assert;
import org.testng.annotations.Test;

// Import your generated gRPC classes
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc;
// import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceData;
// import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SampleData;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse;

public class OpalGrpcTest {
    @Test
    public void testSubmitSampleStream() {
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 61350)
                .usePlaintext()
                .build();

        AcquisitionGrpc.AcquisitionBlockingStub stub = AcquisitionGrpc.newBlockingStub(channel);

        ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest request = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest.newBuilder().build();
        java.util.Iterator<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream> responseStream = stub.submitSampleStream(request);

        Assert.assertNotNull(responseStream);
        while (responseStream.hasNext()) {
            ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream reply = responseStream.next();
            System.out.println("Received SequenceReplyStream: " + reply);
        }

        channel.shutdown();
    }
}
