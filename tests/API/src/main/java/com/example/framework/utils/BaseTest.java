package com.example.framework.utils;

import com.example.framework.Variables.Global;
import org.testng.annotations.BeforeSuite;
import com.example.framework.utils.ConfigUtils;

//Testing the base test

public class BaseTest {

   // @BeforeSuite
    public void setupSuite() throws InterruptedException {
        checkAndRefreshToken();
        Global.baseUri = ConfigUtils.BASE_URI;
    }

    private void checkAndRefreshToken() throws InterruptedException {
        if (Global.bearerToken == null || Global.bearerToken.isEmpty() || isTokenExpired()) {
            CommonFunctions.getBearerToken();
        }
    }

    private boolean isTokenExpired() {
        // Implement your logic to check if the token is expired
        //   For example, you can decode the token and check the expiration time
        return false; // Replace with actual expiration check
    }
}
