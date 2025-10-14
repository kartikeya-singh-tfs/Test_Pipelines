package com.example.framework;

import io.qameta.allure.restassured.AllureRestAssured;
import io.restassured.RestAssured;
import io.restassured.builder.RequestSpecBuilder;
import io.restassured.builder.ResponseSpecBuilder;
import io.restassured.http.ContentType;
import io.restassured.response.Response;
import io.restassured.specification.RequestSpecification;

import static com.example.framework.Variables.Global.*;
import static com.example.framework.utils.ConfigUtils.BASE_URI;
import org.testng.annotations.BeforeClass;
import org.testng.annotations.Test;

import static io.restassured.RestAssured.get;
import static io.restassured.RestAssured.given;
import static com.example.framework.utils.ConfigUtils.BASE_URI;

public class RequestFactory {

    //static String baseUri = BASE_URI;
    static String baseUri=  "ardia-connect-int.cmddev.thermofisher.com";
    RequestSpecification requestSpecification;
    public  RequestFactory()
    {

        RequestSpecBuilder requestSpecificationBuilder = new RequestSpecBuilder();
        requestSpecificationBuilder.setBaseUri("https://api." + baseUri);
        requestSpecificationBuilder.addFilter(new AllureRestAssured());
        requestSpecificationBuilder.setContentType(ContentType.JSON);
        requestSpecificationBuilder.addHeader("Authorization", "Bearer " + bearerToken);


        //Add headers if needed
        //check the length of hashmap headers
        if (headers != null && !headers.isEmpty())
        {
            requestSpecificationBuilder.addHeaders(headers);
        }
        else
        {
            System.out.println("No additional headers found, proceeding without additional headers.");
        }



        requestSpecification=  requestSpecificationBuilder.build();
        clearHeaders();

        //Set response specification
       ResponseSpecBuilder responseSpecBuilder = new ResponseSpecBuilder();
      //responseSpecBuilder.expectStatusCode(201);
       responseSpecBuilder.expectContentType(ContentType.JSON);
       RestAssured.responseSpecification = responseSpecBuilder.build();

    }


    public static Response getRequest(String endpoint) {
        return get(endpoint);
    }

    @Test
    public  Response postRequest(String endpoint, Object body) {


       return  given().spec(requestSpecification)
               .body(body)
               .when()
               .post(endpoint);

    }

    public  Response putRequest(String endpoint, String body) {
        return   given().spec(requestSpecification)
                .body(body)
                .when()
                .put(endpoint);
    }

}
