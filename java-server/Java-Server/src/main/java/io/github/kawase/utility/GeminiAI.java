package io.github.kawase.utility;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import org.json.JSONObject;
import org.json.JSONArray;

public class GeminiAI {
    private final String apiKey = System.getenv("GEMINI_API_KEY");

    public String ask(String question, String context) {
        return ask(question, context, "");
    }

    public String ask(String question, String context, String profileSummary) {
        try {
            if (apiKey == null || apiKey.isBlank()) {
                return "AI Error: GEMINI_API_KEY is not set on the server.";
            }
            StringBuilder promptBuilder = new StringBuilder();
            promptBuilder.append("You are an educational AI mentor for a student learning C++ in a game called NeuroKey. ");
            if (profileSummary != null && !profileSummary.isBlank()) {
                promptBuilder.append("Student profile: ").append(profileSummary).append("\n");
            }
            promptBuilder.append("Context: ").append(context).append("\n")
                    .append("Student's Question: ").append(question).append("\n")
                    .append("Please provide a helpful, encouraging, and easy-to-understand response for a student. ")
                    .append("Keep it concise (1-3 sentences).");

            final String prompt = promptBuilder.toString();

            JSONObject requestBody = new JSONObject();
            JSONArray contents = new JSONArray();
            JSONObject part = new JSONObject();
            part.put("text", prompt);
            JSONArray parts = new JSONArray();
            parts.put(part);
            JSONObject content = new JSONObject();
            content.put("parts", parts);
            contents.put(content);
            requestBody.put("contents", contents);

            HttpClient client = HttpClient.newHttpClient();

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create("https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + apiKey))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(requestBody.toString()))
                    .build();

            HttpResponse<String> response = client.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() == 200) {
                JSONObject jsonResponse = new JSONObject(response.body());
                return jsonResponse.getJSONArray("candidates")
                        .getJSONObject(0)
                        .getJSONObject("content")
                        .getJSONArray("parts")
                        .getJSONObject(0)
                        .getString("text");
            } else {
                return "AI Error: Server returned status " + response.statusCode() + " - " + response.body();
            }

        } catch (Exception e) {
            e.printStackTrace();
            return "AI Error: " + e.getMessage();
        }
    }

    public String generate(String prompt) {
        try {
            if (apiKey == null || apiKey.isBlank()) {
                return "AI Error: GEMINI_API_KEY is not set on the server.";
            }

            JSONObject requestBody = new JSONObject();
            JSONArray contents = new JSONArray();
            JSONObject part = new JSONObject();
            part.put("text", prompt);
            JSONArray parts = new JSONArray();
            parts.put(part);
            JSONObject content = new JSONObject();
            content.put("parts", parts);
            contents.put(content);
            requestBody.put("contents", contents);

            HttpClient client = HttpClient.newHttpClient();

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create("https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + apiKey))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(requestBody.toString()))
                    .build();

            HttpResponse<String> response = client.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() == 200) {
                JSONObject jsonResponse = new JSONObject(response.body());
                return jsonResponse.getJSONArray("candidates")
                        .getJSONObject(0)
                        .getJSONObject("content")
                        .getJSONArray("parts")
                        .getJSONObject(0)
                        .getString("text");
            } else {
                return "AI Error: Server returned status " + response.statusCode() + " - " + response.body();
            }

        } catch (Exception e) {
            e.printStackTrace();
            return "AI Error: " + e.getMessage();
        }
    }
}
