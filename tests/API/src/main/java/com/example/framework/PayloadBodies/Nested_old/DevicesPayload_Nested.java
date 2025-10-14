//This payload class is for the nested payloads structure similar to ardiacore framework where the nested json is kept
//in seperate class and is used here to initialize nested payloads

package com.example.framework.PayloadBodies.Nested_old;
import com.example.framework.TestData.TestData_DevicePayload;
import lombok.Data;
import lombok.experimental.Accessors;
import com.fasterxml.jackson.annotation.JsonInclude;

@JsonInclude(JsonInclude.Include.NON_NULL)
@Data
@Accessors(chain = true)
public class DevicesPayload_Nested {

    private Object deviceKey = null;
    private Object serialNumber = null;
    private Object deviceType = null;
    private Object deviceClass = null;
    private Object discoveryUri = null;
    private Object manufacturer = null;
    private Object family = null;
    private Object model = null;
    private Object modelDisplayName = null;
    private Object ddiOriginId = null;
    private Object computerName = null;
    private Object driverVersion = null;

    @Data
    public class DriverVersionPayload{
        private  String DriverVersion = null;
        private  String FirmwareVersion = null;
    }


    public static DevicesPayload_Nested RegisterNewDevice(TestData_DevicePayload DataObject) {
        DriverVersionPayload driverVersionPayload = new DevicesPayload_Nested().new DriverVersionPayload()
                        .setDriverVersion(DataObject.getDriverVersion())
                        .setFirmwareVersion(DataObject.getFirmwareVersion());


        return new DevicesPayload_Nested()
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
                .setDriverVersion(driverVersionPayload);


    }
}
