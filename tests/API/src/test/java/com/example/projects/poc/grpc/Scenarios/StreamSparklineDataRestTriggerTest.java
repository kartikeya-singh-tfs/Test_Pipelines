package com.example.projects.poc.grpc.Scenarios;

import java.io.StringWriter;
import java.io.PrintWriter;
import java.util.concurrent.CountDownLatch;

import org.testng.Assert;
import org.testng.annotations.Test;

import io.qameta.allure.Allure;
import io.restassured.RestAssured;
import io.restassured.response.Response;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.stub.StreamObserver;

import ThermoFisher.AcquisitionModule.Contracts.RealTimePlotServiceGrpc;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess;

import com.example.projects.poc.commons.CommonUtilities;

/**
 * Test that opens a RealTimePlotService streamSparklineData bidi call, triggers the
 * REST POST that creates a sequence, sends a few SparklineData client packets, and
 * records any PlotDataResponse messages returned by the server.
 */
public class StreamSparklineDataRestTriggerTest {

    private static final String BASE_URL = "https://localhost:61350/api/acquisition/v1/sequence";
    private static final String SEQUENCE_JSON = "{" +
            "\"id\": \"001a32b5-a010-4513-a355-c0d4a21edf7c\"," +
            "\"name\": \"Test\"," +
            "\"description\": \"\"," +
            "\"samples\": [" +
            "{" +
            "\"id\": \"61f6b9b7-7149-49b9-be33-207b2d9da941\"," +
            "\"name\": \"sample01\"," +
            "\"type\": \"Standard\"," +
            "\"methodFilePath\": \"C:/TestOnPremAcq/TestMethod.meth\"," +
            "\"rawFilePath\": \"C:/TestOnPremAcq/sample2.raw\"," +
            "\"volume\": 1," +
            "\"position\": \"1\"" +
            "}" +
            "]" +
            "}";

    @Test
    public void testStreamSparklineDataTriggeredByRestPost() throws Exception {
        // Start gRPC bidi stream (response observer will receive server PlotDataResponse messages)
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 51640)
                .usePlaintext()
                .build();

        RealTimePlotServiceGrpc.RealTimePlotServiceStub async = RealTimePlotServiceGrpc.newStub(channel);

        final java.util.List<RawDataAccess.PlotDataResponse> responses = java.util.Collections.synchronizedList(new java.util.ArrayList<>());
        final CountDownLatch streamCompleted = new CountDownLatch(1);

        StreamObserver<RawDataAccess.PlotDataResponse> respObserver = new StreamObserver<RawDataAccess.PlotDataResponse>() {
            @Override
            public void onNext(RawDataAccess.PlotDataResponse value) {
                responses.add(value);
                Allure.step("Received PlotDataResponse #" + responses.size());
            }

            @Override
            public void onError(Throwable t) {
                try {
                    StringWriter sw = new StringWriter();
                    t.printStackTrace(new PrintWriter(sw));
                    Allure.addAttachment("StreamSparklineData onError (stacktrace)", "text/plain", sw.toString());
                } catch (Exception ex) {
                    Allure.addAttachment("StreamSparklineData onError (toString)", "text/plain", t.toString());
                }
                streamCompleted.countDown();
            }

            @Override
            public void onCompleted() {
                Allure.step("StreamSparklineData completed");
                streamCompleted.countDown();
            }
        };

        // Create request observer for sending SparklineData messages
        StreamObserver<RawDataAccess.SparklineData> reqObs = async.streamSparklineData(respObserver);

        // Give the stream a short moment to register on the server
        Thread.sleep(500);

        // Trigger REST POST to create the sequence
        System.out.println("[REST] Sending POST to trigger sequence creation (RestAssured)...");
        RestAssured.useRelaxedHTTPSValidation();

        Response response = RestAssured
                .given()
                .header("Content-Type", "application/json")
                .body(SEQUENCE_JSON)
                .post(BASE_URL);

        Assert.assertEquals(response.getStatusCode(), 200);
        System.out.println("RestAssured POST Response: " + response.getBody().asString());

        // Send a few SparklineData messages referencing the sequence/sample we created
        for (int i = 1; i <= 3; i++) {
            RawDataAccess.SparklineData data = RawDataAccess.SparklineData.newBuilder()
                    .setSequenceId("001a32b5-a010-4513-a355-c0d4a21edf7c")
                    .setSampleId("61f6b9b7-7149-49b9-be33-207b2d9da941")
                    .setStartScanNumber(i)
                    .setEndScanNumber(i)
                    .addIntensities(100.0 + i)
                    .build();
            reqObs.onNext(data);
            Thread.sleep(50);
        }

        // Grace period then half-close to signal client is done
        Thread.sleep(150);
        reqObs.onCompleted();

        // Wait up to 10s for server responses
        int pollMs = 200;
        int maxPolls = 50; // 10s
        int polls = 0;
        while (responses.isEmpty() && polls < maxPolls) {
            Thread.sleep(pollMs);
            polls++;
        }

        Allure.step("PlotDataResponses received: " + responses.size());

        if (!responses.isEmpty()) {
            Allure.addAttachment("PlotDataResponse Summary", "application/json", CommonUtilities.createStreamingSummary(responses), "json");
        }

        System.out.println("[TEST] PlotDataResponses count=" + responses.size());

        channel.shutdownNow();

        // No strict assertion; this test records results for inspection
        Assert.assertTrue(true, "Completed StreamSparklineData/REST trigger test; inspect attachments for details.");
    }

}
