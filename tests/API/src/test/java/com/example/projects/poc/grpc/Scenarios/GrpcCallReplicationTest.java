package com.example.projects.poc.grpc.Scenarios;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.stub.StreamObserver;
import okhttp3.OkHttpClient;
import okhttp3.Protocol;
import okhttp3.Request;
import okhttp3.sse.EventSource;
import okhttp3.sse.EventSources;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.security.SecureRandom;
import java.security.cert.X509Certificate;
import java.util.Arrays;
import java.util.List;
import com.example.projects.poc.grpc.helpers.SseEventListener;
import com.example.projects.poc.grpc.helpers.GrpcClientAllureInterceptor;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

// Generated classes from acquisition.proto
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass;

// Allure imports for detailed reporting
import io.qameta.allure.*;

/**
 * Replicates (in simplified form) the gRPC call sequence observed in the server logs.
 * NOTE: Proto definitions expose: submitSampleStream(EmptyRequest)->(stream SequenceReplyStream),
 * sequenceStateEvent(SequenceEvent), acquisitionStateEvent(AcquisitionEvent), deviceStateEvent(DeviceEvent).
 * There are NO message types named SubmitSampleStreamRequest/Response or AcquisitionStateEvent/DeviceStateEvent (those are RPC names).
 */
import org.testng.Assert;
import org.testng.annotations.Test;

@Epic("gRPC API Testing")
@Feature("Acquisition Lifecycle Management")

public class GrpcCallReplicationTest 
{
        private static final String SEQUENCE_ID = "001a32b5-a010-4513-a355-c0d4a21edf7c";
        private static final String SAMPLE_ID = "61f6b9b7-7149-49b9-be33-207b2d9da941";
        private static final String ZERO_GUID = "00000000-0000-0000-0000-000000000000";

        @Test(groups = "GRPC")
        @Story("gRPC Lifecycle Management with SSE Integration")
        @Description("Validates complete acquisition lifecycle via gRPC calls and verifies corresponding SSE events")
        @Severity(SeverityLevel.CRITICAL)


        public void replicateAcquisitionLifecycle() throws InterruptedException {
                // --- SSE subscription (generic lifecycle events) ---
                final String sseUrl = "https://localhost:61350/api/acquisition/v1/sse"; // generic SSE endpoint




                // Trust all SSL certs (self-signed)
                TrustManager[] trustAllCerts = new TrustManager[]{
                        new X509TrustManager() {
                                public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                                public void checkClientTrusted(X509Certificate[] certs, String authType) { }
                                public void checkServerTrusted(X509Certificate[] certs, String authType) { }
                        }
                };



                SSLContext sc;
                try {
                        sc = SSLContext.getInstance("SSL");
                        sc.init(null, trustAllCerts, new SecureRandom());
                } catch (Exception e) {
                        throw new RuntimeException("Failed to init SSL context", e);
                }

                OkHttpClient sseClient = new OkHttpClient.Builder()
                        .protocols(Arrays.asList(Protocol.HTTP_2, Protocol.HTTP_1_1))
                        .sslSocketFactory(sc.getSocketFactory(), (X509TrustManager) trustAllCerts[0])
                        .hostnameVerifier((h, s) -> true)
                        .build();

                List<String> sseEvents = java.util.Collections.synchronizedList(new java.util.ArrayList<>());
                CountDownLatch minimumEventsLatch = new CountDownLatch(3); // expect at least SequenceStatus, SampleStatus, DeviceEvent

                Request sseRequest = new Request.Builder().url(sseUrl).build();



                // use reusable SSE listener implementation for readability
                SseEventListener sseListener = new SseEventListener(sseEvents, minimumEventsLatch);
                EventSource.Factory factory = EventSources.createFactory(sseClient);
                EventSource eventSource = factory.newEventSource(sseRequest, sseListener);

                // Give the SSE connection a moment to establish
                Thread.sleep(500);

        // interceptor now attaches immediately; no timestamp drain is required

                ManagedChannel rawChannel = ManagedChannelBuilder.forAddress("localhost", 51640)
                .usePlaintext() // adjust if TLS is required
                .build();
        io.grpc.Channel interceptedChannel = io.grpc.ClientInterceptors.intercept(
            rawChannel,
                        new GrpcClientAllureInterceptor()
        );
        AcquisitionGrpc.AcquisitionBlockingStub blocking = AcquisitionGrpc.newBlockingStub(interceptedChannel);
        AcquisitionGrpc.AcquisitionStub async = AcquisitionGrpc.newStub(interceptedChannel);

                // 1. Start server streaming call SubmitSampleStream with EmptyRequest
                AcquisitionOuterClass.EmptyRequest emptyRequest = AcquisitionOuterClass.EmptyRequest.newBuilder().build();
                
                                // Start server streaming call SubmitSampleStream with async stub and a response observer
                                java.util.List<AcquisitionOuterClass.SequenceReplyStream> streamResponses =
                                        setupServerStreaming(async, emptyRequest, 10);
                                // attach a compact summary
                                Allure.addAttachment("Streaming summary", "application/json",
                                        createStreamingSummary(streamResponses), "json");

                // 2. Sequence state progression (subset of the log sequence)
                sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.MethodValidationOk, ZERO_GUID);
                sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SequenceStart, ZERO_GUID);
                sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SampleStart, SAMPLE_ID);
                sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.DataFileCreate, SAMPLE_ID);

                // 3. Acquisition state progression
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.ReadBarcode);
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.SendMethod);
                sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SampleComplete, SAMPLE_ID);
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.WaitMethodReady);
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.WaitStartSlaves);
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.WaitStartMaster);
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.WaitContactClosure);

                // 4. Device state transitions (wrap inside DeviceEvent -> repeated DeviceStatus)
                sendDeviceState(blocking, AcquisitionOuterClass.DeviceState.ReadyForRun);
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.Acquire);
                sendDeviceState(blocking, AcquisitionOuterClass.DeviceState.Running);
                sendDeviceState(blocking, AcquisitionOuterClass.DeviceState.ReadyToDownload);

                // 5. Post run states
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.PostRun);
                sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SequenceComplete, ZERO_GUID);
                sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SequenceCompletedFilesMoved, ZERO_GUID);
                sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.Ready);

                // Allow async streaming to receive potential messages (adjust as needed)
                Thread.sleep(1500);
                rawChannel.shutdown();

                // Wait (up to) for minimum SSE events
                minimumEventsLatch.await(5, TimeUnit.SECONDS);

                eventSource.cancel();

                // Interceptor now attaches payloads immediately; no drain performed here.
                Allure.step("gRPC interceptor immediate-attach mode: no buffered drain performed");

                // Basic assertions: verify we saw the lifecycle related SSE types
                boolean hasSequence = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"SequenceStatus\""));
                boolean hasSample = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"SampleStatus\""));
                boolean hasDevice = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"DeviceEvent\""));

                // Attach SSE events to Allure report
                Allure.addAttachment("📡 SSE Events Captured", "application/json", 
                    createSseEventsSummary(sseEvents), "json");

                System.out.println("[SSE] Total events received: " + sseEvents.size());
                if (!sseEvents.isEmpty()) {
                        System.out.println("[SSE] First event: " + sseEvents.get(0));
                }
                
                                // Allure assertions with better error messages
                                validateSseEventTypes(hasSequence, hasSample, hasDevice, sseEvents.size());
                
                Assert.assertTrue(true, "✅ gRPC lifecycle + SSE correlation executed successfully");
        }
        
        /**
         * Create a JSON summary of SSE events for Allure attachment
         */
        private static String createSseEventsSummary(List<String> sseEvents) {
            StringBuilder summary = new StringBuilder();
            summary.append("{\n");
            summary.append("  \"totalEvents\": ").append(sseEvents.size()).append(",\n");
            summary.append("  \"events\": [\n");
            
            for (int i = 0; i < sseEvents.size(); i++) {
                if (i > 0) summary.append(",\n");
                summary.append("    {\n");
                summary.append("      \"index\": ").append(i + 1).append(",\n");
                summary.append("      \"timestamp\": \"").append(System.currentTimeMillis()).append("\",\n");
                summary.append("      \"data\": ").append(sseEvents.get(i)).append("\n");
                summary.append("    }");
            }
            
            summary.append("\n  ]\n}");
            return summary.toString();
        }

                /**
                 * Create a compact JSON summary of streaming responses for Allure attachment
                 */
                private static String createStreamingSummary(List<AcquisitionOuterClass.SequenceReplyStream> responses) {
                        StringBuilder sb = new StringBuilder();
                        sb.append("{\n");
                        sb.append("  \"totalResponses\": ").append(responses.size()).append(",\n");
                        sb.append("  \"responses\": [\n");
                        for (int i = 0; i < responses.size(); i++) {
                                if (i > 0) sb.append(",\n");
                                sb.append("    {\n");
                                sb.append("      \"index\": ").append(i + 1).append(",\n");
                                sb.append("      \"type\": \"").append(responses.get(i).getClass().getSimpleName()).append("\"\n");
                                sb.append("    }");
                        }
                        sb.append("\n  ]\n}");
                        return sb.toString();
                }

                @Step("🔄 Setting up server streaming for SubmitSampleStream")
                private java.util.List<AcquisitionOuterClass.SequenceReplyStream> setupServerStreaming(
                        AcquisitionGrpc.AcquisitionStub async,
                        AcquisitionOuterClass.EmptyRequest emptyRequest,
                        int timeoutSeconds) {

                        final java.util.List<AcquisitionOuterClass.SequenceReplyStream> streamResponses =
                                        java.util.Collections.synchronizedList(new java.util.ArrayList<>());
                        final CountDownLatch streamLatch = new CountDownLatch(1);

                        StreamObserver<AcquisitionOuterClass.SequenceReplyStream> streamObserver = new StreamObserver<AcquisitionOuterClass.SequenceReplyStream>() {
                                @Override
                                public void onNext(AcquisitionOuterClass.SequenceReplyStream value) {
                                        streamResponses.add(value);
                                        Allure.step("📨 Received streaming response #" + streamResponses.size());
                                }

                                @Override
                                public void onError(Throwable t) {
                                        Allure.step("❌ Streaming error: " + t.getMessage());
                                        streamLatch.countDown();
                                }

                                @Override
                                public void onCompleted() {
                                        Allure.step("✅ Server streaming completed - received " + streamResponses.size() + " responses");
                                        streamLatch.countDown();
                                }
                        };

                        try {
                                async.submitSampleStream(emptyRequest, streamObserver);
                                boolean finished = streamLatch.await(timeoutSeconds, TimeUnit.SECONDS);
                                if (!finished) {
                                        Allure.step("⏰ Server streaming timed out after " + timeoutSeconds + "s");
                                }
                                Allure.step("📊 Stream received " + streamResponses.size() + " responses");
                        } catch (InterruptedException ie) {
                                Allure.step("⚠️ Streaming interrupted: " + ie.getMessage());
                                Thread.currentThread().interrupt();
                        } catch (Exception e) {
                                Allure.step("❌ Streaming failed: " + e.getMessage());
                        }

                        return streamResponses;
                }

                @Step("🔍 Validating SSE Event Types (sequence={0}, sample={1}, device={2}, totalEvents={3})")
                private void validateSseEventTypes(boolean hasSequence, boolean hasSample, boolean hasDevice, int totalEvents) {
                        Assert.assertTrue(hasSequence, "❌ Expected at least one SequenceStatus SSE event. Received " + totalEvents + " total events.");
                        Assert.assertTrue(hasSample, "❌ Expected at least one SampleStatus SSE event. Received " + totalEvents + " total events.");
                        Assert.assertTrue(hasDevice, "❌ Expected at least one DeviceEvent SSE event. Received " + totalEvents + " total events.");
                }

        @Step("📤 Send Sequence State: {state}")
        private static void sendSequenceState(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                                                                                  AcquisitionOuterClass.SequenceState state,
                                                                                  String sampleId) {
                AcquisitionOuterClass.SequenceEvent evt = AcquisitionOuterClass.SequenceEvent.newBuilder()
                                .setSequenceId(SEQUENCE_ID)
                                .setSampleId(sampleId)
                                .setSequenceState(state)
                                .setIsError(false)
                                .build();
                
                                // Execute blocking call directly (interceptor will capture attachments)
                                AcquisitionOuterClass.EmptyResponse resp = blocking.sequenceStateEvent(evt);
                
                Allure.parameter("Sequence State", state.toString());
                Allure.parameter("Sample ID", sampleId);
                logEmpty("SequenceStateEvent", resp, () -> state + " sample=" + sampleId);
        }

        @Step("📤 Send Acquisition State: {acqState}")
        private static void sendAcqState(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                                                                          AcquisitionOuterClass.AcquisitionState acqState) {
                AcquisitionOuterClass.AcquisitionEvent evt = AcquisitionOuterClass.AcquisitionEvent.newBuilder()
                                .setSequenceId(SEQUENCE_ID)
                                .setSampleId(SAMPLE_ID)
                                .setAcquisitionState(acqState)
                                .build();
                
                                // Execute blocking call directly (interceptor will capture attachments)
                                AcquisitionOuterClass.EmptyResponse resp = blocking.acquisitionStateEvent(evt);
                
                Allure.parameter("Acquisition State", acqState.toString());
                logEmpty("AcquisitionStateEvent", resp, () -> acqState.toString());
        }

        @Step("📤 Send Device State: {deviceState}")
        private static void sendDeviceState(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                                                                                AcquisitionOuterClass.DeviceState deviceState) {
                        AcquisitionOuterClass.DeviceStatus status = AcquisitionOuterClass.DeviceStatus.newBuilder()
                                .setDeviceName("SimulationMS")
                                .setDeviceState(deviceState)
                                .build();
                        AcquisitionOuterClass.DeviceEvent devEvt = AcquisitionOuterClass.DeviceEvent.newBuilder()
                                .addDeviceStatus(status)
                                .build();
                        
                                                // Execute blocking call directly (interceptor will capture attachments)
                                                AcquisitionOuterClass.EmptyResponse resp = blocking.deviceStateEvent(devEvt);
                        
                        Allure.parameter("Device State", deviceState.toString());
                        Allure.parameter("Device Name", "SimulationMS");
                        logEmpty("DeviceStateEvent", resp, () -> deviceState.toString());
        }

                        private static void logEmpty(String label, AcquisitionOuterClass.EmptyResponse resp, java.util.concurrent.Callable<String> contextSupplier) {
                                String ctx;
                                try { ctx = contextSupplier.call(); } catch (Exception e) { ctx = "<ctx-error>"; }
                                System.out.println("[Client] " + label + " ack | context=" + ctx + " | resp class=" + resp.getClass().getSimpleName());
                        }
}
