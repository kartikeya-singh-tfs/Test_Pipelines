package ThermoFisher.AcquisitionModule.Contracts;

import static io.grpc.MethodDescriptor.generateFullMethodName;

/**
 * <pre>
 * The greeting service definition.
 * </pre>
 */
@io.grpc.stub.annotations.GrpcGenerated
public final class AcquisitionGrpc {

  private AcquisitionGrpc() {}

  public static final java.lang.String SERVICE_NAME = "ThermoFisher.AcquisitionModule.Contracts.Acquisition";

  // Static method descriptors that strictly reflect the proto.
  private static volatile io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest,
      ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream> getSubmitSampleStreamMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "SubmitSampleStream",
      requestType = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest.class,
      responseType = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream.class,
      methodType = io.grpc.MethodDescriptor.MethodType.SERVER_STREAMING)
  public static io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest,
      ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream> getSubmitSampleStreamMethod() {
    io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream> getSubmitSampleStreamMethod;
    if ((getSubmitSampleStreamMethod = AcquisitionGrpc.getSubmitSampleStreamMethod) == null) {
      synchronized (AcquisitionGrpc.class) {
        if ((getSubmitSampleStreamMethod = AcquisitionGrpc.getSubmitSampleStreamMethod) == null) {
          AcquisitionGrpc.getSubmitSampleStreamMethod = getSubmitSampleStreamMethod =
              io.grpc.MethodDescriptor.<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.SERVER_STREAMING)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "SubmitSampleStream"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream.getDefaultInstance()))
              .setSchemaDescriptor(new AcquisitionMethodDescriptorSupplier("SubmitSampleStream"))
              .build();
        }
      }
    }
    return getSubmitSampleStreamMethod;
  }

  private static volatile io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent,
      ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getSequenceStateEventMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "SequenceStateEvent",
      requestType = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent.class,
      responseType = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.UNARY)
  public static io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent,
      ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getSequenceStateEventMethod() {
    io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getSequenceStateEventMethod;
    if ((getSequenceStateEventMethod = AcquisitionGrpc.getSequenceStateEventMethod) == null) {
      synchronized (AcquisitionGrpc.class) {
        if ((getSequenceStateEventMethod = AcquisitionGrpc.getSequenceStateEventMethod) == null) {
          AcquisitionGrpc.getSequenceStateEventMethod = getSequenceStateEventMethod =
              io.grpc.MethodDescriptor.<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.UNARY)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "SequenceStateEvent"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse.getDefaultInstance()))
              .setSchemaDescriptor(new AcquisitionMethodDescriptorSupplier("SequenceStateEvent"))
              .build();
        }
      }
    }
    return getSequenceStateEventMethod;
  }

  private static volatile io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent,
      ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getAcquisitionStateEventMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "AcquisitionStateEvent",
      requestType = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent.class,
      responseType = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.UNARY)
  public static io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent,
      ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getAcquisitionStateEventMethod() {
    io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getAcquisitionStateEventMethod;
    if ((getAcquisitionStateEventMethod = AcquisitionGrpc.getAcquisitionStateEventMethod) == null) {
      synchronized (AcquisitionGrpc.class) {
        if ((getAcquisitionStateEventMethod = AcquisitionGrpc.getAcquisitionStateEventMethod) == null) {
          AcquisitionGrpc.getAcquisitionStateEventMethod = getAcquisitionStateEventMethod =
              io.grpc.MethodDescriptor.<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.UNARY)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "AcquisitionStateEvent"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse.getDefaultInstance()))
              .setSchemaDescriptor(new AcquisitionMethodDescriptorSupplier("AcquisitionStateEvent"))
              .build();
        }
      }
    }
    return getAcquisitionStateEventMethod;
  }

  private static volatile io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent,
      ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getDeviceStateEventMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "DeviceStateEvent",
      requestType = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent.class,
      responseType = ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.UNARY)
  public static io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent,
      ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getDeviceStateEventMethod() {
    io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> getDeviceStateEventMethod;
    if ((getDeviceStateEventMethod = AcquisitionGrpc.getDeviceStateEventMethod) == null) {
      synchronized (AcquisitionGrpc.class) {
        if ((getDeviceStateEventMethod = AcquisitionGrpc.getDeviceStateEventMethod) == null) {
          AcquisitionGrpc.getDeviceStateEventMethod = getDeviceStateEventMethod =
              io.grpc.MethodDescriptor.<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.UNARY)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "DeviceStateEvent"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse.getDefaultInstance()))
              .setSchemaDescriptor(new AcquisitionMethodDescriptorSupplier("DeviceStateEvent"))
              .build();
        }
      }
    }
    return getDeviceStateEventMethod;
  }

  /**
   * Creates a new async stub that supports all call types for the service
   */
  public static AcquisitionStub newStub(io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<AcquisitionStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<AcquisitionStub>() {
        @java.lang.Override
        public AcquisitionStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new AcquisitionStub(channel, callOptions);
        }
      };
    return AcquisitionStub.newStub(factory, channel);
  }

  /**
   * Creates a new blocking-style stub that supports all types of calls on the service
   */
  public static AcquisitionBlockingV2Stub newBlockingV2Stub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<AcquisitionBlockingV2Stub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<AcquisitionBlockingV2Stub>() {
        @java.lang.Override
        public AcquisitionBlockingV2Stub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new AcquisitionBlockingV2Stub(channel, callOptions);
        }
      };
    return AcquisitionBlockingV2Stub.newStub(factory, channel);
  }

  /**
   * Creates a new blocking-style stub that supports unary and streaming output calls on the service
   */
  public static AcquisitionBlockingStub newBlockingStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<AcquisitionBlockingStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<AcquisitionBlockingStub>() {
        @java.lang.Override
        public AcquisitionBlockingStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new AcquisitionBlockingStub(channel, callOptions);
        }
      };
    return AcquisitionBlockingStub.newStub(factory, channel);
  }

  /**
   * Creates a new ListenableFuture-style stub that supports unary calls on the service
   */
  public static AcquisitionFutureStub newFutureStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<AcquisitionFutureStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<AcquisitionFutureStub>() {
        @java.lang.Override
        public AcquisitionFutureStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new AcquisitionFutureStub(channel, callOptions);
        }
      };
    return AcquisitionFutureStub.newStub(factory, channel);
  }

  /**
   * <pre>
   * The greeting service definition.
   * </pre>
   */
  public interface AsyncService {

    /**
     */
    default void submitSampleStream(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getSubmitSampleStreamMethod(), responseObserver);
    }

    /**
     */
    default void sequenceStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getSequenceStateEventMethod(), responseObserver);
    }

    /**
     */
    default void acquisitionStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getAcquisitionStateEventMethod(), responseObserver);
    }

    /**
     */
    default void deviceStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getDeviceStateEventMethod(), responseObserver);
    }
  }

  /**
   * Base class for the server implementation of the service Acquisition.
   * <pre>
   * The greeting service definition.
   * </pre>
   */
  public static abstract class AcquisitionImplBase
      implements io.grpc.BindableService, AsyncService {

    @java.lang.Override public final io.grpc.ServerServiceDefinition bindService() {
      return AcquisitionGrpc.bindService(this);
    }
  }

  /**
   * A stub to allow clients to do asynchronous rpc calls to service Acquisition.
   * <pre>
   * The greeting service definition.
   * </pre>
   */
  public static final class AcquisitionStub
      extends io.grpc.stub.AbstractAsyncStub<AcquisitionStub> {
    private AcquisitionStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected AcquisitionStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new AcquisitionStub(channel, callOptions);
    }

    /**
     */
    public void submitSampleStream(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream> responseObserver) {
      io.grpc.stub.ClientCalls.asyncServerStreamingCall(
          getChannel().newCall(getSubmitSampleStreamMethod(), getCallOptions()), request, responseObserver);
    }

    /**
     */
    public void sequenceStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> responseObserver) {
      io.grpc.stub.ClientCalls.asyncUnaryCall(
          getChannel().newCall(getSequenceStateEventMethod(), getCallOptions()), request, responseObserver);
    }

    /**
     */
    public void acquisitionStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> responseObserver) {
      io.grpc.stub.ClientCalls.asyncUnaryCall(
          getChannel().newCall(getAcquisitionStateEventMethod(), getCallOptions()), request, responseObserver);
    }

    /**
     */
    public void deviceStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> responseObserver) {
      io.grpc.stub.ClientCalls.asyncUnaryCall(
          getChannel().newCall(getDeviceStateEventMethod(), getCallOptions()), request, responseObserver);
    }
  }

  /**
   * A stub to allow clients to do synchronous rpc calls to service Acquisition.
   * <pre>
   * The greeting service definition.
   * </pre>
   */
  public static final class AcquisitionBlockingV2Stub
      extends io.grpc.stub.AbstractBlockingStub<AcquisitionBlockingV2Stub> {
    private AcquisitionBlockingV2Stub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected AcquisitionBlockingV2Stub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new AcquisitionBlockingV2Stub(channel, callOptions);
    }

    /**
     */
    @io.grpc.ExperimentalApi("https://github.com/grpc/grpc-java/issues/10918")
    public io.grpc.stub.BlockingClientCall<?, ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream>
        submitSampleStream(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest request) {
      return io.grpc.stub.ClientCalls.blockingV2ServerStreamingCall(
          getChannel(), getSubmitSampleStreamMethod(), getCallOptions(), request);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse sequenceStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent request) throws io.grpc.StatusException {
      return io.grpc.stub.ClientCalls.blockingV2UnaryCall(
          getChannel(), getSequenceStateEventMethod(), getCallOptions(), request);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse acquisitionStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent request) throws io.grpc.StatusException {
      return io.grpc.stub.ClientCalls.blockingV2UnaryCall(
          getChannel(), getAcquisitionStateEventMethod(), getCallOptions(), request);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse deviceStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent request) throws io.grpc.StatusException {
      return io.grpc.stub.ClientCalls.blockingV2UnaryCall(
          getChannel(), getDeviceStateEventMethod(), getCallOptions(), request);
    }
  }

  /**
   * A stub to allow clients to do limited synchronous rpc calls to service Acquisition.
   * <pre>
   * The greeting service definition.
   * </pre>
   */
  public static final class AcquisitionBlockingStub
      extends io.grpc.stub.AbstractBlockingStub<AcquisitionBlockingStub> {
    private AcquisitionBlockingStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected AcquisitionBlockingStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new AcquisitionBlockingStub(channel, callOptions);
    }

    /**
     */
    public java.util.Iterator<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream> submitSampleStream(
        ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest request) {
      return io.grpc.stub.ClientCalls.blockingServerStreamingCall(
          getChannel(), getSubmitSampleStreamMethod(), getCallOptions(), request);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse sequenceStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent request) {
      return io.grpc.stub.ClientCalls.blockingUnaryCall(
          getChannel(), getSequenceStateEventMethod(), getCallOptions(), request);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse acquisitionStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent request) {
      return io.grpc.stub.ClientCalls.blockingUnaryCall(
          getChannel(), getAcquisitionStateEventMethod(), getCallOptions(), request);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse deviceStateEvent(ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent request) {
      return io.grpc.stub.ClientCalls.blockingUnaryCall(
          getChannel(), getDeviceStateEventMethod(), getCallOptions(), request);
    }
  }

  /**
   * A stub to allow clients to do ListenableFuture-style rpc calls to service Acquisition.
   * <pre>
   * The greeting service definition.
   * </pre>
   */
  public static final class AcquisitionFutureStub
      extends io.grpc.stub.AbstractFutureStub<AcquisitionFutureStub> {
    private AcquisitionFutureStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected AcquisitionFutureStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new AcquisitionFutureStub(channel, callOptions);
    }

    /**
     */
    public com.google.common.util.concurrent.ListenableFuture<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> sequenceStateEvent(
        ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent request) {
      return io.grpc.stub.ClientCalls.futureUnaryCall(
          getChannel().newCall(getSequenceStateEventMethod(), getCallOptions()), request);
    }

    /**
     */
    public com.google.common.util.concurrent.ListenableFuture<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> acquisitionStateEvent(
        ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent request) {
      return io.grpc.stub.ClientCalls.futureUnaryCall(
          getChannel().newCall(getAcquisitionStateEventMethod(), getCallOptions()), request);
    }

    /**
     */
    public com.google.common.util.concurrent.ListenableFuture<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse> deviceStateEvent(
        ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent request) {
      return io.grpc.stub.ClientCalls.futureUnaryCall(
          getChannel().newCall(getDeviceStateEventMethod(), getCallOptions()), request);
    }
  }

  private static final int METHODID_SUBMIT_SAMPLE_STREAM = 0;
  private static final int METHODID_SEQUENCE_STATE_EVENT = 1;
  private static final int METHODID_ACQUISITION_STATE_EVENT = 2;
  private static final int METHODID_DEVICE_STATE_EVENT = 3;

  private static final class MethodHandlers<Req, Resp> implements
      io.grpc.stub.ServerCalls.UnaryMethod<Req, Resp>,
      io.grpc.stub.ServerCalls.ServerStreamingMethod<Req, Resp>,
      io.grpc.stub.ServerCalls.ClientStreamingMethod<Req, Resp>,
      io.grpc.stub.ServerCalls.BidiStreamingMethod<Req, Resp> {
    private final AsyncService serviceImpl;
    private final int methodId;

    MethodHandlers(AsyncService serviceImpl, int methodId) {
      this.serviceImpl = serviceImpl;
      this.methodId = methodId;
    }

    @java.lang.Override
    @java.lang.SuppressWarnings("unchecked")
    public void invoke(Req request, io.grpc.stub.StreamObserver<Resp> responseObserver) {
      switch (methodId) {
        case METHODID_SUBMIT_SAMPLE_STREAM:
          serviceImpl.submitSampleStream((ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest) request,
              (io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream>) responseObserver);
          break;
        case METHODID_SEQUENCE_STATE_EVENT:
          serviceImpl.sequenceStateEvent((ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent) request,
              (io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>) responseObserver);
          break;
        case METHODID_ACQUISITION_STATE_EVENT:
          serviceImpl.acquisitionStateEvent((ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent) request,
              (io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>) responseObserver);
          break;
        case METHODID_DEVICE_STATE_EVENT:
          serviceImpl.deviceStateEvent((ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent) request,
              (io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>) responseObserver);
          break;
        default:
          throw new AssertionError();
      }
    }

    @java.lang.Override
    @java.lang.SuppressWarnings("unchecked")
    public io.grpc.stub.StreamObserver<Req> invoke(
        io.grpc.stub.StreamObserver<Resp> responseObserver) {
      switch (methodId) {
        default:
          throw new AssertionError();
      }
    }
  }

  public static final io.grpc.ServerServiceDefinition bindService(AsyncService service) {
    return io.grpc.ServerServiceDefinition.builder(getServiceDescriptor())
        .addMethod(
          getSubmitSampleStreamMethod(),
          io.grpc.stub.ServerCalls.asyncServerStreamingCall(
            new MethodHandlers<
              ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyRequest,
              ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceReplyStream>(
                service, METHODID_SUBMIT_SAMPLE_STREAM)))
        .addMethod(
          getSequenceStateEventMethod(),
          io.grpc.stub.ServerCalls.asyncUnaryCall(
            new MethodHandlers<
              ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.SequenceEvent,
              ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>(
                service, METHODID_SEQUENCE_STATE_EVENT)))
        .addMethod(
          getAcquisitionStateEventMethod(),
          io.grpc.stub.ServerCalls.asyncUnaryCall(
            new MethodHandlers<
              ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.AcquisitionEvent,
              ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>(
                service, METHODID_ACQUISITION_STATE_EVENT)))
        .addMethod(
          getDeviceStateEventMethod(),
          io.grpc.stub.ServerCalls.asyncUnaryCall(
            new MethodHandlers<
              ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.DeviceEvent,
              ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.EmptyResponse>(
                service, METHODID_DEVICE_STATE_EVENT)))
        .build();
  }

  private static abstract class AcquisitionBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoFileDescriptorSupplier, io.grpc.protobuf.ProtoServiceDescriptorSupplier {
    AcquisitionBaseDescriptorSupplier() {}

    @java.lang.Override
    public com.google.protobuf.Descriptors.FileDescriptor getFileDescriptor() {
      return ThermoFisher.AcquisitionModule.Contracts.AcquisitionOuterClass.getDescriptor();
    }

    @java.lang.Override
    public com.google.protobuf.Descriptors.ServiceDescriptor getServiceDescriptor() {
      return getFileDescriptor().findServiceByName("Acquisition");
    }
  }

  private static final class AcquisitionFileDescriptorSupplier
      extends AcquisitionBaseDescriptorSupplier {
    AcquisitionFileDescriptorSupplier() {}
  }

  private static final class AcquisitionMethodDescriptorSupplier
      extends AcquisitionBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoMethodDescriptorSupplier {
    private final java.lang.String methodName;

    AcquisitionMethodDescriptorSupplier(java.lang.String methodName) {
      this.methodName = methodName;
    }

    @java.lang.Override
    public com.google.protobuf.Descriptors.MethodDescriptor getMethodDescriptor() {
      return getServiceDescriptor().findMethodByName(methodName);
    }
  }

  private static volatile io.grpc.ServiceDescriptor serviceDescriptor;

  public static io.grpc.ServiceDescriptor getServiceDescriptor() {
    io.grpc.ServiceDescriptor result = serviceDescriptor;
    if (result == null) {
      synchronized (AcquisitionGrpc.class) {
        result = serviceDescriptor;
        if (result == null) {
          serviceDescriptor = result = io.grpc.ServiceDescriptor.newBuilder(SERVICE_NAME)
              .setSchemaDescriptor(new AcquisitionFileDescriptorSupplier())
              .addMethod(getSubmitSampleStreamMethod())
              .addMethod(getSequenceStateEventMethod())
              .addMethod(getAcquisitionStateEventMethod())
              .addMethod(getDeviceStateEventMethod())
              .build();
        }
      }
    }
    return result;
  }
}
