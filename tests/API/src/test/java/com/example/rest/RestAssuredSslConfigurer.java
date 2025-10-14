package com.example.rest;

import io.restassured.RestAssured;
import io.restassured.config.HttpClientConfig;
import io.restassured.config.RestAssuredConfig;
import org.apache.http.conn.ssl.NoopHostnameVerifier;
import org.apache.http.conn.ssl.SSLConnectionSocketFactory;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;

import javax.net.ssl.SSLContext;

public final class RestAssuredSslConfigurer {

    private RestAssuredSslConfigurer() {}

    public static void configure(SSLContext sslContext) {
        SSLConnectionSocketFactory sslsf = new SSLConnectionSocketFactory(
                sslContext,
                NoopHostnameVerifier.INSTANCE
        );

        CloseableHttpClient apacheClient = HttpClients.custom()
                .setSSLSocketFactory(sslsf)
                .build();

                RestAssured.config = RestAssuredConfig.config()
                                .httpClient(HttpClientConfig.httpClientConfig()
                                                .httpClientFactory(new HttpClientConfig.HttpClientFactory() {
                                                        @Override
                                                        public org.apache.http.client.HttpClient createHttpClient() {
                                                                return apacheClient;
                                                        }
                                                }));
    }
}
