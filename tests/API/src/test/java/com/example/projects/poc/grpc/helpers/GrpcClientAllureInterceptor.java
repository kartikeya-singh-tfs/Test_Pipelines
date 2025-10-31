package com.example.projects.poc.grpc.helpers;

import io.grpc.*;
import io.grpc.ForwardingClientCall.SimpleForwardingClientCall;
import io.grpc.ForwardingClientCallListener;
import com.google.protobuf.Message;
import com.google.protobuf.util.JsonFormat;
import java.util.*;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.function.Function;

/**
 * Intercepts outgoing/incoming gRPC messages and buffers captures for later attachment to Allure.
 * Designed for tests: provides a small API so a TestNG listener can drain captures at test end.
 */
public class GrpcClientAllureInterceptor implements ClientInterceptor {
    @SuppressWarnings("unused")
    private final int maxStreamAttachments;
    private final int maxAttachmentSizeBytes;

    // shared buffer (synchronized list) holding captures until a listener drains them
    private static final List<Capture> CAPTURES = Collections.synchronizedList(new ArrayList<>());

    // optional redaction function (null = identity)
    private static volatile Function<String, String> redactor = null;

    public static void setRedactor(Function<String, String> r) {
        redactor = r;
    }

    public GrpcClientAllureInterceptor() {
        this(100, 64 * 1024); // defaults
    }

    public GrpcClientAllureInterceptor(int maxStreamAttachments, int maxAttachmentSizeBytes) {
        this.maxStreamAttachments = maxStreamAttachments;
        this.maxAttachmentSizeBytes = maxAttachmentSizeBytes;
    }

    // Capture model
    public static class Capture {
        public final long timestampMillis;
        public final String testId; // may be null if not available
        public final String rpcName;
        public final String direction; // REQUEST | RESPONSE | STATUS
        public final String payloadJson;
        public final Object payloadObject; // original object when available (e.g., protobuf Message)

        public Capture(long ts, String testId, String rpcName, String direction, String payloadJson, Object payloadObject) {
            this.timestampMillis = ts;
            this.testId = testId;
            this.rpcName = rpcName;
            this.direction = direction;
            this.payloadJson = payloadJson;
            this.payloadObject = payloadObject;
        }
    }

    /**
     * Drain and return captures associated with the supplied testId. Removes from the buffer.
     */
    public static List<Capture> drainCapturesForTest(String testId) {
        if (testId == null) return Collections.emptyList();
        List<Capture> out = new ArrayList<>();
        synchronized (CAPTURES) {
            Iterator<Capture> it = CAPTURES.iterator();
            while (it.hasNext()) {
                Capture c = it.next();
                if (testId.equals(c.testId)) {
                    out.add(c);
                    it.remove();
                }
            }
        }
        return out;
    }

    /**
     * Drain and return captures recorded since the supplied timestamp. Removes from the buffer.
     * Useful as a fallback when testId tagging is not available.
     */
    public static List<Capture> drainCapturesSince(long sinceMillis) {
        List<Capture> out = new ArrayList<>();
        synchronized (CAPTURES) {
            Iterator<Capture> it = CAPTURES.iterator();
            while (it.hasNext()) {
                Capture c = it.next();
                if (c.timestampMillis >= sinceMillis) {
                    out.add(c);
                    it.remove();
                }
            }
        }
        return out;
    }

    @Override
    public <ReqT, RespT> ClientCall<ReqT, RespT> interceptCall(
            MethodDescriptor<ReqT, RespT> method,
            CallOptions callOptions,
            Channel next) {

        final String methodName = method.getFullMethodName();

        ClientCall<ReqT, RespT> call = next.newCall(method, callOptions);

        return new SimpleForwardingClientCall<ReqT, RespT>(call) {
            final AtomicInteger inboundCounter = new AtomicInteger();
            final AtomicInteger outboundCounter = new AtomicInteger();
            // will be set at start() time - captures original test id for this call
            private volatile String callTestId = null;

            @Override
            public void start(Listener<RespT> responseListener, Metadata headers) {
                // capture test id at call start (reflection-based to avoid hard dependency)
                this.callTestId = tryReadTestContext();

                // wrap the response listener to capture onMessage/onClose; pass callTestId
                Listener<RespT> wrapped = new AllureClientCallListener<>(responseListener, methodName, inboundCounter, this.callTestId);
                super.start(wrapped, headers);
            }

            @Override
            public void sendMessage(ReqT message) {
                int n = outboundCounter.incrementAndGet();
                try {
                    bufferCapture(methodName, "Request#" + n, message, this.callTestId, "REQUEST");
                } catch (Exception ex) {
                    // swallow - buffering should not interrupt the RPC
                }
                super.sendMessage(message);
            }

            @Override
            public void halfClose() {
                super.halfClose();
            }

            @Override
            public void cancel(String message, Throwable cause) {
                // call canceled - no direct Allure attachments here (we buffer captures instead)
                super.cancel(message, cause);
            }
        };
    }

    // small wrapper to capture inbound messages and close
    private class AllureClientCallListener<RespT> extends ForwardingClientCallListener.SimpleForwardingClientCallListener<RespT> {
        private final String methodName;
        private final AtomicInteger inboundCounter;
        private final String callTestId;

        protected AllureClientCallListener(io.grpc.ClientCall.Listener<RespT> delegate, String methodName, AtomicInteger inboundCounter, String callTestId) {
            super(delegate);
            this.methodName = methodName;
            this.inboundCounter = inboundCounter;
            this.callTestId = callTestId;
        }

        @Override
        public void onMessage(RespT message) {
            int idx = inboundCounter.incrementAndGet();
            try {
                bufferCapture(methodName, "Response#" + idx, message, this.callTestId, "RESPONSE");
            } catch (Exception ex) {
                // ignore buffering errors
            }
            super.onMessage(message);
        }

        @Override
        public void onClose(Status status, Metadata trailers) {
            if (!status.isOk()) {
                try {
                    bufferCapture(methodName, "Status", status.getDescription() == null ? status.getCode().name() : status.getCode().name() + ": " + status.getDescription(), this.callTestId, "STATUS");
                } catch (Exception ex) {
                    // ignore
                }
            }
            super.onClose(status, trailers);
        }
    }

    // Buffer a capture for later attachment by the TestNG listener. This keeps attachments
    // associated with the test thread when the listener drains the CAPTURES list.
    private void bufferCapture(String methodName, String label, Object message, String callTestId, String direction) {
        String payload;
        if (message instanceof Message) {
            Message proto = (Message) message;
            try {
                String json = JsonFormat.printer().includingDefaultValueFields().print(proto);
                payload = json;
            } catch (Exception ex) {
                payload = proto.toString();
            }
        } else {
            payload = String.valueOf(message);
        }

        // apply redaction if present
        if (redactor != null) {
            try {
                payload = redactor.apply(payload);
            } catch (Exception e) {
                // ignore redaction errors
            }
        }

        // truncate if too large
        if (payload.length() > maxAttachmentSizeBytes) {
            payload = payload.substring(0, maxAttachmentSizeBytes) + "\n... (truncated)";
        }

        Object raw = null;
        if (message instanceof Message) raw = message;
        Capture cap = new Capture(System.currentTimeMillis(), callTestId, methodName, direction, payload, raw);
        CAPTURES.add(cap);
    }

    // Reflection-based helper to read TestContext.getCurrentTestId() if a TestContext helper is present.
    private String tryReadTestContext() {
        try {
            Class<?> tc = Class.forName("com.example.projects.poc.grpc.helpers.TestContext");
            java.lang.reflect.Method m = tc.getMethod("getCurrentTestId");
            Object v = m.invoke(null);
            if (v instanceof String) return (String) v;
        } catch (Throwable ignored) {
            // deliberately ignore - TestContext may not be present
        }
        return null;
    }

    // (previously had a convenience to convert to InputStream; removed since attachments are immediate)
}
