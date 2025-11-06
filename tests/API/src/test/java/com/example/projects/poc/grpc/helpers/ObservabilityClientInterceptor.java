package com.example.projects.poc.grpc.helpers;

import com.fasterxml.jackson.databind.ObjectMapper;
import io.grpc.*;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.time.Instant;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.TimeUnit;

/**
 * Client-side gRPC interceptor that records outgoing calls into a JSONL file.
 *
 * Usage in tests:
 *  - ObservabilityClientInterceptor.setCurrentTestId("MyTest#name");
 *  - Channel intercepted = ClientInterceptors.intercept(channel, new ObservabilityClientInterceptor());
 *  - Create stubs from the intercepted channel.
 *
 * The output path can be set via system property `observed.calls` (defaults to observed_calls.jsonl).
 */
public class ObservabilityClientInterceptor implements ClientInterceptor {
    private static final ObjectMapper MAPPER = new ObjectMapper();
    private static final ThreadLocal<String> CURRENT_TEST = new ThreadLocal<>();
    private static final ThreadLocal<java.util.List<String>> CURRENT_SCENARIOS = ThreadLocal.withInitial(java.util.ArrayList::new);
    private static final ThreadLocal<Boolean> CURRENT_PAYLOAD_VALID = new ThreadLocal<>();
    private final String outPath;

    public ObservabilityClientInterceptor() {
        this.outPath = System.getProperty("observed.calls", "observed_calls.jsonl");
    }

    public static void setCurrentTestId(String id) {
        CURRENT_TEST.set(id);
    }

    public static void clearCurrentTestId() { CURRENT_TEST.remove(); }

    // Scenario helpers: tests can add short tags describing the scenario being exercised.
    public static void addScenarioTag(String tag) {
        if (tag == null) return;
        java.util.List<String> list = CURRENT_SCENARIOS.get();
        if (!list.contains(tag)) list.add(tag);
    }

    public static void clearScenarioTags() { CURRENT_SCENARIOS.remove(); }

    // Mark whether the current outgoing payload was validated by the test (true/false).
    // If unset, telemetry will emit null.
    public static void setPayloadValid(Boolean valid) { CURRENT_PAYLOAD_VALID.set(valid); }

    public static void clearPayloadValid() { CURRENT_PAYLOAD_VALID.remove(); }

    private void appendJson(Map<String, Object> obj) {
        try (FileWriter fw = new FileWriter(outPath, true);
             BufferedWriter bw = new BufferedWriter(fw)) {
            bw.write(MAPPER.writeValueAsString(obj));
            bw.newLine();
        } catch (IOException e) {
            // non-fatal: write to stderr
            e.printStackTrace();
        }
    }

    @Override
    public <ReqT, RespT> ClientCall<ReqT, RespT> interceptCall(
            MethodDescriptor<ReqT, RespT> method, CallOptions callOptions, Channel next) {

        final String fullMethod = method.getFullMethodName(); // "package.Service/Method"
        final String[] parts = fullMethod.split("/");
        final String service = parts.length > 0 ? parts[0] : "";
        final String methodName = parts.length > 1 ? parts[1] : "";
        final String streamingType = method.getType().toString();

        final long start = System.nanoTime();

        ClientCall<ReqT, RespT> delegate = next.newCall(method, callOptions);

        return new ForwardingClientCall.SimpleForwardingClientCall<ReqT, RespT>(delegate) {
            private int sentMessages = 0;
            private int receivedMessages = 0;

            @Override
            public void start(Listener<RespT> responseListener, Metadata headers) {
                // attach test id header if present
                String testId = CURRENT_TEST.get();
                if (testId != null && !testId.isEmpty()) {
                    Metadata.Key<String> key = Metadata.Key.of("x-test-id", Metadata.ASCII_STRING_MARSHALLER);
                    headers.put(key, testId);
                }

                super.start(new ForwardingClientCallListener.SimpleForwardingClientCallListener<RespT>(responseListener) {
                    @Override
                    public void onMessage(RespT message) {
                        receivedMessages++;
                        super.onMessage(message);
                    }

                    @Override
                    public void onClose(Status status, Metadata trailers) {
                        long latencyMs = TimeUnit.NANOSECONDS.toMillis(System.nanoTime() - start);
                        Map<String, Object> rec = new HashMap<>();
                        rec.put("timestamp", Instant.now().toString());
                        rec.put("testId", CURRENT_TEST.get());
                        rec.put("service", service);
                        rec.put("method", methodName);
                        // include any scenario tags and payload validity markers from the test
                        try {
                            java.util.List<String> scen = CURRENT_SCENARIOS.get();
                            if (scen != null && !scen.isEmpty()) rec.put("scenarios", scen);
                        } catch (Exception ex) {
                            // ignore
                        }
                        try {
                            Boolean pv = CURRENT_PAYLOAD_VALID.get();
                            if (pv != null) rec.put("payloadValid", pv);
                        } catch (Exception ex) {
                            // ignore
                        }
                        rec.put("streamingType", streamingType);
                        rec.put("status", status.getCode().name());
                        rec.put("latencyMs", latencyMs);
                        rec.put("messages", Map.of("sent", sentMessages, "received", receivedMessages));
                        // optional: peer information not available easily from client interceptor

                        appendJson(rec);
                        super.onClose(status, trailers);
                    }
                }, headers);
            }

            @Override
            public void sendMessage(ReqT message) {
                sentMessages++;
                super.sendMessage(message);
            }
        };
    }
}
