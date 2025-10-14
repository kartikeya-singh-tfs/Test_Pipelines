package com.example.framework.DevicesPayload;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.annotation.JsonPropertyOrder;
import lombok.Data;
import lombok.experimental.Accessors;

@JsonInclude(JsonInclude.Include.NON_EMPTY)
@Data
@Accessors(chain = true)
public class Versions {

    public Object driverVersion = null;
    public Object firmwareVersion = null;

    public boolean isEmpty() {
        return (driverVersion == null || driverVersion.toString().isEmpty()) &&
               (firmwareVersion == null || firmwareVersion.toString().isEmpty());
    }

}
