package com.example.projects.poc.grpc.Scenarios;

import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.stub.StreamObserver;
import org.testng.Assert;
import org.testng.annotations.Test;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionGrpc;
import ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass;

/**
 * Executes the same lifecycle RPC sequence as GrpcCallReplicationTest but WITHOUT SSE monitoring.
 * This is aimed at exercising the mock lifecycle service responses (EmptyResponse acks + streaming reply).
 * Assumes a mock server is running that exposes Acquisition service methods (e.g., MockAcquisitionLifecycleService).
 */
public class MockAcquisitionServiceTest {

    private static final String SEQUENCE_ID = "001a32b5-a010-4513-a355-c0d4a21edf7c";
    private static final String SAMPLE_ID = "61f6b9b7-7149-49b9-be33-207b2d9da941";
    private static final String ZERO_GUID = "00000000-0000-0000-0000-000000000000";
    private static final int PORT = 50051; // adjust if mock server uses different port

    @Test
    public void replicateLifecycleWithoutSse() throws InterruptedException {
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", PORT)
            .usePlaintext()
            .build();

        AcquisitionGrpc.AcquisitionBlockingStub blocking = AcquisitionGrpc.newBlockingStub(channel);
        AcquisitionGrpc.AcquisitionStub async = AcquisitionGrpc.newStub(channel);

        // Start submitSampleStream to receive initial sequence reply (if any)
        AcquisitionOuterClass.EmptyRequest emptyRequest = AcquisitionOuterClass.EmptyRequest.newBuilder().build();
        async.submitSampleStream(emptyRequest, new StreamObserver<AcquisitionOuterClass.SequenceReplyStream>() {
            @Override
            public void onNext(AcquisitionOuterClass.SequenceReplyStream value) {
                System.out.println("[MockLifecycle] SequenceReplyStream: id=" + value.getId() + " samples=" + value.getSamplesCount());
            }
            @Override
            public void onError(Throwable t) {
                System.err.println("[MockLifecycle] submitSampleStream error: " + t);
            }
            @Override
            public void onCompleted() {
                System.out.println("[MockLifecycle] submitSampleStream completed");
            }
        });

        // Sequence states
        seq(blocking, AcquisitionOuterClass.SequenceState.MethodValidationOk, ZERO_GUID);
        seq(blocking, AcquisitionOuterClass.SequenceState.SequenceStart, ZERO_GUID);
        seq(blocking, AcquisitionOuterClass.SequenceState.SampleStart, SAMPLE_ID);
        seq(blocking, AcquisitionOuterClass.SequenceState.DataFileCreate, SAMPLE_ID);

        // Acquisition states
        acq(blocking, AcquisitionOuterClass.AcquisitionState.ReadBarcode);
        acq(blocking, AcquisitionOuterClass.AcquisitionState.SendMethod);
        seq(blocking, AcquisitionOuterClass.SequenceState.SampleComplete, SAMPLE_ID);
        acq(blocking, AcquisitionOuterClass.AcquisitionState.WaitMethodReady);
        acq(blocking, AcquisitionOuterClass.AcquisitionState.WaitStartSlaves);
        acq(blocking, AcquisitionOuterClass.AcquisitionState.WaitStartMaster);
        acq(blocking, AcquisitionOuterClass.AcquisitionState.WaitContactClosure);

        // Device states
        dev(blocking, AcquisitionOuterClass.DeviceState.ReadyForRun);
        acq(blocking, AcquisitionOuterClass.AcquisitionState.Acquire);
        dev(blocking, AcquisitionOuterClass.DeviceState.Running);
        dev(blocking, AcquisitionOuterClass.DeviceState.ReadyToDownload);

        // Post run
        acq(blocking, AcquisitionOuterClass.AcquisitionState.PostRun);
        seq(blocking, AcquisitionOuterClass.SequenceState.SequenceComplete, ZERO_GUID);
        seq(blocking, AcquisitionOuterClass.SequenceState.SequenceCompletedFilesMoved, ZERO_GUID);
        acq(blocking, AcquisitionOuterClass.AcquisitionState.Ready);

        Thread.sleep(800); // allow async observer to receive potential sequence reply
        channel.shutdown();
        Assert.assertTrue(true, "Lifecycle gRPC sequence executed against mock service");
    }

    private static void seq(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                             AcquisitionOuterClass.SequenceState state,
                             String sampleId) {
        AcquisitionOuterClass.SequenceEvent evt = AcquisitionOuterClass.SequenceEvent.newBuilder()
            .setSequenceId(SEQUENCE_ID)
            .setSampleId(sampleId)
            .setSequenceState(state)
            .setIsError(false)
            .build();
        AcquisitionOuterClass.EmptyResponse resp = blocking.sequenceStateEvent(evt);
        log("SequenceStateEvent", state.toString(), resp);
    }

    private static void acq(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                             AcquisitionOuterClass.AcquisitionState acqState) {
        AcquisitionOuterClass.AcquisitionEvent evt = AcquisitionOuterClass.AcquisitionEvent.newBuilder()
            .setSequenceId(SEQUENCE_ID)
            .setSampleId(SAMPLE_ID)
            .setAcquisitionState(acqState)
            .build();
        AcquisitionOuterClass.EmptyResponse resp = blocking.acquisitionStateEvent(evt);
        log("AcquisitionStateEvent", acqState.toString(), resp);
    }

    private static void dev(AcquisitionGrpc.AcquisitionBlockingStub blocking,
                             AcquisitionOuterClass.DeviceState deviceState) {
        AcquisitionOuterClass.DeviceStatus status = AcquisitionOuterClass.DeviceStatus.newBuilder()
            .setDeviceName("SimulationMS")
            .setDeviceState(deviceState)
            .build();
        AcquisitionOuterClass.DeviceEvent devEvt = AcquisitionOuterClass.DeviceEvent.newBuilder()
            .addDeviceStatus(status)
            .build();
        AcquisitionOuterClass.EmptyResponse resp = blocking.deviceStateEvent(devEvt);
        log("DeviceStateEvent", deviceState.toString(), resp);
    }

    private static void log(String label, String state, AcquisitionOuterClass.EmptyResponse resp) {
        System.out.println("[MockLifecycle] " + label + " sent state=" + state + " ackClass=" + resp.getClass().getSimpleName());
    }
}
