package com.example.framework.DevicesPayload;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;
import lombok.Data;
import lombok.experimental.Accessors;

@JsonInclude(JsonInclude.Include.NON_EMPTY)
@Data
@Accessors(chain = true)
public class WebhookRegistrations {

    public Object additionalProp1 = null;
    public Object additionalProp2 = null;
    public Object additionalProp3 = null;

    public boolean isEmpty() {
        return (additionalProp1 == null || additionalProp1.toString().isEmpty()) &&
               (additionalProp2 == null || additionalProp2.toString().isEmpty()) &&
               (additionalProp3 == null || additionalProp3.toString().isEmpty());
    }

}
