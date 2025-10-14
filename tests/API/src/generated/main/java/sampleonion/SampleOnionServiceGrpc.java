package sampleonion;

import static io.grpc.MethodDescriptor.generateFullMethodName;

/**
 */
@io.grpc.stub.annotations.GrpcGenerated
public final class SampleOnionServiceGrpc {

  private SampleOnionServiceGrpc() {}

  public static final java.lang.String SERVICE_NAME = "sampleonion.SampleOnionService";

  // Static method descriptors that strictly reflect the proto.
  private static volatile io.grpc.MethodDescriptor<sampleonion.SampleOnionModule.GetCompoundsRequest,
      sampleonion.SampleOnionModule.GetCompoundsResponse> getGetCompoundsMethod;

  @io.grpc.stub.annotations.RpcMethod(
      fullMethodName = SERVICE_NAME + '/' + "GetCompounds",
      requestType = sampleonion.SampleOnionModule.GetCompoundsRequest.class,
      responseType = sampleonion.SampleOnionModule.GetCompoundsResponse.class,
      methodType = io.grpc.MethodDescriptor.MethodType.UNARY)
  public static io.grpc.MethodDescriptor<sampleonion.SampleOnionModule.GetCompoundsRequest,
      sampleonion.SampleOnionModule.GetCompoundsResponse> getGetCompoundsMethod() {
    io.grpc.MethodDescriptor<sampleonion.SampleOnionModule.GetCompoundsRequest, sampleonion.SampleOnionModule.GetCompoundsResponse> getGetCompoundsMethod;
    if ((getGetCompoundsMethod = SampleOnionServiceGrpc.getGetCompoundsMethod) == null) {
      synchronized (SampleOnionServiceGrpc.class) {
        if ((getGetCompoundsMethod = SampleOnionServiceGrpc.getGetCompoundsMethod) == null) {
          SampleOnionServiceGrpc.getGetCompoundsMethod = getGetCompoundsMethod =
              io.grpc.MethodDescriptor.<sampleonion.SampleOnionModule.GetCompoundsRequest, sampleonion.SampleOnionModule.GetCompoundsResponse>newBuilder()
              .setType(io.grpc.MethodDescriptor.MethodType.UNARY)
              .setFullMethodName(generateFullMethodName(SERVICE_NAME, "GetCompounds"))
              .setSampledToLocalTracing(true)
              .setRequestMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  sampleonion.SampleOnionModule.GetCompoundsRequest.getDefaultInstance()))
              .setResponseMarshaller(io.grpc.protobuf.ProtoUtils.marshaller(
                  sampleonion.SampleOnionModule.GetCompoundsResponse.getDefaultInstance()))
              .setSchemaDescriptor(new SampleOnionServiceMethodDescriptorSupplier("GetCompounds"))
              .build();
        }
      }
    }
    return getGetCompoundsMethod;
  }

  /**
   * Creates a new async stub that supports all call types for the service
   */
  public static SampleOnionServiceStub newStub(io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<SampleOnionServiceStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<SampleOnionServiceStub>() {
        @java.lang.Override
        public SampleOnionServiceStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new SampleOnionServiceStub(channel, callOptions);
        }
      };
    return SampleOnionServiceStub.newStub(factory, channel);
  }

  /**
   * Creates a new blocking-style stub that supports all types of calls on the service
   */
  public static SampleOnionServiceBlockingV2Stub newBlockingV2Stub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<SampleOnionServiceBlockingV2Stub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<SampleOnionServiceBlockingV2Stub>() {
        @java.lang.Override
        public SampleOnionServiceBlockingV2Stub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new SampleOnionServiceBlockingV2Stub(channel, callOptions);
        }
      };
    return SampleOnionServiceBlockingV2Stub.newStub(factory, channel);
  }

  /**
   * Creates a new blocking-style stub that supports unary and streaming output calls on the service
   */
  public static SampleOnionServiceBlockingStub newBlockingStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<SampleOnionServiceBlockingStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<SampleOnionServiceBlockingStub>() {
        @java.lang.Override
        public SampleOnionServiceBlockingStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new SampleOnionServiceBlockingStub(channel, callOptions);
        }
      };
    return SampleOnionServiceBlockingStub.newStub(factory, channel);
  }

  /**
   * Creates a new ListenableFuture-style stub that supports unary calls on the service
   */
  public static SampleOnionServiceFutureStub newFutureStub(
      io.grpc.Channel channel) {
    io.grpc.stub.AbstractStub.StubFactory<SampleOnionServiceFutureStub> factory =
      new io.grpc.stub.AbstractStub.StubFactory<SampleOnionServiceFutureStub>() {
        @java.lang.Override
        public SampleOnionServiceFutureStub newStub(io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
          return new SampleOnionServiceFutureStub(channel, callOptions);
        }
      };
    return SampleOnionServiceFutureStub.newStub(factory, channel);
  }

  /**
   */
  public interface AsyncService {

    /**
     */
    default void getCompounds(sampleonion.SampleOnionModule.GetCompoundsRequest request,
        io.grpc.stub.StreamObserver<sampleonion.SampleOnionModule.GetCompoundsResponse> responseObserver) {
      io.grpc.stub.ServerCalls.asyncUnimplementedUnaryCall(getGetCompoundsMethod(), responseObserver);
    }
  }

  /**
   * Base class for the server implementation of the service SampleOnionService.
   */
  public static abstract class SampleOnionServiceImplBase
      implements io.grpc.BindableService, AsyncService {

    @java.lang.Override public final io.grpc.ServerServiceDefinition bindService() {
      return SampleOnionServiceGrpc.bindService(this);
    }
  }

  /**
   * A stub to allow clients to do asynchronous rpc calls to service SampleOnionService.
   */
  public static final class SampleOnionServiceStub
      extends io.grpc.stub.AbstractAsyncStub<SampleOnionServiceStub> {
    private SampleOnionServiceStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected SampleOnionServiceStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new SampleOnionServiceStub(channel, callOptions);
    }

    /**
     */
    public void getCompounds(sampleonion.SampleOnionModule.GetCompoundsRequest request,
        io.grpc.stub.StreamObserver<sampleonion.SampleOnionModule.GetCompoundsResponse> responseObserver) {
      io.grpc.stub.ClientCalls.asyncUnaryCall(
          getChannel().newCall(getGetCompoundsMethod(), getCallOptions()), request, responseObserver);
    }
  }

  /**
   * A stub to allow clients to do synchronous rpc calls to service SampleOnionService.
   */
  public static final class SampleOnionServiceBlockingV2Stub
      extends io.grpc.stub.AbstractBlockingStub<SampleOnionServiceBlockingV2Stub> {
    private SampleOnionServiceBlockingV2Stub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected SampleOnionServiceBlockingV2Stub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new SampleOnionServiceBlockingV2Stub(channel, callOptions);
    }

    /**
     */
    public sampleonion.SampleOnionModule.GetCompoundsResponse getCompounds(sampleonion.SampleOnionModule.GetCompoundsRequest request) throws io.grpc.StatusException {
      return io.grpc.stub.ClientCalls.blockingV2UnaryCall(
          getChannel(), getGetCompoundsMethod(), getCallOptions(), request);
    }
  }

  /**
   * A stub to allow clients to do limited synchronous rpc calls to service SampleOnionService.
   */
  public static final class SampleOnionServiceBlockingStub
      extends io.grpc.stub.AbstractBlockingStub<SampleOnionServiceBlockingStub> {
    private SampleOnionServiceBlockingStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected SampleOnionServiceBlockingStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new SampleOnionServiceBlockingStub(channel, callOptions);
    }

    /**
     */
    public sampleonion.SampleOnionModule.GetCompoundsResponse getCompounds(sampleonion.SampleOnionModule.GetCompoundsRequest request) {
      return io.grpc.stub.ClientCalls.blockingUnaryCall(
          getChannel(), getGetCompoundsMethod(), getCallOptions(), request);
    }
  }

  /**
   * A stub to allow clients to do ListenableFuture-style rpc calls to service SampleOnionService.
   */
  public static final class SampleOnionServiceFutureStub
      extends io.grpc.stub.AbstractFutureStub<SampleOnionServiceFutureStub> {
    private SampleOnionServiceFutureStub(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      super(channel, callOptions);
    }

    @java.lang.Override
    protected SampleOnionServiceFutureStub build(
        io.grpc.Channel channel, io.grpc.CallOptions callOptions) {
      return new SampleOnionServiceFutureStub(channel, callOptions);
    }

    /**
     */
    public com.google.common.util.concurrent.ListenableFuture<sampleonion.SampleOnionModule.GetCompoundsResponse> getCompounds(
        sampleonion.SampleOnionModule.GetCompoundsRequest request) {
      return io.grpc.stub.ClientCalls.futureUnaryCall(
          getChannel().newCall(getGetCompoundsMethod(), getCallOptions()), request);
    }
  }

  private static final int METHODID_GET_COMPOUNDS = 0;

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
        case METHODID_GET_COMPOUNDS:
          serviceImpl.getCompounds((sampleonion.SampleOnionModule.GetCompoundsRequest) request,
              (io.grpc.stub.StreamObserver<sampleonion.SampleOnionModule.GetCompoundsResponse>) responseObserver);
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
          getGetCompoundsMethod(),
          io.grpc.stub.ServerCalls.asyncUnaryCall(
            new MethodHandlers<
              sampleonion.SampleOnionModule.GetCompoundsRequest,
              sampleonion.SampleOnionModule.GetCompoundsResponse>(
                service, METHODID_GET_COMPOUNDS)))
        .build();
  }

  private static abstract class SampleOnionServiceBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoFileDescriptorSupplier, io.grpc.protobuf.ProtoServiceDescriptorSupplier {
    SampleOnionServiceBaseDescriptorSupplier() {}

    @java.lang.Override
    public com.google.protobuf.Descriptors.FileDescriptor getFileDescriptor() {
      return sampleonion.SampleOnionModule.getDescriptor();
    }

    @java.lang.Override
    public com.google.protobuf.Descriptors.ServiceDescriptor getServiceDescriptor() {
      return getFileDescriptor().findServiceByName("SampleOnionService");
    }
  }

  private static final class SampleOnionServiceFileDescriptorSupplier
      extends SampleOnionServiceBaseDescriptorSupplier {
    SampleOnionServiceFileDescriptorSupplier() {}
  }

  private static final class SampleOnionServiceMethodDescriptorSupplier
      extends SampleOnionServiceBaseDescriptorSupplier
      implements io.grpc.protobuf.ProtoMethodDescriptorSupplier {
    private final java.lang.String methodName;

    SampleOnionServiceMethodDescriptorSupplier(java.lang.String methodName) {
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
      synchronized (SampleOnionServiceGrpc.class) {
        result = serviceDescriptor;
        if (result == null) {
          serviceDescriptor = result = io.grpc.ServiceDescriptor.newBuilder(SERVICE_NAME)
              .setSchemaDescriptor(new SampleOnionServiceFileDescriptorSupplier())
              .addMethod(getGetCompoundsMethod())
              .build();
        }
      }
    }
    return result;
  }
}
