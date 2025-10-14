
package com.example.framework.utils;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ObjectNode;
import org.json.JSONArray;
import org.json.JSONObject;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import org.json.JSONArray;
import org.json.JSONObject;
import org.testng.Assert;

public class JsonUtils {

    private static final ObjectMapper objectMapper = new ObjectMapper();

    public static <T> T fromJson(String json, Class<T> clazz) throws IOException {
        return objectMapper.readValue(json, clazz);
    }

    public static String toJson(Object obj) throws IOException {
        return objectMapper.writeValueAsString(obj);
    }

    public static String readJsonFromFile(String filePath) throws IOException {
        return new String(Files.readAllBytes(Paths.get(filePath)));
    }

    public static String getUpdatedRequestBody(String template, Map<String, String> replacements) {
        for (Map.Entry<String, String> entry : replacements.entrySet()) {
            template = template.replace("{{" + entry.getKey() + "}}", entry.getValue());
        }
        return template;
    }

    public static JsonNode updateJsonValue(JsonNode rootNode, String key, String newValue) {
        JsonNode targetNode = findNode(rootNode, key);
        if (targetNode != null && targetNode.isTextual()) {
            JsonNode parentNode = findParentNode(rootNode, targetNode);
            if (parentNode != null && parentNode.isObject()) {
                ((ObjectNode) parentNode).put(key, newValue);
            }
        }
        return rootNode;
    }

    private static JsonNode findNode(JsonNode rootNode, String fieldName) {
        if (rootNode.has(fieldName)) {
            return rootNode.get(fieldName);
        }
        for (JsonNode childNode : rootNode) {
            JsonNode result = findNode(childNode, fieldName);
            if (result != null) {
                return result;
            }
        }
        return null;
    }

    private static JsonNode findParentNode(JsonNode rootNode, JsonNode targetNode) {
        if (rootNode.isObject()) {
            ObjectNode objectNode = (ObjectNode) rootNode;
            for (JsonNode childNode : objectNode) {
                if (childNode.equals(targetNode)) {
                    return rootNode;
                }
                JsonNode result = findParentNode(childNode, targetNode);
                if (result != null) {
                    return result;
                }
            }
        } else if (rootNode.isArray()) {
            for (JsonNode childNode : rootNode) {
                JsonNode result = findParentNode(childNode, targetNode);
                if (result != null) {
                    return result;
                }
            }
        }
        return null;
    }

    /**
     * Function Name: validateResponseParameterValue
     * Created By: FunctionDescriptionGenerator
     *
     * Description:
     * This function validates whether a specified parameter within a JSON response
     * matches an expected value. The parameter is identified using a dot notated path,
     * which can include array indices.
     *
     * Arg : 3
     * Arg [1][String] : jsonResponse - The JSON response as a string.
     * Arg [2][String] : parameterPath - The dot notated path to the parameter within the JSON.
     * Arg [3][String] : expectedValue - The value expected at the specified parameter path.
     *
     * Returns : boolean - Returns true if the parameter's value matches the expected value, false otherwise.
     */



    public static boolean validateResponseParameterValue(String jsonResponse, String parameterPath, String expectedValue) {
        // Convert the string to a JSONObject
        Object currentObject= getJsonParameterValue(jsonResponse, parameterPath);

        // Compare the final value with the expected value
        if (currentObject != null) {
            Assert.assertEquals(currentObject.toString(), expectedValue);
            System.out.println("Value in Response: "+ currentObject.toString());
            return true;
        } else {
            System.err.println("Current object is null");
            Assert.fail("Parameter not found in the provided Json. Test case failed.");
            return false;
        }
    }

    public static boolean validateSubstringExistsInResponseParameterValue(String jsonResponse, String parameterPath, String expectedValue) {
        // Convert the string to a JSONObject
        Object currentObject= getJsonParameterValue(jsonResponse, parameterPath);

        // Compare the final value with the expected value
        if (currentObject != null) {
            Assert.assertTrue((currentObject.toString()).contains(expectedValue));
            System.out.println("Value in Response: "+currentObject.toString());
            return true;
        } else {
            System.err.println("Current object is null");
            Assert.fail("Parameter not found in the provided Json. Test case failed.");
            return false;
        }
    }

    public static Object getJsonParameterValue(String jsonData, String parameterPath) {
        // Convert the string to a JSONObject
        JSONObject jsonObject = new JSONObject(jsonData);

        Object currentObject = jsonObject;
        String[] pathElements = parameterPath.split("\\.");

        for (String element : pathElements) {
            if (currentObject instanceof JSONObject) {
                JSONObject currentJsonObject = (JSONObject) currentObject;
                if (element.contains("[")) {
                    // Handle array access
                    String key = element.substring(0, element.indexOf("["));
                    int index = Integer.parseInt(element.substring(element.indexOf("[") + 1, element.indexOf("]")));
                    if (currentJsonObject.has(key)) {
                        JSONArray jsonArray = currentJsonObject.getJSONArray(key);
                        currentObject = jsonArray.get(index);
                    } else {
                        System.err.println("Key not found: " + key);
                        return null;
                    }
                } else {
                    // Handle object access
                    if (currentJsonObject.has(element)) {
                        currentObject = currentJsonObject.get(element);
                    } else {
                        System.err.println("Element not found: " + element);
                        return null;
                    }
                }
            } else if (currentObject instanceof JSONArray) {
                // Handle nested arrays
                JSONArray currentJsonArray = (JSONArray) currentObject;
                int index = Integer.parseInt(element);
                if (index < currentJsonArray.length()) {
                    currentObject = currentJsonArray.get(index);
                } else {
                    System.err.println("Index out of bounds: " + index);
                    return null;
                }
            } else {
                System.err.println("Current object is not a JSONObject or JSONArray");
                return null;
            }
        }

        return currentObject;
    }
}

