package com.example.projects.poc.grpc.Scenarios;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import org.testng.Assert;
import org.testng.annotations.Test;
import ThermoFisher.AcquisitionModule.Contracts.RealTimePlotServiceGrpc;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.UploadStatusType;

public class RealTimePlotServiceIntegrationTest {
    @Test
    public void testUploadSparklineData() {
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 50051)
            .usePlaintext()
            .build();
        try {
            RealTimePlotServiceGrpc.RealTimePlotServiceBlockingStub stub = RealTimePlotServiceGrpc.newBlockingStub(channel);
            // Build minimal SparklineData request
            SparklineData request = SparklineData.newBuilder()
                .setSequenceId("seq-123")
                .setSampleId("sample-456")
                .build();
            PlotDataResponse response = stub.uploadSparklineData(request);
            Assert.assertNotNull(response, "Response from UploadSparklineData should not be null");
            System.out.println("UploadSparklineData response: " + response);
            Assert.assertEquals(response.getStatus(), UploadStatusType.OK);
            Assert.assertEquals(response.getMessage(), "Mock upload successful");
        } finally {
            channel.shutdown();
        }
    }
}
