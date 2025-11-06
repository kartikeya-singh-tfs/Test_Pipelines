package com.example.projects.poc.grpc.Scenarios;

import io.grpc.ManagedChannel;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.sse.EventSource;
import okhttp3.sse.EventSources;

import java.util.List;
import com.example.projects.poc.grpc.helpers.SseEventListener;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import com.example.projects.poc.commons.CommonUtilities;
// Generated classes from acquisition.proto
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass;

// Allure imports for detailed reporting
import io.qameta.allure.*;

import org.testng.Assert;
import org.testng.annotations.Listeners;
import org.testng.annotations.Test;

@Epic("gRPC API Testing - AllureGrpc")
@Feature("Acquisition Lifecycle Management - AllureGrpc")
@Listeners(com.example.projects.poc.grpc.helpers.GrpcAllureTestListener.class)

public class GrpcCallReplicationAllureGrpcTest 
{
    private static final String SEQUENCE_ID = "001a32b5-a010-4513-a355-c0d4a21edf7c";
    private static final String SAMPLE_ID = "61f6b9b7-7149-49b9-be33-207b2d9da941";
    private static final String ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    @Test(groups = "GRPCAllure")
    @Story("gRPC Lifecycle Management with SSE Integration - AllureGrpc")
    @Description("Acquisition lifecycle test using the AllureGrpc interceptor to produce attachments")
    @Severity(SeverityLevel.CRITICAL)

    public void replicateAcquisitionLifecycleWithAllureGrpc() throws InterruptedException {
        // SSE subscription (reuse same logic)
        final String sseUrl = "https://localhost:61350/api/acquisition/v1/sse";

        // Reuse the common test util to create a permissive SSL context for SSE
        OkHttpClient sseClient = CommonUtilities.okHttpClientTrustAll();

        List<String> sseEvents = java.util.Collections.synchronizedList(new java.util.ArrayList<>());
        CountDownLatch minimumEventsLatch = new CountDownLatch(3);

        Request sseRequest = new Request.Builder().url(sseUrl).build();
        SseEventListener sseListener = new SseEventListener(sseEvents, minimumEventsLatch);
        EventSource.Factory factory = EventSources.createFactory(sseClient);
        EventSource eventSource = factory.newEventSource(sseRequest, sseListener);
        Thread.sleep(500);



    // Use shared helper to create a channel already wrapped with Allure and Observability
    ManagedChannel interceptedChannel = CommonUtilities.createGrpcChannel("localhost", 51640);



        AcquisitionGrpc.AcquisitionBlockingStub blocking = AcquisitionGrpc.newBlockingStub(interceptedChannel);
        AcquisitionGrpc.AcquisitionStub async = AcquisitionGrpc.newStub(interceptedChannel);

    AcquisitionOuterClass.EmptyRequest emptyRequest = AcquisitionOuterClass.EmptyRequest.newBuilder().build();
    java.util.List<AcquisitionOuterClass.SequenceReplyStream> streamResponses = com.example.projects.poc.commons.CommonUtilities.setupServerStreaming(async, emptyRequest, 10);

        sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.MethodValidationOk, ZERO_GUID);
        sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SequenceStart, ZERO_GUID);
        sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SampleStart, SAMPLE_ID);
        sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.DataFileCreate, SAMPLE_ID);

        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.ReadBarcode);
        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.SendMethod);
        sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SampleComplete, SAMPLE_ID);
        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.WaitMethodReady);
        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.WaitStartSlaves);
        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.WaitStartMaster);
        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.WaitContactClosure);

        sendDeviceState(blocking, AcquisitionOuterClass.DeviceState.ReadyForRun);
        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.Acquire);
        sendDeviceState(blocking, AcquisitionOuterClass.DeviceState.Running);
        sendDeviceState(blocking, AcquisitionOuterClass.DeviceState.ReadyToDownload);

        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.PostRun);
        sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SequenceComplete, ZERO_GUID);
        sendSequenceState(blocking, AcquisitionOuterClass.SequenceState.SequenceCompletedFilesMoved, ZERO_GUID);
        sendAcqState(blocking, AcquisitionOuterClass.AcquisitionState.Ready);

        // Give the async stream a short window to receive responses triggered by the
        // sequence of events we just sent. Poll the shared list for up to 5s.
        int pollMs = 100;
        int maxPolls = 50; // 50 * 100ms = 5s
        int polls = 0;
        while (streamResponses.isEmpty() && polls < maxPolls) {
            Thread.sleep(pollMs);
            polls++;
        }

        Allure.step("📊 Stream received (post-send) " + streamResponses.size() + " responses after polling");

        // Attach a compact streaming summary after we've polled for responses so the
        // attachment reflects actual received messages instead of an empty list.
        if (streamResponses != null && !streamResponses.isEmpty()) {
            Allure.addAttachment("Streaming summary (AllureGrpc)", "application/json", CommonUtilities.createStreamingSummary(streamResponses), "json");
        } else {
            Allure.step("No streaming responses received; skipping streaming summary attachment");
        }

    interceptedChannel.shutdown();
        minimumEventsLatch.await(5, TimeUnit.SECONDS);
        eventSource.cancel();

        Allure.step("AllureGrpc interceptor used; attachments should be created by the interceptor");

        boolean hasSequence = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"SequenceStatus\""));
        boolean hasSample = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"SampleStatus\""));
        boolean hasDevice = sseEvents.stream().anyMatch(e -> e.contains("\"Type\":\"DeviceEvent\""));

    Allure.addAttachment("📡 SSE Events Captured (AllureGrpc)", "application/json", CommonUtilities.createStreamingSummary(sseEvents), "json");

        validateSseEventTypes(hasSequence, hasSample, hasDevice, sseEvents.size());
        Assert.assertTrue(true, "✅ gRPC lifecycle + SSE correlation executed with AllureGrpc");
    }

    // Reuse helper methods from original test (createSseEventsSummary, createStreamingSummary, setupServerStreaming, sendSequenceState, sendAcqState, sendDeviceState, validateSseEventTypes)
    // To avoid duplication, we copy minimal implementations below. In a real refactor these could be shared.

    // Use CommonUtilities.createStreamingSummary(...) to produce compact JSON summaries for streaming

    

    // setupServerStreaming moved to CommonUtilities.setupServerStreaming

    @Step("🔍 Validating SSE Event Types (sequence={0}, sample={1}, device={2}, totalEvents={3})")
    private void validateSseEventTypes(boolean hasSequence, boolean hasSample, boolean hasDevice, int totalEvents) {
        Assert.assertTrue(hasSequence, "❌ Expected at least one SequenceStatus SSE event. Received " + totalEvents + " total events.");
        Assert.assertTrue(hasSample, "❌ Expected at least one SampleStatus SSE event. Received " + totalEvents + " total events.");
        Assert.assertTrue(hasDevice, "❌ Expected at least one DeviceEvent SSE event. Received " + totalEvents + " total events.");
    }

    @Step("📤 Send Sequence State: {state} (sampleId={2})")
    private static void sendSequenceState(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                      AcquisitionOuterClass.SequenceState state,
                      String sampleId) {
    AcquisitionOuterClass.SequenceEvent evt = AcquisitionOuterClass.SequenceEvent.newBuilder()
        .setSequenceId(SEQUENCE_ID)
        .setSampleId(sampleId)
        .setSequenceState(state)
        .setIsError(false)
        .build();

    // Avoid duplicating parameters in the Allure report — include sampleId in the step title
    blocking.sequenceStateEvent(evt);
    }

    @Step("📤 Send Acquisition State: {acqState}")
    private static void sendAcqState(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                                     AcquisitionOuterClass.AcquisitionState acqState) {
        AcquisitionOuterClass.AcquisitionEvent evt = AcquisitionOuterClass.AcquisitionEvent.newBuilder()
                .setSequenceId(SEQUENCE_ID)
                .setSampleId(SAMPLE_ID)
                .setAcquisitionState(acqState)
                .build();
        blocking.acquisitionStateEvent(evt);
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

    // Place parameters under the step for clearer mapping in Allure

    blocking.deviceStateEvent(devEvt);
    }
}
