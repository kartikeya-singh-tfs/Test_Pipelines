package com.example.projects.poc.grpc.Scenarios;

import okhttp3.OkHttpClient;
import okhttp3.Protocol;
import okhttp3.Request;
import okhttp3.sse.EventSource;
import okhttp3.sse.EventSourceListener;
import okhttp3.sse.EventSources;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
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

import ThermoFisher.AcquisitionModule.Contracts.RealTimePlotServiceGrpc;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse;
import ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData;

/**
 * Integration tests that correlate RealTimePlotService gRPC calls with their dedicated SSE endpoints:
 *  - /api/acquisition/v1/sseSparklineData
 *  - /api/acquisition/v1/sseChromatogramSvgData
 */
public class RealTimePlotSseIntegrationTest {

    private static final int GRPC_PORT = 51640; // same port used in other tests
    private static final String HTTPS_BASE = "https://localhost:61350"; // SSE endpoints are exposed over this port

    private static OkHttpClient trustAllClient(TrustManager[] trustAllCerts, SSLContext sc) {
        return new OkHttpClient.Builder()
            .protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))
            .sslSocketFactory(sc.getSocketFactory(), (X509TrustManager) trustAllCerts[0])
            .hostnameVerifier((h, s) -> true)
            .build();
    }

    private static SSLContext buildSsl(TrustManager[] trustAllCerts) {
        try {
            SSLContext sc = SSLContext.getInstance("SSL");
            sc.init(null, trustAllCerts, new SecureRandom());
            return sc;
        } catch (Exception e) {
            throw new RuntimeException("Failed to init SSL context", e);
        }
    }

    private static TrustManager[] trustAll() {
        return new TrustManager[]{
            new X509TrustManager() {
                public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                public void checkClientTrusted(X509Certificate[] certs, String authType) { }
                public void checkServerTrusted(X509Certificate[] certs, String authType) { }
            }
        };
    }

    @Test
    public void testUploadSparklineDataProducesSseEvent() throws Exception {
        String sseUrl = HTTPS_BASE + "/api/acquisition/v1/sseSparklineData";

        TrustManager[] trustAllCerts = trustAll();
        SSLContext sc = buildSsl(trustAllCerts);
        OkHttpClient client = trustAllClient(trustAllCerts, sc);

        List<String> events = java.util.Collections.synchronizedList(new java.util.ArrayList<>());
        CountDownLatch latch = new CountDownLatch(1);

        EventSourceListener listener = new EventSourceListener() {
            @Override
            public void onEvent(EventSource eventSource, String id, String type, String data) {
                events.add(data);
                System.out.println("[SSE Sparkline] " + data);
                if (data.contains("SparklineData")) {
                    latch.countDown();
                }
            }
            @Override
            public void onFailure(EventSource eventSource, Throwable t, okhttp3.Response response) {
                System.err.println("[SSE Sparkline] Error: " + t);
            }
        };

        EventSource eventSource = EventSources.createFactory(client)
            .newEventSource(new Request.Builder().url(sseUrl).build(), listener);

        Thread.sleep(400); // allow subscription

        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", GRPC_PORT)
            .usePlaintext()
            .build();
        RealTimePlotServiceGrpc.RealTimePlotServiceBlockingStub stub = RealTimePlotServiceGrpc.newBlockingStub(channel);

        SparklineData spark = SparklineData.newBuilder()
            .setSequenceId("seq-sse-1")
            .setSampleId("sample-sse-1")
            .setStartScanNumber(1)
            .setEndScanNumber(5)
            .addIntensities(100.1)
            .addIntensities(101.2)
            .build();

        PlotDataResponse resp = stub.uploadSparklineData(spark);
        System.out.println("[gRPC] uploadSparklineData ack: " + resp);

        boolean received = latch.await(5, TimeUnit.SECONDS);
        eventSource.cancel();
        channel.shutdown();

        System.out.println("[SSE Sparkline] Total received: " + events.size());
        if (!events.isEmpty()) System.out.println("[SSE Sparkline] First: " + events.get(0));

        Assert.assertTrue(received, "Expected at least one SparklineData SSE event");
        Assert.assertTrue(events.stream().anyMatch(e -> e.contains("\"SparklineData\"")), "Event should contain SparklineData payload");
    }

    @Test
    public void testUploadChromatogramDataProducesSseEvent() throws Exception {
        String sseUrl = HTTPS_BASE + "/api/acquisition/v1/sseChromatogramSvgData";

        TrustManager[] trustAllCerts = trustAll();
        SSLContext sc = buildSsl(trustAllCerts);
        OkHttpClient client = trustAllClient(trustAllCerts, sc);

        List<String> events = java.util.Collections.synchronizedList(new java.util.ArrayList<>());
        CountDownLatch latch = new CountDownLatch(1);

        EventSourceListener listener = new EventSourceListener() {
            @Override
            public void onEvent(EventSource eventSource, String id, String type, String data) {
                events.add(data);
                System.out.println("[SSE Chrom] " + data);
                if (data.contains("ChromatogramSvgData")) {
                    latch.countDown();
                }
            }
            @Override
            public void onFailure(EventSource eventSource, Throwable t, okhttp3.Response response) {
                System.err.println("[SSE Chrom] Error: " + t);
            }
        };

        EventSource eventSource = EventSources.createFactory(client)
            .newEventSource(new Request.Builder().url(sseUrl).build(), listener);

        Thread.sleep(400); // allow subscription

        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", GRPC_PORT)
            .usePlaintext()
            .build();
        RealTimePlotServiceGrpc.RealTimePlotServiceBlockingStub stub = RealTimePlotServiceGrpc.newBlockingStub(channel);

        ChromatogramSvgData chrom = ChromatogramSvgData.newBuilder()
            .setSequenceId("seq-chrom-1")
            .setSampleId("sample-chrom-1")
            .setStartScanNumber(10)
            .setEndScanNumber(20)
            .setChromatogramSvg("<svg><path d='M0 0 L10 10'/></svg>")
            .build();

        PlotDataResponse resp = stub.uploadChromatogramSvgData(chrom);
        System.out.println("[gRPC] uploadChromatogramSvgData ack: " + resp);

        boolean received = latch.await(5, TimeUnit.SECONDS);
        eventSource.cancel();
        channel.shutdown();

        System.out.println("[SSE Chrom] Total received: " + events.size());
        if (!events.isEmpty()) System.out.println("[SSE Chrom] First: " + events.get(0));

        Assert.assertTrue(received, "Expected at least one ChromatogramSvgData SSE event");
        Assert.assertTrue(events.stream().anyMatch(e -> e.contains("\"ChromatogramSvgData\"")), "Event should contain ChromatogramSvgData payload");
    }
}
