package com.example.projects.poc.grpc.helpers;

import io.qameta.allure.*;
import io.grpc.Status;
import io.grpc.StatusRuntimeException;
import io.grpc.stub.StreamObserver;
import com.google.protobuf.Message;

import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;

/**
 * Allure helper class for capturing detailed gRPC call information
 */
public class GrpcAllureHelper {

    /**
     * Wrap a blocking gRPC call with Allure reporting
     */
    @Step("gRPC Unary Call: {methodName}")
    public static <T extends Message, R extends Message> R executeBlockingCall(
            String methodName,
            T request,
            GrpcCallExecutor<T, R> executor) {
        
        try {
            // Attach request details
            attachGrpcRequest(methodName, request);
            
            // Execute the call
            long startTime = System.currentTimeMillis();
            R response = executor.execute(request);
            long duration = System.currentTimeMillis() - startTime;
            
            // Attach response details and timing
            attachGrpcResponse(methodName, response);
            Allure.parameter("Duration (ms)", duration);
            
            // Mark as successful
            Allure.step("✅ gRPC call completed successfully in " + duration + "ms");
            return response;
            
        } catch (StatusRuntimeException e) {
            // Attach error details
            attachGrpcError(methodName, e);
            Allure.step("❌ gRPC call failed: " + e.getStatus().getCode());
            throw e;
        } catch (Exception e) {
            // Attach generic error
            attachGenericError(methodName, e);
            Allure.step("❌ gRPC call failed: " + e.getMessage());
            throw e;
        }
    }

    /**
     * Wrap a server streaming gRPC call with Allure reporting
     */
    @Step("gRPC Server Streaming: {methodName}")
    public static <T extends Message, R extends Message> List<R> executeServerStreaming(
            String methodName,
            T request,
            GrpcStreamingExecutor<T, R> executor,
            long timeoutSeconds) throws InterruptedException {
        
        try {
            // Attach request details
            attachGrpcRequest(methodName, request);
            
            // Collect streaming responses
            List<R> responses = new CopyOnWriteArrayList<>();
            CountDownLatch completedLatch = new CountDownLatch(1);
            Exception[] errorHolder = new Exception[1];
            long startTime = System.currentTimeMillis();
            
            StreamObserver<R> responseObserver = new StreamObserver<R>() {
                @Override
                public void onNext(R value) {
                    responses.add(value);
                    Allure.step("📨 Received streaming response #" + responses.size(), 
                        () -> attachGrpcStreamingResponse(methodName, value, responses.size()));
                }
                
                @Override
                public void onError(Throwable t) {
                    errorHolder[0] = (Exception) t;
                    attachGrpcError(methodName, t);
                    completedLatch.countDown();
                }
                
                @Override
                public void onCompleted() {
                    long duration = System.currentTimeMillis() - startTime;
                    Allure.step("✅ Server streaming completed - received " + responses.size() + 
                              " responses in " + duration + "ms");
                    completedLatch.countDown();
                }
            };
            
            // Execute the streaming call
            executor.execute(request, responseObserver);
            
            // Wait for completion
            boolean completed = completedLatch.await(timeoutSeconds, TimeUnit.SECONDS);
            
            if (!completed) {
                throw new RuntimeException("⏰ Server streaming timed out after " + timeoutSeconds + " seconds");
            }
            
            if (errorHolder[0] != null) {
                throw new RuntimeException("❌ Server streaming failed", errorHolder[0]);
            }
            
            // Attach summary
            attachStreamingSummary(methodName, responses);
            
            return responses;
            
        } catch (Exception e) {
            attachGrpcError(methodName, e);
            throw e;
        }
    }

    /**
     * Helper method to create sequence state events with Allure reporting
     */
    @Step("📤 Send Sequence State: {state}")
    public static void sendSequenceStateWithLogging(
            String methodName,
            io.grpc.stub.AbstractBlockingStub<?> stub,
            Object state,
            String sequenceId) {
        
        Allure.parameter("Sequence ID", sequenceId);
        Allure.parameter("State", state.toString());
        
        // This will be implemented by the caller with the actual gRPC call
        Allure.step("Executing sequence state transition...");
    }

    /**
     * Helper method for acquisition state events
     */
    @Step("📤 Send Acquisition State: {state}")
    public static void sendAcquisitionStateWithLogging(
            String methodName,
            io.grpc.stub.AbstractBlockingStub<?> stub,
            Object state,
            String sampleId) {
        
        Allure.parameter("Sample ID", sampleId);
        Allure.parameter("State", state.toString());
        
        Allure.step("Executing acquisition state transition...");
    }

    /**
     * Helper method for device state events
     */
    @Step("📤 Send Device State: {state}")
    public static void sendDeviceStateWithLogging(
            String methodName,
            io.grpc.stub.AbstractBlockingStub<?> stub,
            Object state,
            String deviceId) {
        
        Allure.parameter("Device ID", deviceId);
        Allure.parameter("State", state.toString());
        
        Allure.step("Executing device state transition...");
    }

    // Attachment methods
    @Attachment(value = "🔄 gRPC Request: {methodName}", type = "application/json")
    public static String attachGrpcRequest(String methodName, Message request) {
        try {
            // Simple JSON-like format since JsonFormat might not be available
            return formatProtobufAsJson(request);
        } catch (Exception e) {
            return "Failed to serialize request: " + e.getMessage() + "\n\nRaw protobuf:\n" + request.toString();
        }
    }

    @Attachment(value = "✅ gRPC Response: {methodName}", type = "application/json")
    public static String attachGrpcResponse(String methodName, Message response) {
        try {
            return formatProtobufAsJson(response);
        } catch (Exception e) {
            return "Failed to serialize response: " + e.getMessage() + "\n\nRaw protobuf:\n" + response.toString();
        }
    }

    @Attachment(value = "📨 Streaming Response #{responseNum}", type = "application/json")
    public static String attachGrpcStreamingResponse(String methodName, Message response, int responseNum) {
        try {
            return formatProtobufAsJson(response);
        } catch (Exception e) {
            return "Failed to serialize streaming response: " + e.getMessage() + "\n\nRaw protobuf:\n" + response.toString();
        }
    }
    
    /**
     * Simple protobuf to JSON-like format converter
     */
    private static String formatProtobufAsJson(Message message) {
        // For now, just return the toString() with better formatting
        String raw = message.toString();
        return "{\n  \"protobuf_type\": \"" + message.getClass().getSimpleName() + "\",\n" +
               "  \"content\": " + raw.replace("\n", "\\n").replace("\"", "\\\"") + "\n}";
    }

    @Attachment(value = "❌ gRPC Error Details", type = "text/plain")
    public static String attachGrpcError(String methodName, Throwable error) {
        StringBuilder errorDetails = new StringBuilder();
        errorDetails.append("=== gRPC Error Report ===\n");
        errorDetails.append("Method: ").append(methodName).append("\n");
        errorDetails.append("Error Type: ").append(error.getClass().getSimpleName()).append("\n");
        errorDetails.append("Message: ").append(error.getMessage()).append("\n");
        
        if (error instanceof StatusRuntimeException) {
            StatusRuntimeException sre = (StatusRuntimeException) error;
            errorDetails.append("gRPC Status Code: ").append(sre.getStatus().getCode()).append("\n");
            errorDetails.append("gRPC Description: ").append(sre.getStatus().getDescription()).append("\n");
            errorDetails.append("gRPC Cause: ").append(sre.getStatus().getCause()).append("\n");
            
            // Add troubleshooting hints based on common gRPC errors
            Status.Code code = sre.getStatus().getCode();
            if (code == Status.Code.UNAVAILABLE) {
                errorDetails.append("\n💡 Troubleshooting: Server may be down or unreachable\n");
            } else if (code == Status.Code.DEADLINE_EXCEEDED) {
                errorDetails.append("\n💡 Troubleshooting: Request timed out - consider increasing timeout\n");
            } else if (code == Status.Code.UNAUTHENTICATED) {
                errorDetails.append("\n💡 Troubleshooting: Authentication required or invalid credentials\n");
            } else if (code == Status.Code.PERMISSION_DENIED) {
                errorDetails.append("\n💡 Troubleshooting: Insufficient permissions for this operation\n");
            } else if (code == Status.Code.NOT_FOUND) {
                errorDetails.append("\n💡 Troubleshooting: Resource not found or method not implemented\n");
            } else {
                errorDetails.append("\n💡 Troubleshooting: Check server logs for more details\n");
            }
        }
        
        errorDetails.append("\n=== Stack Trace ===\n");
        for (StackTraceElement element : error.getStackTrace()) {
            errorDetails.append("  ").append(element.toString()).append("\n");
        }
        
        return errorDetails.toString();
    }

    @Attachment(value = "❌ Generic Error", type = "text/plain")
    public static String attachGenericError(String methodName, Exception error) {
        StringBuilder errorDetails = new StringBuilder();
        errorDetails.append("=== Error Report ===\n");
        errorDetails.append("Method: ").append(methodName).append("\n");
        errorDetails.append("Error Type: ").append(error.getClass().getSimpleName()).append("\n");
        errorDetails.append("Message: ").append(error.getMessage()).append("\n");
        
        errorDetails.append("\n=== Stack Trace ===\n");
        for (StackTraceElement element : error.getStackTrace()) {
            errorDetails.append("  ").append(element.toString()).append("\n");
        }
        
        return errorDetails.toString();
    }

    @Attachment(value = "📊 Streaming Summary", type = "application/json")
    public static String attachStreamingSummary(String methodName, List<? extends Message> responses) {
        StringBuilder summary = new StringBuilder();
        summary.append("{\n");
        summary.append("  \"methodName\": \"").append(methodName).append("\",\n");
        summary.append("  \"totalResponses\": ").append(responses.size()).append(",\n");
        summary.append("  \"success\": true,\n");
        summary.append("  \"responseTypes\": [\n");
        
        for (int i = 0; i < responses.size(); i++) {
            if (i > 0) summary.append(",\n");
            summary.append("    {\n");
            summary.append("      \"index\": ").append(i + 1).append(",\n");
            summary.append("      \"type\": \"").append(responses.get(i).getClass().getSimpleName()).append("\"\n");
            summary.append("    }");
        }
        
        summary.append("\n  ]\n}");
        return summary.toString();
    }

    // Functional interfaces for different call types
    @FunctionalInterface
    public interface GrpcCallExecutor<T extends Message, R extends Message> {
        R execute(T request) throws StatusRuntimeException;
    }

    @FunctionalInterface
    public interface GrpcStreamingExecutor<T extends Message, R extends Message> {
        void execute(T request, StreamObserver<R> responseObserver);
    }
}