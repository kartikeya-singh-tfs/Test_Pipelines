package com.example.framework.Variables;


import java.util.HashMap;

public class Global {
    public static String bearerToken ;
    public static  String baseUri;
    public static HashMap<String, String> headers = new HashMap<>();
    public static void setHeader(String header)
    {
        String[] headerParts = header.split(":");
        if (headerParts.length == 2) {
            headers.put(headerParts[0].trim(), headerParts[1].trim());
        } else {
            System.out.println("Invalid header format: " + header);
        }

    }

    public static void clearHeaders()
    {
        headers.clear();
    }


}

