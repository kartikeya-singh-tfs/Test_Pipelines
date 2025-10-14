package com.example.framework.PayloadBodies.ado;

import com.fasterxml.jackson.annotation.JsonInclude;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.experimental.Accessors;
import java.util.ArrayList;
import java.util.List;

import com.example.framework.PayloadBodies.ado.ErrorMessage;
import com.example.framework.PayloadBodies.ado.UpdateField;

@Data
@NoArgsConstructor
@Accessors(chain  = true)
@JsonInclude(JsonInclude.Include.NON_DEFAULT)
public class AdoPayloads {
    private String outcome;
    private String suiteType;
    private String name;
    private List<ErrorMessage> errorMessage;
    private List<UpdateField> updateField;

    public static AdoPayloads patchTestOutcome(String outcome) {
        return new AdoPayloads()
                .setOutcome(outcome);
    }

    public static List<ErrorMessage> patchTestErrorMessage(String errorMessageLog) {
        List<ErrorMessage> errorMessage = new ArrayList<>();

        ErrorMessage payload = new ErrorMessage();
        payload.setId(100000);
        payload.setErrorMessage(errorMessageLog);

        errorMessage.add(payload);

        return errorMessage;
    }

    public static List<UpdateField> updateField(String value) {
        List<UpdateField> updateField = new ArrayList<>();

        UpdateField payload = new UpdateField();
        payload.setOp("replace");
        payload.setPath("/fields/Microsoft.VSTS.TCM.Steps");
        payload.setValue("<steps id=\"0\" last=\"2\"><step id=\"2\" type=\"ActionStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;" + value + "&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;&lt;BR/&gt;&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><description/></step></steps>");
        updateField.add(payload);

        UpdateField statusPayload = new UpdateField();
        statusPayload.setOp("replace");
        statusPayload.setPath("/fields/Custom.ThermoAutomationStatus");
        statusPayload.setValue("Automated");
        updateField.add(statusPayload);

        UpdateField areaPathPayload = new UpdateField();
        areaPathPayload.setOp("replace");
        areaPathPayload.setPath("/fields/System.AreaPath");
        areaPathPayload.setValue("cmdea\\Team Skynet");
        updateField.add(areaPathPayload);

        UpdateField statePayload = new UpdateField();
        statePayload.setOp("replace");
        statePayload.setPath("/fields/System.State");
        statePayload.setValue("Ready");
        updateField.add(statePayload);

        return updateField;
    }

    public static AdoPayloads postTestSuite(String name) {
        return new AdoPayloads()
                .setSuiteType("StaticTestSuite")
                .setName(name);
    }
}
