package com.example.framework.PayloadBodies.ado;

import com.fasterxml.jackson.annotation.JsonInclude;
import lombok.Data;
import lombok.experimental.Accessors;

@Data
@Accessors(chain = true)
@JsonInclude(JsonInclude.Include.NON_DEFAULT)
public class UpdateField {
    private String op;
    private String path;
    private String value;
}
