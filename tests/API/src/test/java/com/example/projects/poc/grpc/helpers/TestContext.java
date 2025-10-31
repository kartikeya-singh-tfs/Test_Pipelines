package com.example.projects.poc.grpc.helpers;

/**
 * Small test context helper used to tag calls with a test id.
 * Tests do not need to call this directly; the listener will set/clear it.
 */
public final class TestContext {
    private static final ThreadLocal<String> CURRENT = new ThreadLocal<>();

    private TestContext() {}

    public static void setCurrentTestId(String id) { CURRENT.set(id); }
    public static String getCurrentTestId() { return CURRENT.get(); }
    public static void clear() { CURRENT.remove(); }
}
