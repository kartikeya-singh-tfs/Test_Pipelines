package com.example.framework.TestData;

import lombok.Data;
import lombok.experimental.Accessors;

@Accessors(chain = true)


@Data
public class TestData_DevicePayload {
    public Object setSecret;
    private String deviceKey;
    private String serialNumber;
    private String deviceType;
    private String deviceClass;
    private String discoveryUri;
    private String manufacturer;
    private String family;
    private String model;
    private String modelDisplayName;
    private String ddiOriginId;
    private String computerName;
    private String driverVersion;
    private String firmwareVersion;
    private String deviceToken;
    private String CallBack1;
    private String Secret1;
}


