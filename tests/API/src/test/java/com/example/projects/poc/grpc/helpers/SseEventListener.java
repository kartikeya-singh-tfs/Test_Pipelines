package com.example.projects.poc.grpc.helpers;

import okhttp3.Response;
import okhttp3.sse.EventSource;
import okhttp3.sse.EventSourceListener;

import java.util.List;
import java.util.concurrent.CountDownLatch;

/**
 * Small reusable SSE EventSourceListener for tests.
 * Collects raw event data strings into a synchronized list and counts down a latch
 * when expected event types are observed.
 */
public class SseEventListener extends EventSourceListener {
    private final List<String> events;
    private final CountDownLatch latch;

    public SseEventListener(List<String> events, CountDownLatch latch) {
        this.events = events;
        this.latch = latch;
    }

    @Override
    public void onEvent(EventSource eventSource, String id, String type, String data) {
        events.add(data);
        System.out.println("[SSE] " + data);
        if (data.contains("\"Type\":\"SequenceStatus\"")) latch.countDown();
        else if (data.contains("\"Type\":\"SampleStatus\"")) latch.countDown();
        else if (data.contains("\"Type\":\"DeviceEvent\"")) latch.countDown();
    }

    @Override
    public void onFailure(EventSource eventSource, Throwable t, Response response) {
        System.err.println("[SSE] Error: " + t);
    }
}
