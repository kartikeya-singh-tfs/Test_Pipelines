package com.example.projects.poc.commons;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.security.SecureRandom;
import java.security.cert.X509Certificate;
import okhttp3.OkHttpClient;
import okhttp3.Protocol;
import java.time.Duration;
import java.util.Arrays;
import java.util.ArrayList;
import java.util.List;
import com.google.protobuf.Message;
import com.google.protobuf.util.JsonFormat;
import io.qameta.allure.Allure;
import io.grpc.stub.StreamObserver;
import java.io.StringWriter;
import java.io.PrintWriter;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
// Generated classes from acquisition.proto
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass;

/**
 * Small collection of test utilities.
 */
public final class CommonUtilities {
    private CommonUtilities() {}

    /**
     * Returns a TrustManager[] that trusts all certificates (for tests only).
     */
    public static TrustManager[] trustAllCerts() {
        return new TrustManager[]{
                new X509TrustManager() {
                    public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                    public void checkClientTrusted(X509Certificate[] certs, String authType) { }
                    public void checkServerTrusted(X509Certificate[] certs, String authType) { }
                }
        };
    }

    /**
     * Create an SSLContext initialized to trust all certificates (tests only).
     */
    public static SSLContext sslContextTrustAll() {
        try {
            SSLContext sc = SSLContext.getInstance("SSL");
            sc.init(null, trustAllCerts(), new SecureRandom());
            return sc;
        } catch (Exception e) {
            throw new RuntimeException("Failed to init SSL context", e);
        }
    }

    /**
     * Convenience: build an OkHttpClient that uses the permissive SSL context and trust manager.
     * Strictly for tests/local environments only.
     */
    public static OkHttpClient okHttpClientTrustAll() {
        SSLContext sc = sslContextTrustAll();
        return new OkHttpClient.Builder()
                .protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))
                .sslSocketFactory(sc.getSocketFactory(), (X509TrustManager) trustAllCerts()[0])
                .hostnameVerifier((h, s) -> true)
                .connectTimeout(Duration.ofSeconds(10))
                .readTimeout(Duration.ofSeconds(60))
                .build();
    }

    /**
     * Create a compact JSON summary of a list of streaming responses.
     * If items are protobuf Messages they will be serialized to JSON using JsonFormat.
     * The method snapshots the provided list to avoid concurrent-modification issues.
     *
     * @param responses list of streaming response objects (may include protobuf Message instances)
     * @return JSON string summarizing the responses
     */
    public static String createStreamingSummary(List<?> responses) {
        if (responses == null) return "{ \"totalResponses\": 0, \"responses\": [] }";

        // Snapshot to avoid iterating while the list may be concurrently modified
        List<Object> snapshot;
        synchronized (responses) {
            snapshot = new ArrayList<>(responses);
        }

        StringBuilder sb = new StringBuilder();
        sb.append("{\n  \"totalResponses\": ").append(snapshot.size()).append(",\n  \"responses\": [\n");

        JsonFormat.Printer printer = JsonFormat.printer().omittingInsignificantWhitespace();

        for (int i = 0; i < snapshot.size(); i++) {
            if (i > 0) sb.append(",\n");
            Object item = snapshot.get(i);
            sb.append("    {\n      \"index\": ").append(i + 1).append(",\n      \"type\": \"")
                    .append(item == null ? "null" : item.getClass().getSimpleName()).append("\",")
                    .append("\n      \"payload\": ");

            try {
                if (item instanceof Message) {
                    String json = printer.print((Message) item);
                    sb.append(json);
                } else {
                    // Fallback to toString (escaped minimally)
                    String s = String.valueOf(item);
                    // crude escaping: wrap in quotes and replace newlines
                    s = s.replace("\\", "\\\\").replace("\"", "\\\"").replace("\n", "\\n");
                    sb.append("\"").append(s).append("\"");
                }
            } catch (Exception e) {
                sb.append("\"<serialization-error>\"");
            }

            sb.append("\n    }");
        }

        sb.append("\n  ]\n}");
        return sb.toString();
    }

    /**
     * Create a compact JSON summary from a list of JSON payload strings.
     * Each provided string is expected to be a JSON object/array or plain text. When
     * a payload looks like JSON (starts with '{' or '[') it will be embedded raw.
     * Otherwise it will be quoted as a string.
     *
     * This is useful when captures already contain serialized protobuf JSON and
     * you want a single attachment that preserves the original JSON structure.
     */
    public static String createStreamingSummaryFromJsonStrings(List<String> jsonPayloads) {
        if (jsonPayloads == null) return "{ \"totalResponses\": 0, \"responses\": [] }";

        List<String> snapshot;
        synchronized (jsonPayloads) {
            snapshot = new ArrayList<>(jsonPayloads);
        }

        StringBuilder sb = new StringBuilder();
        sb.append("{\n  \"totalResponses\": ").append(snapshot.size()).append(",\n  \"responses\": [\n");

        for (int i = 0; i < snapshot.size(); i++) {
            if (i > 0) sb.append(",\n");
            String item = snapshot.get(i);
            sb.append("    {\n      \"index\": ").append(i + 1).append(",\n      \"payload\": ");

            if (item == null) {
                sb.append("null");
            } else {
                String trimmed = item.trim();
                if (trimmed.startsWith("{") || trimmed.startsWith("[")) {
                    // embed raw JSON
                    sb.append(trimmed);
                } else {
                    // quote and escape
                    String s = item.replace("\\", "\\\\").replace("\"", "\\\"").replace("\n", "\\n");
                    sb.append("\"").append(s).append("\"");
                }
            }

            sb.append("\n    }");
        }

        sb.append("\n  ]\n}");
        return sb.toString();
    }

    /**
     * Helper that mirrors the test-local setupServerStreaming behavior.
     * Starts an async server-stream subscription and returns a synchronized list
     * which will be populated by incoming SequenceReplyStream messages. The
     * returned list is expected to be polled by the caller; this method does
     * not block until stream completion.
     */
    public static java.util.List<AcquisitionOuterClass.SequenceReplyStream> setupServerStreaming(
            AcquisitionGrpc.AcquisitionStub async,
            AcquisitionOuterClass.EmptyRequest emptyRequest,
            int timeoutSeconds) {

        final java.util.List<AcquisitionOuterClass.SequenceReplyStream> streamResponses = java.util.Collections.synchronizedList(new java.util.ArrayList<>());
        final CountDownLatch streamLatch = new CountDownLatch(1);
        final CountDownLatch firstMessageLatch = new CountDownLatch(1);

        StreamObserver<AcquisitionOuterClass.SequenceReplyStream> streamObserver = new StreamObserver<AcquisitionOuterClass.SequenceReplyStream>() {
            @Override
            public void onNext(AcquisitionOuterClass.SequenceReplyStream value) {
                streamResponses.add(value);
                // signal that we've received at least one message
                firstMessageLatch.countDown();
                Allure.step("📨 Received streaming response #" + streamResponses.size());
            }

            @Override
            public void onError(Throwable t) {
                try {
                    StringWriter sw = new StringWriter();
                    t.printStackTrace(new PrintWriter(sw));
                    Allure.addAttachment("Streaming onError (stacktrace)", "text/plain", sw.toString());
                } catch (Exception ex) {
                    Allure.addAttachment("Streaming onError (toString)", "text/plain", t.toString());
                }

                try {
                    io.grpc.Status status = io.grpc.Status.fromThrowable(t);
                    Allure.addAttachment("gRPC status", "text/plain", status.toString());
                } catch (Exception ex) {
                    // ignore
                }

                Allure.step("❌ Streaming error: " + t.getMessage());
                streamLatch.countDown();
            }

            @Override
            public void onCompleted() {
                Allure.step("✅ Server streaming completed - received " + streamResponses.size() + " responses");
                streamLatch.countDown();
            }
        };

        new Thread(() -> {
            try {
                async.submitSampleStream(emptyRequest, streamObserver);
            } catch (Exception e) {
                Allure.step("❌ Streaming failed to start: " + e.getMessage());
            }
        }, "stream-submitter").start();

        try {
            firstMessageLatch.await(Math.max(1, timeoutSeconds), TimeUnit.SECONDS);
        } catch (InterruptedException ie) {
            Thread.currentThread().interrupt();
        }

        Allure.step("📊 Stream started (listening) - awaiting responses asynchronously");

        return streamResponses;
    }
}
