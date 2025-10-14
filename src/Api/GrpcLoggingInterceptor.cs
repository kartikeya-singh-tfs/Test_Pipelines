using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Threading.Tasks;
using System;

namespace Thermofisher.Opal.Api
{
    public class GrpcLoggingInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"[gRPC] Method: {context.Method}, Request: {request}");
            var response = await continuation(request, context);
            Console.WriteLine($"[gRPC] Response: {response}");
            return response;
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            Console.WriteLine($"[gRPC] Streaming Method: {context.Method}, Request: {request}");
            await continuation(request, responseStream, context);
        }
    }
}
