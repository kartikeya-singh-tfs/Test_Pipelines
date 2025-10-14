////This structure uses JsonInclude to skip the null fields in the request body where jackson is used to serialize the payloads
//
//package com.example.framework.PayloadBodies.Simple_old;
//
//import com.example.framework.TestData.TestData_DevicePayload;
//import lombok.Data;
//import lombok.experimental.Accessors;
//import com.fasterxml.jackson.annotation.JsonInclude;
//
//@JsonInclude(JsonInclude.Include.NON_NULL)
//@Data
//@Accessors(chain = true)
//public class DevicesPayload
//{
//    private Object deviceKey = null;
//    private Object serialNumber = null;
//    private Object deviceType = null;
//    private Object deviceClass = null;
//    private Object discoveryUri = null;
//    private Object manufacturer = null;
//    private Object family = null;
//    private Object model = null;
//    private Object modelDisplayName = null;
//    private Object ddiOriginId = null;
//    private Object computerName = null;
//    private Object driverVersion = null;
//
//
//
//    public static DevicesPayload RegisterNewDevice(TestData_DevicePayload DataObject) {
//        return new DevicesPayload()
//                .setDeviceKey(DataObject.getDeviceKey())
//                .setSerialNumber(DataObject.getSerialNumber())
//                .setDeviceType(DataObject.getDeviceType())
//                .setDeviceClass(DataObject.getDeviceClass())
//                .setDiscoveryUri(DataObject.getDiscoveryUri())
//                .setManufacturer(DataObject.getManufacturer())
//                .setFamily(DataObject.getFamily())
//                .setModel(DataObject.getModel())
//                .setModelDisplayName(DataObject.getModelDisplayName())
//                .setDdiOriginId(DataObject.getDdiOriginId())
//                .setComputerName(DataObject.getComputerName())
//
//
//    }
//
//}
