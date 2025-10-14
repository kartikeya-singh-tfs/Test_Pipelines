package com.example.framework.EndPoints.Connectivity;

import static com.example.framework.utils.ConfigUtils.BASE_URI;

public class Connectivity_IDS_Endpoints {

    //Devices Endpoints--------------------------
    //The controller for Device related endpoints
    public static String postOrPutRegisterDevice() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/device";
    }

    public static String postRegisterDevices() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/devices";
    }

    public static String putDeviceSerialNumberByKey(String deviceKey,String serialNumber) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/device/" + deviceKey+"/user-entered-serial-number?serialNumber="+serialNumber;
    }

    public static String getOrDeleteDeviceByKey(String deviceKey) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/device/" + deviceKey;
    }

    public static String postMatchingDevices() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/matching-devices";
    }

    //DeviceServices ----------------------------------
    //The controller for DeviceService related endpoints
    public static String postOrPutRegisterDeviceService() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/device-service";
    }

    public static String postRegisterDeviceServices() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/device-services";
    }

    public static String getOrDeleteDeviceServiceById(String deviceServiceId) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/device-service/" + deviceServiceId;
    }

    public static String postMatchingDeviceServices() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/matching-device-services";
    }

    public static String postCompatibleDeviceServices(String deviceServiceType) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/compatible-device-services?type="+deviceServiceType;
    }

    //Instruments Endpoints--------------------------
    //The controller for Instrument related endpoints
    public static String postCreateInstrument() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument";
    }

    public static String postCreateInstrumentWithSkipValidation(Boolean boolSkipDevValidation) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument?skipDeviceValidation="+boolSkipDevValidation;
    }

    public static String getPutDeleteInstrumentById(String instrumentId) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument/" + instrumentId;
    }

    public static String putSyncInstrument() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/sync-instrument";
    }

    public static String putSyncInstrumentWithSkipValidation(Boolean boolSkipDevValidation) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/sync-instrument?skipDeviceValidation="+boolSkipDevValidation;
    }

    public static String putInstrumentLockedState(String instrumentId, Boolean lockState) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument/"+instrumentId+"/locked-state?locked="+lockState;
    }

    public static String putInstrumentRetiredState(String instrumentId, Boolean retiredState) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument/"+instrumentId+"/retired-state?retired="+retiredState;
    }

    public static String putInstrumentAcquisitionEnabledState(String instrumentId, Boolean acquisitionEnabledState) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument/"+instrumentId+"/acquisition-enabled-state?enabled="+acquisitionEnabledState;
    }

    public static String postMatchingInstruments(String after, String before) {
        if (after != null && !after.isEmpty() && before != null && !before.isEmpty()) {
            return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/matching-instruments?after=" + after + "&before=" + before;
        } else if (after != null && !after.isEmpty()) {
            return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/matching-instruments?after=" + after;
        } else if (before != null && !before.isEmpty()) {
            return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/matching-instruments?before=" + before;
        } else {
            return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/matching-instruments";
        }
    }

    public static String getInstrumentVersion(String instrumentId) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument/"+instrumentId+"/versions";
    }

    public static String getUnconfiguredDevices() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/unconfigured-devices";
    }

    public static String putSetUserDefinedData(String instrumentId) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument/"+instrumentId+"/user-defined-data";
    }

    //InstrumentServices Endpoints--------------------------
    //The controller for InstrumentService related endpoints
    public static String postOrPutRegisterInstrumentService() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument-service";
    }

    public static String postRegisterInstrumentServices() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument-services";
    }

    public static String getOrDeleteInstrumentServiceById(String instrumentServiceId) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/instrument-service/" + instrumentServiceId;
    }

    public static String postMatchingInstrumentServices() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/matching-instrument-services";
    }

    //Webhooks Endpoints--------------------------
    //The controller for Webhook related endpoints
    public static String getWebhooks() {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/webhooks";
    }

    public static String postRegisterWebhook(String webhookRegistrationType) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/webhooks/"+webhookRegistrationType;
    }

    public static String deleteWebhookById(String registrationId) {
        return BASE_URI + "/instrument-discovery-service/instrument-discovery/v1/webhooks/" + registrationId;
    }
}