package com.example.projects.poc.grpc.helpers;

import org.testng.ITestContext;
import org.testng.ITestListener;
import org.testng.ITestResult;
import io.qameta.allure.Allure;

import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.List;
import java.util.Map;
import java.util.ArrayList;
import java.util.LinkedHashMap;

/**
 * TestNG listener that drains buffered gRPC captures and adds them to Allure attachments
 * at the end of each test. Attachments are created from the test thread to ensure
 * they map correctly to the test in Allure.
 */
public class GrpcAllureTestListener implements ITestListener {

    private String buildTestId(ITestResult result) {
        // a deterministic test id - can be customized
        return result.getTestClass().getName() + "#" + result.getMethod().getMethodName() + "@" + result.getStartMillis();
    }

    @Override
    public void onTestStart(ITestResult result) {
        String id = buildTestId(result);
        TestContext.setCurrentTestId(id);
    }

    @Override
    public void onTestSuccess(ITestResult result) {
        drainAndAttach(result);
        TestContext.clear();
    }

    @Override
    public void onTestFailure(ITestResult result) {
        drainAndAttach(result);
        TestContext.clear();
    }

    @Override
    public void onTestSkipped(ITestResult result) {
        drainAndAttach(result);
        TestContext.clear();
    }

    @Override
    public void onFinish(ITestContext context) {
        // nothing
    }

    @Override public void onStart(ITestContext context) {}
    @Override public void onTestFailedButWithinSuccessPercentage(ITestResult result) { }

    private void drainAndAttach(ITestResult result) {
        String id = TestContext.getCurrentTestId();
        long start = result.getStartMillis();

        List<GrpcClientAllureInterceptor.Capture> captures = null;
        // try by testId first
        if (id != null) {
            captures = GrpcClientAllureInterceptor.drainCapturesForTest(id);
        }
        if (captures == null || captures.isEmpty()) {
            // fallback: drain by timestamp
            captures = GrpcClientAllureInterceptor.drainCapturesSince(start);
        }

        if (captures == null || captures.isEmpty()) return;

        // Group captures by RPC name so we can create a parent step per RPC and attach
        // each captured message as a child step/attachment for better navigation.
        // Group captures by full RPC method name
        Map<String, List<GrpcClientAllureInterceptor.Capture>> grouped = new LinkedHashMap<>();
        for (GrpcClientAllureInterceptor.Capture c : captures) {
            String rpc = c.rpcName != null ? c.rpcName : "<unknown>";
            grouped.computeIfAbsent(rpc, k -> new ArrayList<>()).add(c);
        }

        for (Map.Entry<String, List<GrpcClientAllureInterceptor.Capture>> entry : grouped.entrySet()) {
            final String rpcName = entry.getKey();
            final List<GrpcClientAllureInterceptor.Capture> rpcCaptures = entry.getValue();
            Allure.step("gRPC method: " + rpcName + " (" + rpcCaptures.size() + " messages)", () -> {
                // Build a list of original payload objects where available (protobuf Messages),
                // otherwise fall back to the captured JSON string. This lets CommonUtilities
                // produce the richest possible summary (JSON from protobuf when possible).
                List<Object> payloadObjects = new ArrayList<>();
                for (GrpcClientAllureInterceptor.Capture c : rpcCaptures) {
                    if (c.payloadObject != null) payloadObjects.add(c.payloadObject);
                    else payloadObjects.add(c.payloadJson == null ? "null" : c.payloadJson);
                }

                try {
                    String summary = com.example.projects.poc.commons.CommonUtilities.createStreamingSummary(payloadObjects);
                    try (InputStream is = new ByteArrayInputStream(summary.getBytes(StandardCharsets.UTF_8))) {
                        Allure.addAttachment(rpcName + " - streaming summary", "application/json", is, ".json");
                    }
                } catch (Exception ex) {
                    Allure.step("Failed to create streaming summary attachment: " + ex.getMessage());
                }
            });
        }
    }
}
