package com.example.grpc;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import org.testng.Assert;
import org.testng.annotations.Test;
// Import generated gRPC stubs after codegen
import ThermoFisher.AcquisitionModule.Contracts.RealTimePlotServiceGrpc;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData;

public class RawDataAccessServiceTest {
    @Test
        public void testUploadSparklineData() {
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 51640)
            .usePlaintext()
            .build();
        RealTimePlotServiceGrpc.RealTimePlotServiceBlockingStub stub = RealTimePlotServiceGrpc.newBlockingStub(channel);
        SparklineData request = SparklineData.newBuilder()
            .setSequenceId("seq1")
            .setSampleId("sample1")
            .setStartScanNumber(1)
            .setEndScanNumber(10)
            .addIntensities(123.45)
            .build();
        PlotDataResponse response = stub.uploadSparklineData(request);
        Assert.assertNotNull(response);
        System.out.println("UploadSparklineData response: " + response);
        channel.shutdown();
    }

    @Test
        public void testUploadChromatogramSvgData() {
                ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 51640)
            .usePlaintext()
            .build();
        RealTimePlotServiceGrpc.RealTimePlotServiceBlockingStub stub = RealTimePlotServiceGrpc.newBlockingStub(channel);
        ChromatogramSvgData request = ChromatogramSvgData.newBuilder()
            .setSequenceId("seq2")
            .setSampleId("sample2")
            .setStartScanNumber(5)
            .setEndScanNumber(15)
            .setChromatogramSvg("<svg></svg>")
            .build();
        PlotDataResponse response = stub.uploadChromatogramSvgData(request);
        Assert.assertNotNull(response);
        System.out.println("UploadChromatogramSvgData response: " + response);
        channel.shutdown();
    }
}
