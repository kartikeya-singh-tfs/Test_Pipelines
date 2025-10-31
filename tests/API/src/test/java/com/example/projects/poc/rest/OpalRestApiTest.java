package com.example.projects.poc.rest;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.URL;
import java.security.SecureRandom;
import java.security.cert.X509Certificate;
import java.util.Arrays;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

import org.testng.Assert;
import org.testng.annotations.Test;

import io.restassured.RestAssured;
import io.restassured.response.Response;
import okhttp3.MediaType;
import okhttp3.OkHttpClient;
import okhttp3.Protocol;
import okhttp3.Request;
import okhttp3.RequestBody;

public class OpalRestApiTest {

    private static final String BASE_URL = "https://localhost:61350/api/acquisition/v1/sequence";
    private static final String SAMPLE_JSON = "{" +
        "\"id\": \"001a32b5-a010-4513-a355-c0d4a21edf7c\"," +
        "\"name\": \"Test\"," +
        "\"description\": \"\"," +
        "\"samples\": [" +
        "{" +
        "\"id\": \"61f6b9b7-7149-49b9-be33-207b2d9da941\"," +
        "\"name\": \"sample01\"," +
        "\"type\": \"Standard\"," +
        "\"methodFilePath\": \"C:\\\\TestOnPremAcq\\\\TestMethod.meth\"," +
        "\"rawFilePath\": \"C:\\\\TestOnPremAcq\\\\sample2.raw\"," +
        "\"volume\": 1," +
        "\"position\": \"1\"" +
        "}" +
        "]" +
        "}";

    @Test
    public void testSseWithEventSourceAndOkHttp() throws Exception {
        final String sseUrl = "https://localhost:61350/api/acquisition/v1/sseSparklineData";

        // Trust all SSL certs (self-signed support)
        TrustManager[] trustAllCerts = new TrustManager[]{
            new X509TrustManager() {
                @Override
                public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                @Override
                public void checkClientTrusted(X509Certificate[] certs, String authType) { }
                @Override
                public void checkServerTrusted(X509Certificate[] certs, String authType) { }
            }
        };
        SSLContext sc = SSLContext.getInstance("SSL");
        sc.init(null, trustAllCerts, new SecureRandom());

        OkHttpClient client = new OkHttpClient.Builder()
            .protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))
            .sslSocketFactory(sc.getSocketFactory(), (X509TrustManager) trustAllCerts[0])
            .hostnameVerifier((hostname, session) -> true)
            .build();

        java.util.List<String> sseEvents = java.util.Collections.synchronizedList(new java.util.ArrayList<>());

        // OkHttp SSE EventSource
        okhttp3.Request request = new okhttp3.Request.Builder()
            .url(sseUrl)
            .build();

        final CountDownLatch openLatch = new CountDownLatch(1);

        okhttp3.sse.EventSourceListener listener = new okhttp3.sse.EventSourceListener() {
            @Override
            public void onOpen(okhttp3.sse.EventSource eventSource, okhttp3.Response response) {
                openLatch.countDown();
            }

            @Override
            public void onEvent(okhttp3.sse.EventSource eventSource, String id, String type, String data) {
                sseEvents.add(data);
                System.out.println("[SSE] Event: " + data);
            }
            @Override
            public void onFailure(okhttp3.sse.EventSource eventSource, Throwable t, okhttp3.Response response) {
                System.err.println("[SSE] Error: " + t);
            }
        };

        okhttp3.sse.EventSource.Factory factory = okhttp3.sse.EventSources.createFactory(client);
        okhttp3.sse.EventSource eventSource = factory.newEventSource(request, listener);

        // Wait for SSE connection to open before triggering the POST
        boolean opened = openLatch.await(5, TimeUnit.SECONDS);
        if (!opened) {
            throw new IllegalStateException("SSE connection did not open within timeout");
        }

        // Trigger REST POST call using OkHttp
        System.out.println("[REST] Sending POST to trigger SSE event...");
        RequestBody body = RequestBody.create(SAMPLE_JSON, MediaType.parse("application/json"));
        Request postRequest = new Request.Builder()
            .url(BASE_URL)
            .post(body)
            .build();

        try (okhttp3.Response response = client.newCall(postRequest).execute()) {
            int code = response.code();
            String respBody = response.body() != null ? response.body().string() : "";
            System.out.println("[REST] POST Response code: " + code);
            System.out.println("[REST] POST Response body: " + respBody);
            Assert.assertEquals(code, 200);
        }

        // Wait for SSE events (e.g., 20s)
        Thread.sleep(20000);
        eventSource.cancel();

        // Print all recorded SSE events after test
        System.out.println("[SSE] Recorded events:");
        for (String event : sseEvents) {
            System.out.println("[SSE] " + event);
        }
    }

    @Test(groups = "Rest")
    public void testSseWithEventSourceAndRestAssured() throws Exception {
        final String sseUrl = "https://localhost:61350/api/acquisition/v1/sseSparklineData";

        // Trust all SSL certs (self-signed support)
        TrustManager[] trustAllCerts = new TrustManager[]{
            new X509TrustManager() {
                @Override
                public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                @Override
                public void checkClientTrusted(X509Certificate[] certs, String authType) { }
                @Override
                public void checkServerTrusted(X509Certificate[] certs, String authType) { }
            }
        };
        SSLContext sc = SSLContext.getInstance("SSL");
        sc.init(null, trustAllCerts, new SecureRandom());

        OkHttpClient client = new OkHttpClient.Builder()
            .protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))
            .sslSocketFactory(sc.getSocketFactory(), (X509TrustManager) trustAllCerts[0])
            .hostnameVerifier((hostname, session) -> true)
            .build();

        java.util.List<String> sseEvents = java.util.Collections.synchronizedList(new java.util.ArrayList<>());

        // OkHttp SSE EventSource
        okhttp3.Request request = new okhttp3.Request.Builder()
            .url(sseUrl)
            .build();

        final CountDownLatch openLatch = new CountDownLatch(1);

        okhttp3.sse.EventSourceListener listener = new okhttp3.sse.EventSourceListener() {
            @Override
            public void onOpen(okhttp3.sse.EventSource eventSource, okhttp3.Response response) {
                openLatch.countDown();
            }

            @Override
            public void onEvent(okhttp3.sse.EventSource eventSource, String id, String type, String data) {
                sseEvents.add(data);
                System.out.println("[SSE] Event: " + data);
            }
            @Override
            public void onFailure(okhttp3.sse.EventSource eventSource, Throwable t, okhttp3.Response response) {
                System.err.println("[SSE] Error: " + t);
            }
        };

        okhttp3.sse.EventSource.Factory factory = okhttp3.sse.EventSources.createFactory(client);
        okhttp3.sse.EventSource eventSource = factory.newEventSource(request, listener);

        // Wait for SSE connection to open before triggering the POST
        boolean opened = openLatch.await(5, TimeUnit.SECONDS);
        if (!opened) {
            throw new IllegalStateException("SSE connection did not open within timeout");
        }

        // Trigger REST POST call using RestAssured (with relaxed HTTPS validation)
    System.out.println("[REST] Sending POST to trigger SSE event (RestAssured)...");
    // Configure RestAssured to use the same SSLContext as OkHttp to avoid TLS/ALPN footprint differences
    RestAssuredSslConfigurer.configure(sc);
    RestAssured.useRelaxedHTTPSValidation();

        Response response = RestAssured
                .given()
                .header("Content-Type", "application/json")
                .body(SAMPLE_JSON)
                .post(BASE_URL);

        Assert.assertEquals(response.getStatusCode(), 200);
        System.out.println("RestAssured POST Response: " + response.getBody().asString());

        // Wait for SSE events (e.g., 20s)
        Thread.sleep(20000);
        eventSource.cancel();

        // Print all recorded SSE events after test
        System.out.println("[SSE] Recorded events:");
        for (String event : sseEvents) {
            System.out.println("[SSE] " + event);
        }
    }
    @Test
    public void testSseAfterRestPost() throws Exception {
    final String sessionId = "001a32b5-a010-4513-a355-c0d4a21edf7c"; // Use the same as in SAMPLE_JSON
    final String sseUrl = "https://localhost:61350/api/acquisition/v1/sseSparklineData";

        // Trust all SSL certs (self-signed support)
        TrustManager[] trustAllCerts = new TrustManager[]{
            new X509TrustManager() {
                public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                public void checkClientTrusted(X509Certificate[] certs, String authType) { }
                public void checkServerTrusted(X509Certificate[] certs, String authType) { }
            }
        };
        SSLContext sc = SSLContext.getInstance("SSL");
        sc.init(null, trustAllCerts, new SecureRandom());

        // List to record SSE events
        java.util.List<String> sseEvents = java.util.Collections.synchronizedList(new java.util.ArrayList<>());

        // SSE Thread
        Thread sseThread = new Thread(() -> {
            try {
                URL url = new URL(sseUrl);
                HttpsURLConnection conn = (HttpsURLConnection) url.openConnection();
                conn.setSSLSocketFactory(sc.getSocketFactory());
                conn.setHostnameVerifier((hostname, session) -> true);
                conn.setRequestMethod("GET");
                conn.setRequestProperty("Accept", "text/event-stream");

                try (BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()))) {
                    String line;
                    long startTime = System.currentTimeMillis();
                    long lastLog = startTime;

                    while ((System.currentTimeMillis() - startTime < 20000) && !Thread.currentThread().isInterrupted()) {
                        if ((line = reader.readLine()) != null && !line.trim().isEmpty()) {
                            if (line.startsWith("data:")) {
                                sseEvents.add(line.substring(5).trim());
                            }
                            System.out.println("[SSE] Event: " + line);
                        }
                        long now = System.currentTimeMillis();
                        if (now - lastLog >= 2000) {
                            System.out.println("[SSE] ...still listening...");
                            lastLog = now;
                        }
                        Thread.sleep(100);
                    }
                }
                conn.disconnect();
            } catch (java.net.SocketException se) {
                System.out.println("[SSE] Connection closed by server (normal end).");
            } catch (Exception e) {
                System.err.println("[SSE] Error: " + e);
            }
        });

        sseThread.start();
        Thread.sleep(1000); // Allow SSE connection to establish

        // Trigger REST POST call using OkHttp
        System.out.println("[REST] Sending POST to trigger SSE event...");
        OkHttpClient client = new OkHttpClient.Builder()
            .protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))
            .sslSocketFactory(sc.getSocketFactory(), (X509TrustManager) trustAllCerts[0])
            .hostnameVerifier((hostname, session) -> true)
            .build();

        RequestBody body = RequestBody.create(SAMPLE_JSON, MediaType.parse("application/json"));
        Request request = new Request.Builder()
            .url(BASE_URL)
            .post(body)
            .build();

        try (okhttp3.Response response = client.newCall(request).execute()) {
            int code = response.code();
            String respBody = response.body() != null ? response.body().string() : "";
            System.out.println("[REST] POST Response code: " + code);
            System.out.println("[REST] POST Response body: " + respBody);
            Assert.assertEquals(code, 200);
        }

        sseThread.join(21000); // Wait max 21s
        if (sseThread.isAlive()) {
            sseThread.interrupt();
            System.out.println("[SSE] Thread timed out and interrupted");
        }

        // Print all recorded SSE events after test
        System.out.println("[SSE] Recorded events:");
        for (String event : sseEvents) {
            System.out.println("[SSE] " + event);
        }
    }

    @Test
    public void testSubmitSampleWithRestAssured() {
        RestAssured.useRelaxedHTTPSValidation();

        Response response = RestAssured
                .given()
                .header("Content-Type", "application/json")
                .body(SAMPLE_JSON)
                .post(BASE_URL);

        Assert.assertEquals(response.getStatusCode(), 200);
        System.out.println("RestAssured POST Response: " + response.getBody().asString());
    }

    @Test
    public void testSubmitSampleWithOkHttp() throws Exception {
    // Trust all SSL certs (self-signed support)
    TrustManager[] trustAllCerts = new TrustManager[]{
        new X509TrustManager() {
        public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
        public void checkClientTrusted(X509Certificate[] certs, String authType) { }
        public void checkServerTrusted(X509Certificate[] certs, String authType) { }
        }
    };
    SSLContext sc = SSLContext.getInstance("SSL");
    sc.init(null, trustAllCerts, new SecureRandom());

    OkHttpClient client = new OkHttpClient.Builder()
        .protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))
        .sslSocketFactory(sc.getSocketFactory(), (X509TrustManager) trustAllCerts[0])
        .hostnameVerifier((hostname, session) -> true)
        .build();

    RequestBody body = RequestBody.create(SAMPLE_JSON, MediaType.parse("application/json"));

    Request request = new Request.Builder()
        .url(BASE_URL)
        .post(body)
        .build();

    try (okhttp3.Response response = client.newCall(request).execute()) {
        int code = response.code();
        String respBody = response.body() != null ? response.body().string() : "";
        System.out.println("OkHttp POST Response code: " + code);
        System.out.println("OkHttp POST Response body: " + respBody);
        Assert.assertEquals(code, 200);
    }
    }

    @Test
    public void testSseAfterRestCall() throws Exception {
        final String sseUrl = "https://localhost:61350/api/acquisition/v1/sse";

        // Trust all SSL certs (self-signed support)
        TrustManager[] trustAllCerts = new TrustManager[]{
                new X509TrustManager() {
                    public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                    public void checkClientTrusted(X509Certificate[] certs, String authType) { }
                    public void checkServerTrusted(X509Certificate[] certs, String authType) { }
                }
        };
        SSLContext sc = SSLContext.getInstance("SSL");
        sc.init(null, trustAllCerts, new SecureRandom());

        // SSE Thread
        Thread sseThread = new Thread(() -> {
            try {
                URL url = new URL(sseUrl);
                HttpsURLConnection conn = (HttpsURLConnection) url.openConnection();
                conn.setSSLSocketFactory(sc.getSocketFactory());
                conn.setHostnameVerifier((hostname, session) -> true);
                conn.setRequestMethod("GET");
                conn.setRequestProperty("Accept", "text/event-stream");

                try (BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()))) {
                    String line;
                    long startTime = System.currentTimeMillis();
                    long lastLog = startTime;

                    while ((System.currentTimeMillis() - startTime < 20000) && !Thread.currentThread().isInterrupted()) {
                        if ((line = reader.readLine()) != null && !line.trim().isEmpty()) {
                            System.out.println("[SSE] Event: " + line);
                        }

                        long now = System.currentTimeMillis();
                        if (now - lastLog >= 2000) {
                            System.out.println("[SSE] ...still listening...");
                            lastLog = now;
                        }

                        Thread.sleep(100);
                    }
                }

                conn.disconnect();
            } catch (Exception e) {
                System.err.println("[SSE] Error: " + e);
            }
        });

        sseThread.start();
        Thread.sleep(1000); // Allow SSE connection to establish

        // Simulate gRPC call to send SequenceStateEvent
        System.out.println("[gRPC] Sending SequenceStateEvent...");
        try {
            io.grpc.ManagedChannel channel = io.grpc.ManagedChannelBuilder.forAddress("localhost", 61350)
                    .usePlaintext()
                    .build();

            ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc.AcquisitionBlockingStub stub =
                    ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc.newBlockingStub(channel);

            ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent event =
                    ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent.newBuilder()
                            .setSequenceId("3425f96a-f478-4bb6-a7d2-53f2c9719ed8")
                            .setSequenceState(
                                    ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceState.SequenceComplete)
                            .setIsError(false)
                            .build();

            stub.sequenceStateEvent(event);
            channel.shutdown();
            System.out.println("[gRPC] SequenceStateEvent sent");
        } catch (Exception e) {
            System.err.println("[gRPC] Error sending SequenceStateEvent: " + e);
        }

        sseThread.join(21000); // Wait max 21s
        if (sseThread.isAlive()) {
            sseThread.interrupt();
            System.out.println("[SSE] Thread timed out and interrupted");
        }
    }
}
