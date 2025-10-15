package com.exampple.grpc;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.stub.StreamObserver;
import okhttp3.OkHttpClient;
import okhttp3.Protocol;
import okhttp3.Request;
import okhttp3.sse.EventSource;
import okhttp3.sse.EventSourceListener;
import okhttp3.sse.EventSources;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.security.SecureRandom;
import java.security.cert.X509Certificate;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

// Generated classes from acquisition.proto
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass;

/**
 * Replicates (in simplified form) the gRPC call sequence observed in the server logs.
 * NOTE: Proto definitions expose: submitSampleStream(EmptyRequest)->(stream SequenceReplyStream),
 * sequenceStateEvent(SequenceEvent), acquisitionStateEvent(AcquisitionEvent), deviceStateEvent(DeviceEvent).
 * There are NO message types named SubmitSampleStreamRequest/Response or AcquisitionStateEvent/DeviceStateEvent (those are RPC names).
 */
import org.testng.Assert;
import org.testng.annotations.Test;

public class GrpcCallReplicationTest {
        private static final String SEQUENCE_ID = "001a32b5-a010-4513-a355-c0d4a21edf7c";
        private static final String SAMPLE_ID = "61f6b9b7-7149-49b9-be33-207b2d9da941";
        private static final String ZERO_GUID = "00000000-0000-0000-0000-000000000000";

        @Test(groups = "GRPC")
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
                EventSourceListener sseListener = new EventSourceListener() {
                        @Override
                        public void onEvent(EventSource eventSource, String id, String type, String data) {
                                sseEvents.add(data);
                                System.out.println("[SSE] " + data);
                                // naive type detection
                                if (data.contains("\"Type\":\"SequenceStatus\"")) minimumEventsLatch.countDown();
                                else if (data.contains("\"Type\":\"SampleStatus\"")) minimumEventsLatch.countDown();
                                else if (data.contains("\"Type\":\"DeviceEvent\"")) minimumEventsLatch.countDown();
                        }
                        @Override
                        public void onFailure(EventSource eventSource, Throwable t, okhttp3.Response response) {
                                System.err.println("[SSE] Error: " + t);
                        }
                };
                EventSource.Factory factory = EventSources.createFactory(sseClient);
                EventSource eventSource = factory.newEventSource(sseRequest, sseListener);

                // Give the SSE connection a moment to establish
                Thread.sleep(500);

                ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 51640)
                                .usePlaintext() // adjust if TLS is required
                                .build();

                AcquisitionGrpc.AcquisitionBlockingStub blocking = AcquisitionGrpc.newBlockingStub(channel);
                AcquisitionGrpc.AcquisitionStub async = AcquisitionGrpc.newStub(channel);

                // 1. Start server streaming call SubmitSampleStream with EmptyRequest
                AcquisitionOuterClass.EmptyRequest emptyRequest = AcquisitionOuterClass.EmptyRequest.newBuilder().build();
                async.submitSampleStream(emptyRequest, new StreamObserver<AcquisitionOuterClass.SequenceReplyStream>() {
                        @Override
                        public void onNext(AcquisitionOuterClass.SequenceReplyStream value) {
                                System.out.println("[Client] SequenceReplyStream: id=" + value.getId() + ", samples=" + value.getSamplesCount());
                        }
                        @Override
                        public void onError(Throwable t) {
                                System.err.println("[Client] submitSampleStream error: " + t);
                        }
                        @Override
                        public void onCompleted() {
                                System.out.println("[Client] submitSampleStream completed");
                        }
                });

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
                channel.shutdown();

                // Wait (up to) for minimum SSE events
                minimumEventsLatch.await(5, TimeUnit.SECONDS);

                eventSource.cancel();

                // Basic assertions: verify we saw the lifecycle related SSE types
                boolean hasSequence = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"SequenceStatus\""));
                boolean hasSample = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"SampleStatus\""));
                boolean hasDevice = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"DeviceEvent\""));

                System.out.println("[SSE] Total events received: " + sseEvents.size());
                if (!sseEvents.isEmpty()) {
                        System.out.println("[SSE] First event: " + sseEvents.get(0));
                }
                Assert.assertTrue(hasSequence, "Expected at least one SequenceStatus SSE event");
                Assert.assertTrue(hasSample, "Expected at least one SampleStatus SSE event");
                Assert.assertTrue(hasDevice, "Expected at least one DeviceEvent SSE event");
                Assert.assertTrue(true, "gRPC lifecycle + SSE correlation executed");
        }

        private static void sendSequenceState(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                                                                                  AcquisitionOuterClass.SequenceState state,
                                                                                  String sampleId) {
                AcquisitionOuterClass.SequenceEvent evt = AcquisitionOuterClass.SequenceEvent.newBuilder()
                                .setSequenceId(SEQUENCE_ID)
                                .setSampleId(sampleId)
                                .setSequenceState(state)
                                .setIsError(false)
                                .build();
                AcquisitionOuterClass.EmptyResponse resp = blocking.sequenceStateEvent(evt);
                logEmpty("SequenceStateEvent", resp, () -> state + " sample=" + sampleId);
        }

        private static void sendAcqState(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                                                                          AcquisitionOuterClass.AcquisitionState acqState) {
                AcquisitionOuterClass.AcquisitionEvent evt = AcquisitionOuterClass.AcquisitionEvent.newBuilder()
                                .setSequenceId(SEQUENCE_ID)
                                .setSampleId(SAMPLE_ID)
                                .setAcquisitionState(acqState)
                                .build();
                AcquisitionOuterClass.EmptyResponse resp = blocking.acquisitionStateEvent(evt);
                logEmpty("AcquisitionStateEvent", resp, () -> acqState.toString());
        }

        private static void sendDeviceState(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                                                                                AcquisitionOuterClass.DeviceState deviceState) {
                        AcquisitionOuterClass.DeviceStatus status = AcquisitionOuterClass.DeviceStatus.newBuilder()
                                .setDeviceName("SimulationMS")
                                .setDeviceState(deviceState)
                                .build();
                        AcquisitionOuterClass.DeviceEvent devEvt = AcquisitionOuterClass.DeviceEvent.newBuilder()
                                .addDeviceStatus(status)
                                .build();
                        AcquisitionOuterClass.EmptyResponse resp = blocking.deviceStateEvent(devEvt);
                        logEmpty("DeviceStateEvent", resp, () -> deviceState.toString());
        }

                        private static void logEmpty(String label, AcquisitionOuterClass.EmptyResponse resp, java.util.concurrent.Callable<String> contextSupplier) {
                                String ctx;
                                try { ctx = contextSupplier.call(); } catch (Exception e) { ctx = "<ctx-error>"; }
                                System.out.println("[Client] " + label + " ack | context=" + ctx + " | resp class=" + resp.getClass().getSimpleName());
                        }
}
