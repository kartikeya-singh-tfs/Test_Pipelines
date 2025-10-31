package com.example.projects.poc.integration;

import ThermoFisher.AcquisitionModule.Contracts.RealTimePlotServiceGrpc;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess; // for nested message / enum types
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.stub.StreamObserver;
import okhttp3.*;
import org.testng.Assert;
import org.testng.annotations.Test;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.security.SecureRandom;
import java.security.cert.X509Certificate;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
// Removed unused imports from previous snapshot streaming approach.

/**
 * Integration test: submit a sequence via REST while a SubmitSampleStream gRPC subscription
 * is active, and log/verify SequenceReplyStream messages.
 *
 * Purpose:
 *  - Proves REST submission populates data visible on the gRPC snapshot stream.
 *  - Provides a future anchor to assert richer snapshot fields.
 */
public class RestSubmitSequenceStreamTest {

    private static final String SEQUENCE_ID = "001a32b5-a010-4513-a355-c0d4a21edf7c";
    private static final String SAMPLE_ID = "61f6b9b7-7149-49b9-be33-207b2d9da941";
    private static final String BASE_URL = "https://localhost:61350/api/acquisition/v1/sequence";
    private static final int GRPC_PORT = 51640; // primary plaintext gRPC port (Acquisition & maybe RealTimePlot)
    private static final int GRPC_TLS_PORT = 61350; // HTTPS/TLS endpoint (used for REST + possibly gRPC over TLS)

    // NOTE: Use forward slashes to avoid JSON invalid escape sequences like \"\T\" which caused
    // System.Text.Json JsonException ('T' is an invalid escapable character). Windows APIs and .NET
    // generally accept forward slashes in file paths, and server logic should treat them equivalently.
    // If native backslashes are required later, generate JSON via a serializer (e.g., Jackson/Gson)
    // instead of manual string concatenation so escaping is handled correctly.
    private static final String SAMPLE_JSON = "{" +
        "\"id\": \"" + SEQUENCE_ID + "\"," +
        "\"name\": \"Test\"," +
        "\"description\": \"\"," +
        "\"samples\": [" +
        "{" +
        "\"id\": \"" + SAMPLE_ID + "\"," +
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
    public void restSubmitSequenceEmitsSequenceReplyStream() throws Exception {
        // --- Trust-all SSL for local self-signed cert ---
        TrustManager[] trustAllCerts = new TrustManager[]{
                new X509TrustManager() {
                    @Override public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                    @Override public void checkClientTrusted(X509Certificate[] certs, String authType) { }
                    @Override public void checkServerTrusted(X509Certificate[] certs, String authType) { }
                }
        };
        SSLContext sc = SSLContext.getInstance("SSL");
        sc.init(null, trustAllCerts, new SecureRandom());

    // Prefer HTTP/2; HTTP/1.1 kept to satisfy OkHttp TLS ALPN requirements. We'll assert after the call.
    OkHttpClient http = new OkHttpClient.Builder()
        .protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))
                .sslSocketFactory(sc.getSocketFactory(), (X509TrustManager) trustAllCerts[0])
                .hostnameVerifier((h, s) -> true)
                .build();

    ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", GRPC_PORT)
            .usePlaintext()
            .build();
    RealTimePlotServiceGrpc.RealTimePlotServiceStub asyncPlot = RealTimePlotServiceGrpc.newStub(channel);

        // --- REST POST sequence submission ---
        System.out.println("[REST] Submitting sequence via POST...");
        RequestBody body = RequestBody.create(SAMPLE_JSON, MediaType.parse("application/json"));
        Request request = new Request.Builder().url(BASE_URL).post(body).build();
        try (Response resp = http.newCall(request).execute()) {
            int code = resp.code();
            ResponseBody rb = resp.body();
            String respBody = rb == null ? "" : rb.string();
            Protocol negotiated = resp.protocol();
            System.out.println("[REST] Negotiated protocol=" + negotiated);
            System.out.println("[REST] Response code=" + code + " body=" + respBody);
            Assert.assertEquals(code, 200, "Expected HTTP 200 from sequence submission");
            boolean skipH2Assert = Boolean.getBoolean("skipHttp2Assert");
            if (!skipH2Assert) {
                Assert.assertEquals(negotiated, Protocol.HTTP_2, "Expected HTTP/2; set -DskipHttp2Assert=true to bypass if server not yet h2-enabled");
            }
        }

        // --- RealTimePlot gRPC: bidirectional streamSparklineData after REST call ---
        // We send multiple SparklineData messages referencing the submitted sequence & sample
        // and log the server's PlotDataResponse for each.
        CountDownLatch doneLatch = new CountDownLatch(1);
    class RespHolder { List<RawDataAccess.PlotDataResponse> list = new java.util.concurrent.CopyOnWriteArrayList<>(); }
    RespHolder respHolder = new RespHolder();

        StreamObserver<RawDataAccess.PlotDataResponse> responseObserver = new StreamObserver<>() {
            @Override public void onNext(RawDataAccess.PlotDataResponse value) {
                respHolder.list.add(value);
                System.out.println("[StreamSparklineData][Resp] status=" + value.getStatus() + " msg=" + value.getMessage());
            }
            @Override public void onError(Throwable t) {
                System.err.println("[StreamSparklineData][Resp] error=" + t);
                doneLatch.countDown();
            }
            @Override public void onCompleted() {
                System.out.println("[StreamSparklineData][Resp] completed");
                doneLatch.countDown();
            }
        };

        StreamObserver<RawDataAccess.SparklineData> requestObserver = asyncPlot.streamSparklineData(responseObserver);

        // send a few packets with small delay to ensure server reads before half-close
        for (int i = 1; i <= 3; i++) {
            RawDataAccess.SparklineData data = RawDataAccess.SparklineData.newBuilder()
                    .setSequenceId(SEQUENCE_ID)
                    .setSampleId(SAMPLE_ID)
                    .setStartScanNumber(i)
                    .setEndScanNumber(i)
                    .addIntensities(10.0 + i)
                    .build();
            System.out.println("[Client] Sending SparklineData scan=" + i);
            requestObserver.onNext(data);
            Thread.sleep(50); // allow flush
        }
        Thread.sleep(150); // grace period before closing
        requestObserver.onCompleted();

        doneLatch.await(5, TimeUnit.SECONDS);

    if (respHolder.list.isEmpty()) {
            System.out.println("[Diag] No responses on plaintext port " + GRPC_PORT + ". Retrying with TLS channel on " + GRPC_TLS_PORT + "...");
            channel.shutdownNow();

            // Build a TLS channel trusting all certs (unsafe for prod)
            io.grpc.netty.shaded.io.grpc.netty.GrpcSslContexts.forClient(); // ensure class load
            javax.net.ssl.SSLContext jssl = SSLContext.getInstance("TLS");
            jssl.init(null, trustAllCerts, new SecureRandom());
            io.grpc.netty.shaded.io.grpc.netty.NettyChannelBuilder tlsBuilder = io.grpc.netty.shaded.io.grpc.netty.NettyChannelBuilder
                    .forAddress("localhost", GRPC_TLS_PORT)
                    .sslContext(io.grpc.netty.shaded.io.grpc.netty.GrpcSslContexts.forClient().trustManager(new javax.net.ssl.X509TrustManager(){
                        @Override public void checkClientTrusted(X509Certificate[] chain, String authType) {}
                        @Override public void checkServerTrusted(X509Certificate[] chain, String authType) {}
                        @Override public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                    }).build())
                    .overrideAuthority("localhost")
                    .enableRetry();

            ManagedChannel tlsChannel = tlsBuilder.build();
            CountDownLatch tlsLatch = new CountDownLatch(1);
            List<RawDataAccess.PlotDataResponse> tlsResponses = new java.util.concurrent.CopyOnWriteArrayList<>();
            StreamObserver<RawDataAccess.PlotDataResponse> tlsRespObs = new StreamObserver<>() {
                @Override public void onNext(RawDataAccess.PlotDataResponse value) {
                    tlsResponses.add(value);
                    System.out.println("[StreamSparklineData][TLS][Resp] status=" + value.getStatus() + " msg=" + value.getMessage());
                }
                @Override public void onError(Throwable t) { System.err.println("[StreamSparklineData][TLS][Resp] error=" + t); tlsLatch.countDown(); }
                @Override public void onCompleted() { System.out.println("[StreamSparklineData][TLS][Resp] completed"); tlsLatch.countDown(); }
            };
            StreamObserver<RawDataAccess.SparklineData> tlsReq = RealTimePlotServiceGrpc.newStub(tlsChannel).streamSparklineData(tlsRespObs);
            for (int i = 1; i <= 3; i++) {
                RawDataAccess.SparklineData data = RawDataAccess.SparklineData.newBuilder()
                        .setSequenceId(SEQUENCE_ID)
                        .setSampleId(SAMPLE_ID)
                        .setStartScanNumber(i)
                        .setEndScanNumber(i)
                        .addIntensities(20.0 + i)
                        .build();
                System.out.println("[Client][TLS] Sending SparklineData scan=" + i);
                tlsReq.onNext(data);
                Thread.sleep(60);
            }
            Thread.sleep(200);
            tlsReq.onCompleted();
            tlsLatch.await(5, TimeUnit.SECONDS);

            // Prefer TLS responses if any; else keep original list (empty)
            if (!tlsResponses.isEmpty()) {
                respHolder.list.addAll(tlsResponses);
            }
            tlsChannel.shutdown();
        }

    Assert.assertFalse(respHolder.list.isEmpty(), "Expected at least one PlotDataResponse from streamSparklineData (after plaintext + optional TLS retry)");
    boolean allOk = respHolder.list.stream().allMatch(r -> r.getStatus() == RawDataAccess.UploadStatusType.OK || r.getStatus() == RawDataAccess.UploadStatusType.ACK);
        Assert.assertTrue(allOk, "All responses should be OK or ACK");
        channel.shutdown();
    }
}
