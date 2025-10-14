// File: Melbourne/src/test/java/utils/ConfigUtils.java
package com.example.framework.utils;

import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;

public class ConfigUtils {
    private static final Properties properties = new Properties();

    public static final String BASE_URI = getProperty("base.uri");

    private static final String CONFIG_FILE = "config.properties";
    static {
        try (InputStream input = ConfigUtils.class.getClassLoader().getResourceAsStream("config.properties")) {
            if (input == null) {
                throw new RuntimeException("Sorry, unable to find config.properties");
            }
            properties.load(input);
        } catch (IOException ex) {
            ex.printStackTrace();
        }
    }

    public static String getProperty(String key) {
        return properties.getProperty(key);
    }

    public static void setProperty(String key, String value) {
        properties.setProperty(key, value);
        try (FileOutputStream output = new FileOutputStream(ConfigUtils.class.getClassLoader().getResource(CONFIG_FILE).getFile())) {
            properties.store(output, null);
        } catch (IOException ex) {
            ex.printStackTrace();
        }
    }
}