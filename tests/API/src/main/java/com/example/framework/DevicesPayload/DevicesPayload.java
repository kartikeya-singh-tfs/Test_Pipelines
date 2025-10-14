package com.example.framework.DevicesPayload;

import java.util.List;

import com.example.framework.DevicesPayload.AdditionalProp1;
import com.example.framework.DevicesPayload.Versions;
import com.example.framework.DevicesPayload.WebhookRegistrations;
import com.example.framework.TestData.TestData_DevicePayload;
import com.fasterxml.jackson.annotation.JsonInclude;
import lombok.Data;
import lombok.experimental.Accessors;

@JsonInclude(JsonInclude.Include.NON_EMPTY)
@Data
@Accessors(chain = true)
public class DevicesPayload {

    public Object deviceKey= null;

    public Object serialNumber= null;
    public Object deviceType = null;

    public Object deviceClass = null;

    public Object discoveryUri = null;

    public Object manufacturer = null;

    public Object family = null;

    public Object model = null;

    public Object modelDisplayName = null;

    public Object ddiOriginId = null;

    public Object computerName = null;

    public Object registrationDateTime = null;

    public Object lastUpdateDateTime = null;

    public Object persistentId = null;

    public Object isHealthy = null;

    public Object lastHealthChangeDateTime = null;

    public Object userEnteredSerialNumber = null;

    public Object externalDiscoveryUri = null;

    public Object ipcDiscoveryUri = null;

    public Object retired = null;

    @JsonInclude(JsonInclude.Include.NON_EMPTY)
    public Versions versions = null;

    @JsonInclude(JsonInclude.Include.NON_EMPTY)
    public WebhookRegistrations webhookRegistrations = null;

    public List<Object> compatibilityIds = null;

    public static DevicesPayload RegisterNewDevice(TestData_DevicePayload DataObject) {
        Versions versions1 = null;
        if (DataObject.getFirmwareVersion() != null || DataObject.getDriverVersion() != null) {
            versions1 = new Versions()
                .setFirmwareVersion(DataObject.getFirmwareVersion())
                .setDriverVersion(DataObject.getDriverVersion());
        }

        AdditionalProp1 additionalProp1 = null;
        if (DataObject.getSecret1() != null || DataObject.getCallBack1() != null) {
            additionalProp1 = new AdditionalProp1()
                .setSecret(DataObject.getSecret1())
                .setCallback(DataObject.getCallBack1());
        }

        WebhookRegistrations webhookRegistrations1 = null;
        if (additionalProp1 != null) {
            webhookRegistrations1 = new WebhookRegistrations()
                .setAdditionalProp1(additionalProp1);
        }

        return new DevicesPayload()
                .setDeviceKey(DataObject.getDeviceKey())
                .setSerialNumber(DataObject.getSerialNumber())
                .setDeviceType(DataObject.getDeviceType())
                .setDeviceClass(DataObject.getDeviceClass())
                .setDiscoveryUri(DataObject.getDiscoveryUri())
                .setManufacturer(DataObject.getManufacturer())
                .setFamily(DataObject.getFamily())
                .setModel(DataObject.getModel())
                .setModelDisplayName(DataObject.getModelDisplayName())
                .setDdiOriginId(DataObject.getDdiOriginId())
                .setComputerName(DataObject.getComputerName())
                .setVersions(versions1)
                .setWebhookRegistrations(webhookRegistrations1);
    }

}

