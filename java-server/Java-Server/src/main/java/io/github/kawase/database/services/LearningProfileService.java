package io.github.kawase.database.services;

import io.github.kawase.database.entity.Child;
import io.github.kawase.database.repository.ChildRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

@Service
@RequiredArgsConstructor
public class LearningProfileService {

    private final ChildRepository childRepository;

    @Transactional
    public void recordLearningEvent(final Long childId, final String eventType, final String topic, final int correctness, final String details) {
        if (childId == null) {
            return;
        }

        Child child = childRepository.findById(childId)
                .orElseThrow(() -> new RuntimeException("Child not found"));

        Map<String, Object> gameStats = child.getGameStats();
        if (gameStats == null) {
            gameStats = new HashMap<>();
        }

        Map<String, Object> aiProfile = ensureMap(gameStats, "aiProfile");
        Map<String, Object> topics = ensureMap(aiProfile, "topics");

        String resolvedTopic = (topic == null || topic.isBlank()) ? "cpp_general" : topic.trim();
        Map<String, Object> topicStats = ensureMap(topics, resolvedTopic);

        int totalInteractions = getInt(aiProfile.get("totalInteractions")) + 1;
        aiProfile.put("totalInteractions", totalInteractions);

        if ("ai_chat".equals(eventType)) {
            aiProfile.put("chatTurns", getInt(aiProfile.get("chatTurns")) + 1);
        } else if ("ai_hint".equals(eventType) || "hint".equals(eventType)) {
            aiProfile.put("hintsUsed", getInt(aiProfile.get("hintsUsed")) + 1);
        }

        if (correctness == 1) {
            aiProfile.put("correctCount", getInt(aiProfile.get("correctCount")) + 1);
            topicStats.put("correct", getInt(topicStats.get("correct")) + 1);
        } else if (correctness == 0) {
            aiProfile.put("incorrectCount", getInt(aiProfile.get("incorrectCount")) + 1);
            topicStats.put("incorrect", getInt(topicStats.get("incorrect")) + 1);
        }

        topicStats.put("attempts", getInt(topicStats.get("attempts")) + 1);
        topicStats.put("lastResult", correctness == 1 ? "correct" : correctness == 0 ? "incorrect" : "unknown");
        topicStats.put("lastSeen", Instant.now().toString());
        topics.put(resolvedTopic, topicStats);
        aiProfile.put("topics", topics);
        aiProfile.put("lastUpdated", Instant.now().toString());

        List<Map<String, Object>> recentEvents = ensureList(aiProfile, "recentEvents");
        Map<String, Object> event = new LinkedHashMap<>();
        event.put("ts", Instant.now().toString());
        event.put("type", eventType == null ? "unknown" : eventType);
        event.put("topic", resolvedTopic);
        event.put("correctness", correctness == 1 ? "correct" : correctness == 0 ? "incorrect" : "unknown");
        event.put("detail", truncate(details, 180));
        recentEvents.add(event);
        if (recentEvents.size() > 10) {
            recentEvents.remove(0);
        }
        aiProfile.put("recentEvents", recentEvents);

        gameStats.put("aiProfile", aiProfile);
        child.setGameStats(gameStats);
        childRepository.save(child);
    }

    @Transactional
    public void recordAiInteraction(final Long childId, final String context, final String question) {
        if (childId == null) {
            return;
        }

        if (context != null && context.toLowerCase().contains("eval")) {
            return;
        }

        String eventType = (context != null && context.toLowerCase().contains("hint")) ? "ai_hint" : "ai_chat";
        String topic = deriveTopic(context, question);
        recordLearningEvent(childId, eventType, topic, -1, question);
    }

    @Transactional(readOnly = true)
    public String buildProfileSummary(final Long childId) {
        if (childId == null) {
            return "";
        }

        Child child = childRepository.findById(childId).orElse(null);
        if (child == null) {
            return "";
        }

        Map<String, Object> gameStats = child.getGameStats();
        if (gameStats == null || !gameStats.containsKey("aiProfile")) {
            return "No prior learning profile yet.";
        }

        Map<String, Object> aiProfile = safeMap(gameStats.get("aiProfile"));
        int correct = getInt(aiProfile.get("correctCount"));
        int incorrect = getInt(aiProfile.get("incorrectCount"));
        int total = correct + incorrect;
        double accuracy = total == 0 ? 0.0 : (double) correct / Math.max(1, total);

        String level;
        if (total < 4) {
            level = "beginner";
        } else if (accuracy >= 0.85 && total >= 8) {
            level = "advanced";
        } else if (accuracy >= 0.65) {
            level = "intermediate";
        } else {
            level = "beginner";
        }

        List<String> strengths = new ArrayList<>();
        List<String> needsHelp = new ArrayList<>();
        Map<String, Object> topics = safeMap(aiProfile.get("topics"));
        List<Map.Entry<String, Integer>> scored = new ArrayList<>();
        for (Map.Entry<String, Object> entry : topics.entrySet()) {
            Map<String, Object> topicStats = safeMap(entry.getValue());
            int topicCorrect = getInt(topicStats.get("correct"));
            int topicIncorrect = getInt(topicStats.get("incorrect"));
            int score = topicCorrect - topicIncorrect;
            scored.add(Map.entry(entry.getKey(), score));
        }

        scored.sort((a, b) -> Integer.compare(b.getValue(), a.getValue()));
        for (Map.Entry<String, Integer> entry : scored) {
            if (entry.getValue() <= 0) {
                continue;
            }
            strengths.add(entry.getKey());
            if (strengths.size() >= 3) {
                break;
            }
        }

        scored.sort((a, b) -> Integer.compare(a.getValue(), b.getValue()));
        for (Map.Entry<String, Integer> entry : scored) {
            if (entry.getValue() >= 0) {
                continue;
            }
            needsHelp.add(entry.getKey());
            if (needsHelp.size() >= 3) {
                break;
            }
        }

        StringBuilder summary = new StringBuilder();
        summary.append("Student level: ").append(level).append(". ");
        if (!strengths.isEmpty()) {
            summary.append("Strengths: ").append(String.join(", ", strengths)).append(". ");
        }
        if (!needsHelp.isEmpty()) {
            summary.append("Needs help with: ").append(String.join(", ", needsHelp)).append(". ");
        }
        summary.append("Accuracy: ").append(correct).append(" correct out of ").append(total).append(".");
        return summary.toString();
    }

    private String deriveTopic(final String context, final String question) {
        String text = (context == null ? "" : context) + " " + (question == null ? "" : question);
        String normalized = text.toLowerCase();

        if (normalized.contains("loop") || normalized.contains("for ") || normalized.contains("while")) {
            return "loops";
        }
        if (normalized.contains("array") || normalized.contains("vector")) {
            return "arrays";
        }
        if (normalized.contains("pointer") || normalized.contains("address")) {
            return "pointers";
        }
        if (normalized.contains("reference") || normalized.contains("&")) {
            return "references";
        }
        if (normalized.contains("function") || normalized.contains("return")) {
            return "functions";
        }
        if (normalized.contains("if") || normalized.contains("else") || normalized.contains("switch")) {
            return "conditionals";
        }
        if (normalized.contains("class") || normalized.contains("struct") || normalized.contains("object")) {
            return "oop";
        }
        if (context != null && !context.isBlank()) {
            return context;
        }
        return "cpp_general";
    }

    @SuppressWarnings("unchecked")
    private Map<String, Object> ensureMap(final Map<String, Object> parent, final String key) {
        Object value = parent.get(key);
        if (value instanceof Map) {
            return (Map<String, Object>) value;
        }
        Map<String, Object> created = new HashMap<>();
        parent.put(key, created);
        return created;
    }

    @SuppressWarnings("unchecked")
    private Map<String, Object> safeMap(final Object value) {
        if (value instanceof Map) {
            return (Map<String, Object>) value;
        }
        return Collections.emptyMap();
    }

    @SuppressWarnings("unchecked")
    private List<Map<String, Object>> ensureList(final Map<String, Object> parent, final String key) {
        Object value = parent.get(key);
        if (value instanceof List) {
            return (List<Map<String, Object>>) value;
        }
        List<Map<String, Object>> created = new ArrayList<>();
        parent.put(key, created);
        return created;
    }

    private int getInt(final Object value) {
        if (value instanceof Number) {
            return ((Number) value).intValue();
        }
        return 0;
    }

    private String truncate(final String value, final int max) {
        if (value == null) {
            return "";
        }
        String trimmed = value.trim();
        if (trimmed.length() <= max) {
            return trimmed;
        }
        return trimmed.substring(0, max);
    }
}
