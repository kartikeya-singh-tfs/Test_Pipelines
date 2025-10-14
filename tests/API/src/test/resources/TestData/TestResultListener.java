package utils;

import com.example.framework.PayloadBodies.ado.AdoPayloads;
import com.example.framework.PayloadBodies.ado.TestCase;
import io.qameta.allure.Description;
import io.restassured.response.Response;
import io.restassured.specification.RequestSpecification;
import org.json.JSONArray;
import org.json.JSONObject;
import org.junit.platform.engine.TestExecutionResult;
import org.junit.platform.engine.TestSource;
import org.junit.platform.engine.support.descriptor.MethodSource;
import org.junit.platform.launcher.TestExecutionListener;
import org.junit.platform.launcher.TestIdentifier;

import java.lang.reflect.Method;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

import static io.restassured.RestAssured.given;

public class TestResultListener implements TestExecutionListener {
    private Response response;
    private final Map<String, Class<?>> classCache = new HashMap<>();
    private int testCaseId = 0;
    private int testPointId = 0;
    private int testSuiteId = 0;
    private String testDescription = "";
    private String errorMessage = "";
    //private final Endpoints API_STEPS = new Endpoints();

    public <T> Response azureDevOpsEndpoint(T body, String method, String endpoint, String contentType) {
        RequestSpecification requestSpecification = given()
                .log().all()
                .baseUri("https://dev.azure.com/")
                .contentType(contentType)
                .header("Authorization", "Basic OkY5eFN3amh6NVFIeVNSbW9OYWs5Wmd6MXFvZFNFY01MMnpsT0h0VXFYenJmV2Y5cGxrZ3JKUVFKOTlBS0FDQUFBQUE2a2JqZUFBQVNBWkRPa3BVbA==");

        if (body != null) {
            requestSpecification.body(body);
        }

        return requestSpecification
                .when()
                .request(method, endpoint)
                .then()
                .log().all()
                .extract()
                .response();
    }

    @Override
    public void executionSkipped(TestIdentifier testIdentifier, String reason) {
        reset();

        if (testIdentifier.isTest() && doNotSkipProcessing(testIdentifier)) {
            getTestAnnotations(testIdentifier);
            updateDescription();
/*
    @Override
    public void executionSkipped(TestIdentifier testIdentifier, String reason) {
        reset();

        if (testIdentifier.isTest() && doNotSkipProcessing(testIdentifier)) {
            getTestAnnotations(testIdentifier);
            updateDescription();
            updateAdoResult("NotApplicable");
        }
    }

    @Override
    public void executionFinished(TestIdentifier testIdentifier, TestExecutionResult testExecutionResult) {
        reset();

        if (testIdentifier.isTest() && doNotSkipProcessing(testIdentifier)) {
            TestExecutionResult.Status resultStatus = testExecutionResult.getStatus();

            testExecutionResult.getThrowable().ifPresent(throwable -> {
                if (testExecutionResult.getStatus() == TestExecutionResult.Status.FAILED) {
                    errorMessage = throwable.getMessage();
                } else if (testExecutionResult.getStatus() == TestExecutionResult.Status.ABORTED) {
                    errorMessage = throwable.getMessage();
                }
            });

            getTestAnnotations(testIdentifier);
            updateDescription();

            switch (resultStatus) {
                case SUCCESSFUL -> updateAdoResult("Passed");
                case FAILED -> updateAdoResult("Failed");
                default -> updateAdoResult("NotApplicable");
            }
        }
    }
*/
                return testClass == null || !testClass.isAnnotationPresent(null);
            } catch (Exception e) {
                System.err.println("Error checking for NoTestResultListener annotation: " + e.getMessage());
            }
        }
        return true;
    }

    private Class<?> getOrLoadClass(String className) {
        return classCache.computeIfAbsent(className, name -> {
            try {
                return Class.forName(name);
            } catch (ClassNotFoundException e) {
                System.err.println("Class not found: " + name);
                return null;
            }
        });
    }

    private void getTestAnnotations(TestIdentifier testIdentifier) {
        testIdentifier.getSource().ifPresent(source -> {
            if (source instanceof MethodSource methodSource) {
                try {
                    String className = methodSource.getClassName();
                    String methodName = methodSource.getMethodName();

                    Class<?> testClass = getOrLoadClass(className);
                    if (testClass != null) {
                        for (Method method : testClass.getDeclaredMethods()) {
                            if (method.getName().equals(methodName)) {
                                TestCase testCase = method.getAnnotation(TestCase.class);
                                if (testCase != null) {
                                    testCaseId = testCase.value();
                                }

                                Description description = method.getAnnotation(Description.class);
                                if (description != null) {
                                    testDescription = description.value();
                                }

                                break;
                            }
                        }
                    }
                } catch (Exception e) {
                    System.err.println("Error retrieving annotations: " + e.getMessage());
                    e.printStackTrace();
                }
            }
        });
    /*
    package utils;

    import com.example.framework.PayloadBodies.ado.AdoPayloads;
    import com.example.framework.PayloadBodies.ado.TestCase;
    import io.qameta.allure.Description;
    import io.restassured.response.Response;
    import io.restassured.specification.RequestSpecification;
    import org.json.JSONArray;
    import org.json.JSONObject;
    import org.junit.platform.engine.TestExecutionResult;
    import org.junit.platform.engine.TestSource;
    import org.junit.platform.engine.support.descriptor.MethodSource;
    import org.junit.platform.launcher.TestExecutionListener;
    import org.junit.platform.launcher.TestIdentifier;

    import java.lang.reflect.Method;
    import java.util.HashMap;
    import java.util.Map;
    import java.util.Optional;

    import static io.restassured.RestAssured.given;

    public class TestResultListener implements TestExecutionListener {
        private Response response;
        private final Map<String, Class<?>> classCache = new HashMap<>();
        private int testCaseId = 0;
        private int testPointId = 0;
        private int testSuiteId = 0;
        private String testDescription = "";
        private String errorMessage = "";
        //private final Endpoints API_STEPS = new Endpoints();

        public <T> Response azureDevOpsEndpoint(T body, String method, String endpoint, String contentType) {
            RequestSpecification requestSpecification = given()
                    .log().all()
                    .baseUri("https://dev.azure.com/")
                    .contentType(contentType)
                    .header("Authorization", "Basic OkY5eFN3amh6NVFIeVNSbW9OYWs5Wmd6MXFvZFNFY01MMnpsT0h0VXFYenJmV2Y5cGxrZ3JKUVFKOTlBS0FDQUFBQUE2a2JqZUFBQVNBWkRPa3BVbA==");

            if (body != null) {
                requestSpecification.body(body);
            }

            return requestSpecification
                    .when()
                    .request(method, endpoint)
                    .then()
                    .log().all()
                    .extract()
                    .response();
        }

        @Override
        public void executionSkipped(TestIdentifier testIdentifier, String reason) {
            reset();

            if (testIdentifier.isTest() && doNotSkipProcessing(testIdentifier)) {
                getTestAnnotations(testIdentifier);
                updateDescription();
                updateAdoResult("NotApplicable");
            }
        }

        @Override
        public void executionFinished(TestIdentifier testIdentifier, TestExecutionResult testExecutionResult) {
            reset();

            if (testIdentifier.isTest() && doNotSkipProcessing(testIdentifier)) {
                TestExecutionResult.Status resultStatus = testExecutionResult.getStatus();

                testExecutionResult.getThrowable().ifPresent(throwable -> {
                    if (testExecutionResult.getStatus() == TestExecutionResult.Status.FAILED) {
                        errorMessage = throwable.getMessage();
                    } else if (testExecutionResult.getStatus() == TestExecutionResult.Status.ABORTED) {
                        errorMessage = throwable.getMessage();
                    }
                });

                getTestAnnotations(testIdentifier);
                updateDescription();

                switch (resultStatus) {
                    case SUCCESSFUL -> updateAdoResult("Passed");
                    case FAILED -> updateAdoResult("Failed");
                    default -> updateAdoResult("NotApplicable");
                }
            }
        }

        private boolean doNotSkipProcessing(TestIdentifier testIdentifier) {
            Optional<TestSource> testSource = testIdentifier.getSource();
            if (testSource.isPresent() && testSource.get() instanceof MethodSource methodSource) {
                String className = methodSource.getClassName();
                try {
                    Class<?> testClass = getOrLoadClass(className);
                    return testClass == null || !testClass.isAnnotationPresent(null);
                } catch (Exception e) {
                    System.err.println("Error checking for NoTestResultListener annotation: " + e.getMessage());
                }
            }
            return true;
        }

        private Class<?> getOrLoadClass(String className) {
            return classCache.computeIfAbsent(className, name -> {
                try {
                    return Class.forName(name);
                } catch (ClassNotFoundException e) {
                    System.err.println("Class not found: " + name);
                    return null;
                }
            });
        }

        private void getTestAnnotations(TestIdentifier testIdentifier) {
            testIdentifier.getSource().ifPresent(source -> {
                if (source instanceof MethodSource methodSource) {
                    try {
                        String className = methodSource.getClassName();
                        String methodName = methodSource.getMethodName();

                        Class<?> testClass = getOrLoadClass(className);
                        if (testClass != null) {
                            for (Method method : testClass.getDeclaredMethods()) {
                                if (method.getName().equals(methodName)) {
                                    TestCase testCase = method.getAnnotation(TestCase.class);
                                    if (testCase != null) {
                                        testCaseId = testCase.value();
                                    }

                                    Description description = method.getAnnotation(Description.class);
                                    if (description != null) {
                                        testDescription = description.value();
                                    }

                                    break;
                                }
                            }
                        }
                    } catch (Exception e) {
                        System.err.println("Error retrieving annotations: " + e.getMessage());
                        e.printStackTrace();
                    }
                }
            });
        }

        public void reset() {
            testCaseId = 0;
            testPointId = 0;
            testSuiteId = 0;
            testDescription = "";
            errorMessage = "";
        }

        public void updateDescription() {
            if (System.getProperty("update_description").equalsIgnoreCase("true")) {
                response = azureDevOpsEndpoint(AdoPayloads.updateField(testDescription), "PATCH", "cmd-sw/cmdea/_apis/wit/workitems/" + testCaseId + "?api-version=7.0", "application/json-patch+json");
            }
        }

        public void updateAdoResult(String outcome) {
            if (System.getProperty("update_ado").equalsIgnoreCase("true")) {
                findTestSuiteAndPointId();

                response = azureDevOpsEndpoint(AdoPayloads.patchTestOutcome(outcome), "PATCH", "cmd-sw/cmdea/_apis/test/Plans/" + System.getProperty("ado_test_plan") + "/Suites/" + testSuiteId + "/points/" + testPointId + "?api-version=6.0", "application/json");

                if (outcome.equalsIgnoreCase("Failed")) {
                    String lastTestRunId = response.jsonPath().getString("value[0].lastTestRun.id");
                    response = azureDevOpsEndpoint(AdoPayloads.patchTestErrorMessage(errorMessage), "PATCH", "cmd-sw/cmdea/_apis/test/Runs/" + lastTestRunId + "/results?api-version=7.1-preview.6", "application/json");
                }
            }
        }

        public void findTestSuiteAndPointId() {
            response = azureDevOpsEndpoint(null, "GET", "cmd-sw/_apis/testplan/suites?testCaseId=" + testCaseId + "&api-version=7.2-preview.1", "application/json");
            JSONObject jsonObject = new JSONObject(response.getBody().asString());
            JSONArray valueArray = jsonObject.getJSONArray("value");

            for (int i = 0; i < valueArray.length(); i++) {
                JSONObject testSuite = valueArray.getJSONObject(i);
                if (testSuite.getJSONObject("plan").getInt("id") == Integer.parseInt(System.getProperty("ado_test_plan"))) {
                    testSuiteId = testSuite.getInt("id");
                    break;
                }
            }

            response = azureDevOpsEndpoint(null, "GET", "cmd-sw/cmdea/_apis/test/Plans/" + System.getProperty("ado_test_plan") + "/Suites/" + testSuiteId + "/points?api-version=6.0&testCaseId=" + testCaseId, "application/json");
            jsonObject = new JSONObject(response.getBody().asString());
            JSONArray pointsArray = jsonObject.getJSONArray("value");
            for (int i = 0; i < pointsArray.length(); i++) {
                JSONObject point = pointsArray.getJSONObject(i);
                if (point.getJSONObject("testCase").getInt("id") == testCaseId) {
                    testPointId = point.getInt("id");
                    break;
                }
            }
        }
    }
    */
