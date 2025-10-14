package com.example.framework.utils;
import com.example.framework.RequestFactory;
import com.example.framework.ui.LoginPage;
//import com.example.framework.ui.WebDriverManager;
import io.restassured.RestAssured;
import io.restassured.response.Response;
import org.json.JSONObject;
import org.openqa.selenium.Cookie;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;
import static com.example.framework.utils.ConfigUtils.BASE_URI;
import com.example.framework.Variables.Global;
import org.testng.annotations.BeforeSuite;

import java.text.SimpleDateFormat;
import java.util.*;

public class CommonFunctions {

    private static WebDriver driver;
    private static LoginPage loginPage;

    private static String accessToken;
    private static final String namespace = "ardia-connect-int.cmddev.thermofisher.com"; // Replace with actual namespace
    private static final String grantType = "client_credentials"; // Replace with actual grant type
    private static final String clientId = "6f557354-c43b-49db-969f-a242eaa2527d"; // Replace with actual client ID
    private static final String clientSecret = "6daba982-4299-4603-b3da-2ff63e974967"; // Replace with actual client secret
    private static final String scope = "InstrumentApi DeviceRegistrationApi"; // Replace with actual scope

    public static String generateTokensEndpoint()
    {    return "https://api.ardia-connect-int.cmddev.thermofisher.com" + "/session-management/bff/generatetokens";}

    public static void getBearerToken() throws InterruptedException {
       // driver = WebDriverManager.getDriver();
        driver = createHeadlessChromeDriver();
        loginPage = new LoginPage(driver);

        loginPage.login("pramod.desai@thermofisher.com", "Thermo@123");

        // Fetch cookies from the browser
        Set<Cookie> seleniumCookies = driver.manage().getCookies();
        StringBuilder cookiesStringBuilder = new StringBuilder();

        for (Cookie cookie : seleniumCookies) {
            cookiesStringBuilder.append(cookie.toString()).append("; ");
        }

        String cookieValue = cookiesStringBuilder.toString();
        System.out.println(cookieValue);

        Thread.sleep(5000);

        //RestAssured.baseURI = "https://api.ardia-instrument.cmdtest.thermofisher.com";

//        Response tokenResponse = RequestFactory.getRequestWithHeaders(generateTokensEndpoint(), cookieValue, "1");
//        System.out.println("Token Response: " + tokenResponse.asString());
//
//        int accessTokenIndex = tokenResponse.asString().indexOf(',');
//
//        String bearerToken = tokenResponse.asString().substring(16, accessTokenIndex - 1);
//        System.out.println(bearerToken);
//
//        // Save the bearer token globally
//        Global.bearerToken = bearerToken;
    }

    public static WebDriver createHeadlessChromeDriver() {
        ChromeOptions options = new ChromeOptions();
        options.addArguments("--headless");
        options.addArguments("--disable-gpu");
        options.addArguments("--window-size=1920,1080");
        return new ChromeDriver(options);
    }

    public static String generateRandomName() {
        String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder sb = new StringBuilder();
        Random random = new Random();
        int length = 5; // Length of the random name

        for (int i = 0; i < length; i++) {
            int index = random.nextInt(alphabet.length());
            sb.append(alphabet.charAt(index));
        }

        int numeric = random.nextInt(90) + 10; // Generate a 2-digit number
        sb.append(numeric);

        return sb.toString();
    }
    public static String getCurrentTimestamp() {
        SimpleDateFormat formatter = new SimpleDateFormat("HH-mm-ss.SSS");
        return formatter.format(new Date());
    }
    public static String generateRandomUUID() {
        return UUID.randomUUID().toString();
    }

    // Method to get the current timestamp in a custom format
    public static String getCurrentTimestamp(String format) {
        SimpleDateFormat formatter = new SimpleDateFormat(format);
        return formatter.format(new Date());
    }
    @BeforeSuite
    public void getAccessToken() {
        if (accessToken == null || isTokenExpired(accessToken)) {
            String idUrl = "https://identity." + namespace + "/connect/token";

            Response response = RestAssured.given()
                    .header("Accept", "application/json")
                    .header("Content-Type", "application/x-www-form-urlencoded")
                    .formParam("grant_type", grantType)
                    .formParam("client_id", clientId)
                    .formParam("client_secret", clientSecret)
                    .formParam("scope", scope)
                    .post(idUrl);

            if (response.getStatusCode() == 200) {
                JSONObject jsonResponse = new JSONObject(response.getBody().asString());
                accessToken = jsonResponse.getString("access_token");
                System.out.println("Generated new Access Token: " + accessToken);
            } else {
                throw new RuntimeException("Failed to fetch access token: " + response.getBody().asString());
            }
        }
        Global.bearerToken = accessToken;
    }

    private static boolean isTokenExpired(String token) {
        try {
            String[] parts = token.split("\\.");
            String payload = new String(Base64.getDecoder().decode(parts[1]));
            JSONObject jsonPayload = new JSONObject(payload);
            long exp = jsonPayload.getLong("exp");
            long currentTime = System.currentTimeMillis() / 1000;
            return exp < currentTime;
        } catch (Exception e) {
            return true; // Treat as expired if decoding fails
        }
    }

}