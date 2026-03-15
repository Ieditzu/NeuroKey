package io.github.kawase.utility;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import org.json.JSONObject;
import org.json.JSONArray;

public class GeminiAI {
    private final String apiKey = System.getenv("GEMINI_API_KEY");
    private static final long CACHE_TTL_MS = 5 * 60 * 1000;
    private static final int CACHE_MAX = 200;
    private static final long COOLDOWN_MS = 60 * 1000;
    private static volatile long cooldownUntilMs = 0L;

    private static final java.util.Map<String, CachedResponse> RESPONSE_CACHE =
            java.util.Collections.synchronizedMap(new java.util.LinkedHashMap<>(CACHE_MAX, 0.75f, true) {
                @Override
                protected boolean removeEldestEntry(java.util.Map.Entry<String, CachedResponse> eldest) {
                    return size() > CACHE_MAX;
                }
            });

    private static final class CachedResponse {
        private final String text;
        private final long createdAtMs;

        private CachedResponse(String text, long createdAtMs) {
            this.text = text;
            this.createdAtMs = createdAtMs;
        }
    }

    private String getCached(String prompt) {
        if (prompt == null || prompt.isBlank()) {
            return null;
        }
        CachedResponse cached = RESPONSE_CACHE.get(prompt);
        if (cached == null) {
            return null;
        }
        if (System.currentTimeMillis() - cached.createdAtMs > CACHE_TTL_MS) {
            RESPONSE_CACHE.remove(prompt);
            return null;
        }
        return cached.text;
    }

    private void putCached(String prompt, String response) {
        if (prompt == null || prompt.isBlank() || response == null || response.isBlank()) {
            return;
        }
        RESPONSE_CACHE.put(prompt, new CachedResponse(response, System.currentTimeMillis()));
    }

    public String ask(String question, String context) {
        return ask(question, context, "");
    }

    public String ask(String question, String context, String profileSummary) {
        try {
            if (apiKey == null || apiKey.isBlank()) {
                return "AI Error: GEMINI_API_KEY is not set on the server.";
            }
            long now = System.currentTimeMillis();
            if (now < cooldownUntilMs) {
                System.out.println("[AI] Cooldown active, skipping ask.");
                return "AI Error: Rate limit reached. Please try again in a moment.";
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
            String cached = getCached(prompt);
            if (cached != null) {
                System.out.println("[AI] Cache hit (ask).");
                return cached;
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
                String text = jsonResponse.getJSONArray("candidates")
                        .getJSONObject(0)
                        .getJSONObject("content")
                        .getJSONArray("parts")
                        .getJSONObject(0)
                        .getString("text");
                System.out.println("[AI] Ask success.");
                putCached(prompt, text);
                return text;
            } else if (response.statusCode() == 429) {
                cooldownUntilMs = System.currentTimeMillis() + COOLDOWN_MS;
                System.out.println("[AI] Ask rate limited (429). Body: " + response.body());
                return "AI Error: Server returned status 429 - " + response.body();
            } else {
                System.out.println("[AI] Ask error " + response.statusCode() + ". Body: " + response.body());
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
            long now = System.currentTimeMillis();
            if (now < cooldownUntilMs) {
                System.out.println("[AI] Cooldown active, skipping generate.");
                return "AI Error: Rate limit reached. Please try again in a moment.";
            }

            String cached = getCached(prompt);
            if (cached != null) {
                System.out.println("[AI] Cache hit (generate).");
                return cached;
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
                String text = jsonResponse.getJSONArray("candidates")
                        .getJSONObject(0)
                        .getJSONObject("content")
                        .getJSONArray("parts")
                        .getJSONObject(0)
                        .getString("text");
                System.out.println("[AI] Generate success.");
                putCached(prompt, text);
                return text;
            } else if (response.statusCode() == 429) {
                cooldownUntilMs = System.currentTimeMillis() + COOLDOWN_MS;
                System.out.println("[AI] Generate rate limited (429). Body: " + response.body());
                return "AI Error: Server returned status 429 - " + response.body();
            } else {
                System.out.println("[AI] Generate error " + response.statusCode() + ". Body: " + response.body());
                return "AI Error: Server returned status " + response.statusCode() + " - " + response.body();
            }

        } catch (Exception e) {
            e.printStackTrace();
            return "AI Error: " + e.getMessage();
        }
    }
}
