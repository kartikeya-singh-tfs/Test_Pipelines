package com.example.framework.ui;

import org.openqa.selenium.*;
import org.openqa.selenium.support.PageFactory;

public class LoginPage {
    private WebDriver driver;
    private JavascriptExecutor jsExecutor;

    public LoginPage(WebDriver driver) {
       // this.driver = Utils.createHeadlessChromeDriver();
        this.driver = driver;
        this.jsExecutor = (JavascriptExecutor) driver;
        PageFactory.initElements(driver, this);
    }

    public void login(String username, String password) throws InterruptedException {
        driver.get("https://ardia-instrument.cmdtest.thermofisher.com/?page=home");
        Thread.sleep(15000);

        WebElement tfLoginElement = driver.findElement(By.cssSelector("tf-login#applogin"));
        SearchContext shadowRoot1 = (SearchContext) jsExecutor.executeScript("return arguments[0].shadowRoot", tfLoginElement);

        WebElement usernameField = shadowRoot1.findElement(By.cssSelector("input#username"));
        WebElement continueButton = shadowRoot1.findElement(By.id("signin"));

        usernameField.click();
        usernameField.sendKeys(username);
        Thread.sleep(5000);
        continueButton.click();
        Thread.sleep(10000);

        WebElement passwordPageElement = driver.findElement(By.cssSelector("tf-cm-login#applogin"));
        SearchContext shadowRootPassword = (SearchContext) jsExecutor.executeScript("return arguments[0].shadowRoot", passwordPageElement);

        WebElement passwordField = shadowRootPassword.findElement(By.id("password"));
        WebElement passwordContinueButton = shadowRootPassword.findElement(By.id("signin"));

        passwordField.click();
        passwordField.sendKeys(password);
        passwordField.click();
        Thread.sleep(5000);
        passwordContinueButton.click();
       // Thread.sleep(5000);
       // driver.navigate().refresh();
        Thread.sleep(10000);


        driver.navigate().to("https://ardia-instrument.cmdtest.thermofisher.com/app/workstreams/#/home");
        Thread.sleep(5000);

    }
}
