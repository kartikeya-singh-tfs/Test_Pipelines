package com.example.framework.PayloadBodies.ado;

import com.fasterxml.jackson.annotation.JsonInclude;
import lombok.Data;
import lombok.experimental.Accessors;

@Data
@Accessors(chain = true)
@JsonInclude(JsonInclude.Include.NON_DEFAULT)
public class ErrorMessage {
    private int id;
    private String errorMessage;
}
