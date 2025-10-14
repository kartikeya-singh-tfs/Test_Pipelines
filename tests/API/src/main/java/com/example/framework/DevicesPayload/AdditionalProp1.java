
package com.example.framework.DevicesPayload;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;
import lombok.Data;
import lombok.experimental.Accessors;

@JsonInclude(JsonInclude.Include.NON_EMPTY)
@Data
@Accessors(chain = true)
public class AdditionalProp1 {

    public Object secret = null;
    public Object callback = null;

}
