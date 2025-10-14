package ThermoFisher.AcquisitionModule.Contracts;

import static io.grpc.MethodDescriptor.generateFullMethodName;

/**
 */
@io.grpc.stub.annotations.GrpcGenerated
public final class RealTimePlotServiceGrpc {

  private RealTimePlotServiceGrpc() {}

  public static final java.lang.String SERVICE_NAME = "ThermoFisher.AcquisitionModule.Contracts.RealTimePlotService";

  // Static method descriptors that strictly reflect the proto.
  private static volatile io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData,
      ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getStreamSparklineDataMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "StreamSparklineData",
      requestType = ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData.class,
      responseType = ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.BIDI_STREAMING)
  public static io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData,
      ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getStreamSparklineDataMethod() {
    io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getStreamSparklineDataMethod;
    if ((getStreamSparklineDataMethod = RealTimePlotServiceGrpc.getStreamSparklineDataMethod) == null) {
      synchronized (RealTimePlotServiceGrpc.class) {
        if ((getStreamSparklineDataMethod = RealTimePlotServiceGrpc.getStreamSparklineDataMethod) == null) {
          RealTimePlotServiceGrpc.getStreamSparklineDataMethod = getStreamSparklineDataMethod =
              io.grpc.MethodDescriptor.<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.BIDI_STREAMING)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "StreamSparklineData"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse.getDefaultInstance()))
              .setSchemaDescriptor(new RealTimePlotServiceMethodDescriptorSupplier("StreamSparklineData"))
              .build();
        }
      }
    }
    return getStreamSparklineDataMethod;
  }

  private static volatile io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData,
      ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getStreamChromatogramSvgDataMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "StreamChromatogramSvgData",
      requestType = ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData.class,
      responseType = ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.BIDI_STREAMING)
  public static io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData,
      ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getStreamChromatogramSvgDataMethod() {
    io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getStreamChromatogramSvgDataMethod;
    if ((getStreamChromatogramSvgDataMethod = RealTimePlotServiceGrpc.getStreamChromatogramSvgDataMethod) == null) {
      synchronized (RealTimePlotServiceGrpc.class) {
        if ((getStreamChromatogramSvgDataMethod = RealTimePlotServiceGrpc.getStreamChromatogramSvgDataMethod) == null) {
          RealTimePlotServiceGrpc.getStreamChromatogramSvgDataMethod = getStreamChromatogramSvgDataMethod =
              io.grpc.MethodDescriptor.<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.BIDI_STREAMING)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "StreamChromatogramSvgData"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse.getDefaultInstance()))
              .setSchemaDescriptor(new RealTimePlotServiceMethodDescriptorSupplier("StreamChromatogramSvgData"))
              .build();
        }
      }
    }
    return getStreamChromatogramSvgDataMethod;
  }

  private static volatile io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData,
      ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getUploadSparklineDataMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "UploadSparklineData",
      requestType = ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData.class,
      responseType = ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.UNARY)
  public static io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData,
      ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getUploadSparklineDataMethod() {
    io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getUploadSparklineDataMethod;
    if ((getUploadSparklineDataMethod = RealTimePlotServiceGrpc.getUploadSparklineDataMethod) == null) {
      synchronized (RealTimePlotServiceGrpc.class) {
        if ((getUploadSparklineDataMethod = RealTimePlotServiceGrpc.getUploadSparklineDataMethod) == null) {
          RealTimePlotServiceGrpc.getUploadSparklineDataMethod = getUploadSparklineDataMethod =
              io.grpc.MethodDescriptor.<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.UNARY)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "UploadSparklineData"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse.getDefaultInstance()))
              .setSchemaDescriptor(new RealTimePlotServiceMethodDescriptorSupplier("UploadSparklineData"))
              .build();
        }
      }
    }
    return getUploadSparklineDataMethod;
  }

  private static volatile io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData,
      ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getUploadChromatogramSvgDataMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "UploadChromatogramSvgData",
      requestType = ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData.class,
      responseType = ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.UNARY)
  public static io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData,
      ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getUploadChromatogramSvgDataMethod() {
    io.grpc.MethodDescriptor<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> getUploadChromatogramSvgDataMethod;
    if ((getUploadChromatogramSvgDataMethod = RealTimePlotServiceGrpc.getUploadChromatogramSvgDataMethod) == null) {
      synchronized (RealTimePlotServiceGrpc.class) {
        if ((getUploadChromatogramSvgDataMethod = RealTimePlotServiceGrpc.getUploadChromatogramSvgDataMethod) == null) {
          RealTimePlotServiceGrpc.getUploadChromatogramSvgDataMethod = getUploadChromatogramSvgDataMethod =
              io.grpc.MethodDescriptor.<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.UNARY)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "UploadChromatogramSvgData"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse.getDefaultInstance()))
              .setSchemaDescriptor(new RealTimePlotServiceMethodDescriptorSupplier("UploadChromatogramSvgData"))
              .build();
        }
      }
    }
    return getUploadChromatogramSvgDataMethod;
  }

  /**
   * Creates a new async stub that supports all call types for the service
   */
  public static RealTimePlotServiceStub newStub(io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<RealTimePlotServiceStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<RealTimePlotServiceStub>() {
        @java.lang.Override
        public RealTimePlotServiceStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new RealTimePlotServiceStub(channel, callOptions);
        }
      };
    return RealTimePlotServiceStub.newStub(factory, channel);
  }

  /**
   * Creates a new blocking-style stub that supports all types of calls on the service
   */
  public static RealTimePlotServiceBlockingV2Stub newBlockingV2Stub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<RealTimePlotServiceBlockingV2Stub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<RealTimePlotServiceBlockingV2Stub>() {
        @java.lang.Override
        public RealTimePlotServiceBlockingV2Stub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new RealTimePlotServiceBlockingV2Stub(channel, callOptions);
        }
      };
    return RealTimePlotServiceBlockingV2Stub.newStub(factory, channel);
  }

  /**
   * Creates a new blocking-style stub that supports unary and streaming output calls on the service
   */
  public static RealTimePlotServiceBlockingStub newBlockingStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<RealTimePlotServiceBlockingStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<RealTimePlotServiceBlockingStub>() {
        @java.lang.Override
        public RealTimePlotServiceBlockingStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new RealTimePlotServiceBlockingStub(channel, callOptions);
        }
      };
    return RealTimePlotServiceBlockingStub.newStub(factory, channel);
  }

  /**
   * Creates a new ListenableFuture-style stub that supports unary calls on the service
   */
  public static RealTimePlotServiceFutureStub newFutureStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<RealTimePlotServiceFutureStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<RealTimePlotServiceFutureStub>() {
        @java.lang.Override
        public RealTimePlotServiceFutureStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new RealTimePlotServiceFutureStub(channel, callOptions);
        }
      };
    return RealTimePlotServiceFutureStub.newStub(factory, channel);
  }

  /**
   */
  public interface AsyncService {

    /**
     */
    default io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData> streamSparklineData(
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> responseObserver) {
      return io.grpc.stub.ServerCalls.asyncUnimplementedStreamingCall(getStreamSparklineDataMethod(), responseObserver);
    }

    /**
     */
    default io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData> streamChromatogramSvgData(
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> responseObserver) {
      return io.grpc.stub.ServerCalls.asyncUnimplementedStreamingCall(getStreamChromatogramSvgDataMethod(), responseObserver);
    }

    /**
     */
    default void uploadSparklineData(ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getUploadSparklineDataMethod(), responseObserver);
    }

    /**
     */
    default void uploadChromatogramSvgData(ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getUploadChromatogramSvgDataMethod(), responseObserver);
    }
  }

  /**
   * Base class for the server implementation of the service RealTimePlotService.
   */
  public static abstract class RealTimePlotServiceImplBase
      implements io.grpc.BindableService, AsyncService {

    @java.lang.Override public final io.grpc.ServerServiceDefinition bindService() {
      return RealTimePlotServiceGrpc.bindService(this);
    }
  }

  /**
   * A stub to allow clients to do asynchronous rpc calls to service RealTimePlotService.
   */
  public static final class RealTimePlotServiceStub
      extends io.grpc.stub.AbstractAsyncStub<RealTimePlotServiceStub> {
    private RealTimePlotServiceStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected RealTimePlotServiceStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new RealTimePlotServiceStub(channel, callOptions);
    }

    /**
     */
    public io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData> streamSparklineData(
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> responseObserver) {
      return io.grpc.stub.ClientCalls.asyncBidiStreamingCall(
          getChannel().newCall(getStreamSparklineDataMethod(), getCallOptions()), responseObserver);
    }

    /**
     */
    public io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData> streamChromatogramSvgData(
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> responseObserver) {
      return io.grpc.stub.ClientCalls.asyncBidiStreamingCall(
          getChannel().newCall(getStreamChromatogramSvgDataMethod(), getCallOptions()), responseObserver);
    }

    /**
     */
    public void uploadSparklineData(ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> responseObserver) {
      io.grpc.stub.ClientCalls.asyncUnaryCall(
          getChannel().newCall(getUploadSparklineDataMethod(), getCallOptions()), request, responseObserver);
    }

    /**
     */
    public void uploadChromatogramSvgData(ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData request,
        io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> responseObserver) {
      io.grpc.stub.ClientCalls.asyncUnaryCall(
          getChannel().newCall(getUploadChromatogramSvgDataMethod(), getCallOptions()), request, responseObserver);
    }
  }

  /**
   * A stub to allow clients to do synchronous rpc calls to service RealTimePlotService.
   */
  public static final class RealTimePlotServiceBlockingV2Stub
      extends io.grpc.stub.AbstractBlockingStub<RealTimePlotServiceBlockingV2Stub> {
    private RealTimePlotServiceBlockingV2Stub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected RealTimePlotServiceBlockingV2Stub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new RealTimePlotServiceBlockingV2Stub(channel, callOptions);
    }

    /**
     */
    @io.grpc.ExperimentalApi("https://github.com/grpc/grpc-java/issues/10918")
    public io.grpc.stub.BlockingClientCall<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>
        streamSparklineData() {
      return io.grpc.stub.ClientCalls.blockingBidiStreamingCall(
          getChannel(), getStreamSparklineDataMethod(), getCallOptions());
    }

    /**
     */
    @io.grpc.ExperimentalApi("https://github.com/grpc/grpc-java/issues/10918")
    public io.grpc.stub.BlockingClientCall<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData, ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>
        streamChromatogramSvgData() {
      return io.grpc.stub.ClientCalls.blockingBidiStreamingCall(
          getChannel(), getStreamChromatogramSvgDataMethod(), getCallOptions());
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse uploadSparklineData(ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData request) throws io.grpc.StatusException {
      return io.grpc.stub.ClientCalls.blockingV2UnaryCall(
          getChannel(), getUploadSparklineDataMethod(), getCallOptions(), request);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse uploadChromatogramSvgData(ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData request) throws io.grpc.StatusException {
      return io.grpc.stub.ClientCalls.blockingV2UnaryCall(
          getChannel(), getUploadChromatogramSvgDataMethod(), getCallOptions(), request);
    }
  }

  /**
   * A stub to allow clients to do limited synchronous rpc calls to service RealTimePlotService.
   */
  public static final class RealTimePlotServiceBlockingStub
      extends io.grpc.stub.AbstractBlockingStub<RealTimePlotServiceBlockingStub> {
    private RealTimePlotServiceBlockingStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected RealTimePlotServiceBlockingStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new RealTimePlotServiceBlockingStub(channel, callOptions);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse uploadSparklineData(ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData request) {
      return io.grpc.stub.ClientCalls.blockingUnaryCall(
          getChannel(), getUploadSparklineDataMethod(), getCallOptions(), request);
    }

    /**
     */
    public ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse uploadChromatogramSvgData(ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData request) {
      return io.grpc.stub.ClientCalls.blockingUnaryCall(
          getChannel(), getUploadChromatogramSvgDataMethod(), getCallOptions(), request);
    }
  }

  /**
   * A stub to allow clients to do ListenableFuture-style rpc calls to service RealTimePlotService.
   */
  public static final class RealTimePlotServiceFutureStub
      extends io.grpc.stub.AbstractFutureStub<RealTimePlotServiceFutureStub> {
    private RealTimePlotServiceFutureStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected RealTimePlotServiceFutureStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new RealTimePlotServiceFutureStub(channel, callOptions);
    }

    /**
     */
    public com.google.common.util.concurrent.ListenableFuture<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> uploadSparklineData(
        ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData request) {
      return io.grpc.stub.ClientCalls.futureUnaryCall(
          getChannel().newCall(getUploadSparklineDataMethod(), getCallOptions()), request);
    }

    /**
     */
    public com.google.common.util.concurrent.ListenableFuture<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse> uploadChromatogramSvgData(
        ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData request) {
      return io.grpc.stub.ClientCalls.futureUnaryCall(
          getChannel().newCall(getUploadChromatogramSvgDataMethod(), getCallOptions()), request);
    }
  }

  private static final int METHODID_UPLOAD_SPARKLINE_DATA = 0;
  private static final int METHODID_UPLOAD_CHROMATOGRAM_SVG_DATA = 1;
  private static final int METHODID_STREAM_SPARKLINE_DATA = 2;
  private static final int METHODID_STREAM_CHROMATOGRAM_SVG_DATA = 3;

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
        case METHODID_UPLOAD_SPARKLINE_DATA:
          serviceImpl.uploadSparklineData((ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData) request,
              (io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>) responseObserver);
          break;
        case METHODID_UPLOAD_CHROMATOGRAM_SVG_DATA:
          serviceImpl.uploadChromatogramSvgData((ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData) request,
              (io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>) responseObserver);
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
        case METHODID_STREAM_SPARKLINE_DATA:
          return (io.grpc.stub.StreamObserver<Req>) serviceImpl.streamSparklineData(
              (io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>) responseObserver);
        case METHODID_STREAM_CHROMATOGRAM_SVG_DATA:
          return (io.grpc.stub.StreamObserver<Req>) serviceImpl.streamChromatogramSvgData(
              (io.grpc.stub.StreamObserver<ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>) responseObserver);
        default:
          throw new AssertionError();
      }
    }
  }

  public static final io.grpc.ServerServiceDefinition bindService(AsyncService service) {
    return io.grpc.ServerServiceDefinition.builder(getServiceDescriptor())
        .addMethod(
          getStreamSparklineDataMethod(),
          io.grpc.stub.ServerCalls.asyncBidiStreamingCall(
            new MethodHandlers<
              ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData,
              ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>(
                service, METHODID_STREAM_SPARKLINE_DATA)))
        .addMethod(
          getStreamChromatogramSvgDataMethod(),
          io.grpc.stub.ServerCalls.asyncBidiStreamingCall(
            new MethodHandlers<
              ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData,
              ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>(
                service, METHODID_STREAM_CHROMATOGRAM_SVG_DATA)))
        .addMethod(
          getUploadSparklineDataMethod(),
          io.grpc.stub.ServerCalls.asyncUnaryCall(
            new MethodHandlers<
              ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.SparklineData,
              ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>(
                service, METHODID_UPLOAD_SPARKLINE_DATA)))
        .addMethod(
          getUploadChromatogramSvgDataMethod(),
          io.grpc.stub.ServerCalls.asyncUnaryCall(
            new MethodHandlers<
              ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.ChromatogramSvgData,
              ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.PlotDataResponse>(
                service, METHODID_UPLOAD_CHROMATOGRAM_SVG_DATA)))
        .build();
  }

  private static abstract class RealTimePlotServiceBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoFileDescriptorSupplier, io.grpc.protobuf.ProtoServiceDescriptorSupplier {
    RealTimePlotServiceBaseDescriptorSupplier() {}

    @java.lang.Override
    public com.google.protobuf.Descriptors.FileDescriptor getFileDescriptor() {
      return ThermoFisher.AcquisitionModule.Contracts.RawDataAccess.getDescriptor();
    }

    @java.lang.Override
    public com.google.protobuf.Descriptors.ServiceDescriptor getServiceDescriptor() {
      return getFileDescriptor().findServiceByName("RealTimePlotService");
    }
  }

  private static final class RealTimePlotServiceFileDescriptorSupplier
      extends RealTimePlotServiceBaseDescriptorSupplier {
    RealTimePlotServiceFileDescriptorSupplier() {}
  }

  private static final class RealTimePlotServiceMethodDescriptorSupplier
      extends RealTimePlotServiceBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoMethodDescriptorSupplier {
    private final java.lang.String methodName;

    RealTimePlotServiceMethodDescriptorSupplier(java.lang.String methodName) {
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
      synchronized (RealTimePlotServiceGrpc.class) {
        result = serviceDescriptor;
        if (result == null) {
          serviceDescriptor = result = io.grpc.ServiceDescriptor.newBuilder(SERVICE_NAME)
              .setSchemaDescriptor(new RealTimePlotServiceFileDescriptorSupplier())
              .addMethod(getStreamSparklineDataMethod())
              .addMethod(getStreamChromatogramSvgDataMethod())
              .addMethod(getUploadSparklineDataMethod())
              .addMethod(getUploadChromatogramSvgDataMethod())
              .build();
        }
      }
    }
    return result;
  }
}
