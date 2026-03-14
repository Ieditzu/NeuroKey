package io.github.kawase.client;

import io.github.kawase.Server;
import io.github.kawase.packet.Packet;
import io.github.kawase.packet.impl.*;
import io.github.kawase.socket.ServerSocket;
import lombok.Getter;
import lombok.RequiredArgsConstructor;
import org.java_websocket.WebSocket;

import java.nio.ByteBuffer;

@RequiredArgsConstructor
@Getter
public class ClientHandler {
    private final WebSocket connection;
    private final Client client;
    private final ServerSocket server;

    public void onMessage(final ByteBuffer byteBuffer) {
        int currentPacketId = -1;
        try {
            final Packet packet = Packet.construct(byteBuffer, client.getPacketManager());
            currentPacketId = packet.getId();

            // Handshake and Registration/Auth/QR Login/Verify Session are the only ones allowed before auth
            // we should prob not hard code this but oh well.
            if (currentPacketId != 1 && currentPacketId != 2 && currentPacketId != 3 && currentPacketId != 19 && currentPacketId != 25 && !client.isAuth()) {
                connection.send(new ActionResponsePacket(currentPacketId, false, "Unauthorized. Please log in first.", -1).encode());
                return;
            }

            switch (packet) {
                case HandShakePacket handShakePacket -> {
                    System.out.println("Got handshake from " + client.getHostID());
                }

                case AuthPacket authPacket -> {
                    System.out.println("Auth attempt: " + authPacket.getEmailHash());
                    final boolean success = Server.getInstance().getParentService().loginParent(
                            authPacket.getEmailHash(), authPacket.getPasswordHash()
                    );
                    if (success) {
                        final var parentOpt = Server.getInstance().getParentService().findByEmail(authPacket.getEmailHash());
                        if (parentOpt.isPresent()) {
                            final var parent = parentOpt.get();
                            client.setAuth(true);
                            client.setParentId(parent.getId());
                            connection.send(new AuthResponsePacket(true, parent.getId(), "Login successful").encode());
                        } else {
                            connection.send(new AuthResponsePacket(false, -1, "User not found").encode());
                        }
                    } else {
                        connection.send(new AuthResponsePacket(false, -1, "Invalid credentials").encode());
                    }
                }

                case RegisterParentPacket registerParentPacket -> {
                    System.out.println("Register Parent: " + registerParentPacket.getEmail());
                    final var parent = Server.getInstance().getParentService().createParentAccount(
                            registerParentPacket.getEmail(),
                            registerParentPacket.getPasswordHash()
                    );
                    client.setAuth(true);
                    client.setParentId(parent.getId());
                    connection.send(new ActionResponsePacket(packet.getId(), true, "Registered successfully", parent.getId()).encode());
                }

                case AddChildPacket addChildPacket -> {
                    System.out.println("Add Child: " + addChildPacket.getChildName());
                    final var child = Server.getInstance().getChildService().addChildToParent(
                            client.getParentId(),
                            addChildPacket.getChildName()
                    );
                    connection.send(new ActionResponsePacket(packet.getId(), true, "Child added successfully", child.getId()).encode());
                }

                case AddGoalPacket addGoalPacket -> {
                    System.out.println("Add Goal: " + addGoalPacket.getTitle());
                    // Verify ownership
                    final var child = Server.getInstance().getChildService().findById(addGoalPacket.getChildId())
                            .orElseThrow(() -> new RuntimeException("Child not found"));

                    if (!child.getParent().getId().equals(client.getParentId())) {
                        throw new RuntimeException("Access denied: This child does not belong to you.");
                    }

                    io.github.kawase.database.entity.Goal goal;
                    if (addGoalPacket.getRequiredTaskId() != -1) {
                        goal = Server.getInstance().getGoalService().createTaskGoal(
                                client.getParentId(),
                                addGoalPacket.getChildId(),
                                addGoalPacket.getTitle(),
                                addGoalPacket.getReward(),
                                addGoalPacket.getRequiredTaskId()
                        );
                    } else {
                        goal = Server.getInstance().getGoalService().createPointsGoal(
                                client.getParentId(),
                                addGoalPacket.getChildId(),
                                addGoalPacket.getTitle(),
                                addGoalPacket.getReward(),
                                addGoalPacket.getRequiredPoints()
                        );
                    }
                    connection.send(new ActionResponsePacket(packet.getId(), true, "Goal added successfully", goal.getId()).encode());
                }

                case CompleteTaskPacket completeTaskPacket -> {
                    System.out.println("Complete Task: Child " + completeTaskPacket.getChildId() + ", Task " + completeTaskPacket.getTaskId());
                    
                    if (client.getChildId() != null && !client.getChildId().equals(completeTaskPacket.getChildId())) {
                        throw new RuntimeException("Access denied: You can only complete tasks for yourself.");
                    }

                    if (client.getChildId() == null) {
                        final var child = Server.getInstance().getChildService().findById(completeTaskPacket.getChildId())
                                .orElseThrow(() -> new RuntimeException("Child not found"));

                        if (!child.getParent().getId().equals(client.getParentId())) {
                            throw new RuntimeException("Access denied.");
                        }
                    }

                    Server.getInstance().getTaskService().completeTask(
                            completeTaskPacket.getChildId(),
                            completeTaskPacket.getTaskId()
                    );

                    connection.send(new ActionResponsePacket(packet.getId(), true, "Task completed", -1).encode());
                }

                case FetchTasksPacket fetchTasksPacket -> {
                    System.out.println("Fetch Tasks for Parent: " + client.getParentId());
                    final var tasks = Server.getInstance().getParentService().getTasks(client.getParentId());
                    final var dtos = new java.util.ArrayList<FetchTasksResponsePacket.TaskDto>();
                    for (final var task : tasks) {
                        dtos.add(new FetchTasksResponsePacket.TaskDto(task.getId(), task.getTitle(), task.getPointValue()));
                    }
                    connection.send(new FetchTasksResponsePacket(dtos).encode());
                }

                case FetchChildStatsPacket fetchChildStatsPacket -> {
                    if (client.getChildId() == null) {
                        throw new RuntimeException("Not logged in as a child.");
                    }
                    final var child = Server.getInstance().getChildService().findById(client.getChildId())
                            .orElseThrow(() -> new RuntimeException("Child not found"));
                    
                    String json = "{}";
                    try {
                        json = new com.fasterxml.jackson.databind.ObjectMapper().writeValueAsString(child.getGameStats());
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    
                    connection.send(new FetchChildStatsResponsePacket(child.getName(), child.getTotalPoints(), json).encode());
                }

                case FetchChildrenPacket fetchChildrenPacket -> {
                    System.out.println("Fetch Children for Parent: " + client.getParentId());
                    final var children = Server.getInstance().getParentService().getChildren(client.getParentId());
                    final var dtos = new java.util.ArrayList<FetchChildrenResponsePacket.ChildDto>();
                    
                    // Get all active child IDs from current connections
                    java.util.Set<Long> onlineChildIds = new java.util.HashSet<>();
                    for (var entry : Server.getInstance().getActiveConnections().keySet()) {
                        if (entry.getChildId() != null) {
                            onlineChildIds.add(entry.getChildId());
                        }
                    }

                    for (final var child : children) {
                        boolean isOnline = onlineChildIds.contains(child.getId());
                        dtos.add(new FetchChildrenResponsePacket.ChildDto(child.getId(), child.name(), child.getTotalPoints(), isOnline));
                    }
                    connection.send(new FetchChildrenResponsePacket(dtos).encode());
                }

                case FetchGoalsPacket fetchGoalsPacket -> {
                    System.out.println("Fetch Goals for Child ID: " + fetchGoalsPacket.getChildId());
                    final var child = Server.getInstance().getChildService().findById(fetchGoalsPacket.getChildId())
                            .orElseThrow(() -> new RuntimeException("Child not found"));

                    if (!child.getParent().getId().equals(client.getParentId())) {
                        throw new RuntimeException("Access denied.");
                    }

                    final var goals = Server.getInstance().getChildService().getGoals(fetchGoalsPacket.getChildId());
                    final var dtos = new java.util.ArrayList<FetchGoalsResponsePacket.GoalDto>();
                    for (final var goal : goals) {
                        dtos.add(new FetchGoalsResponsePacket.GoalDto(
                                goal.getId(),
                                goal.getTitle(),
                                goal.getReward(),
                                goal.getIsCompleted(),
                                goal.getRequiredPoints() != null ? goal.getRequiredPoints() : 0,
                                goal.getRequiredTask() != null ? goal.getRequiredTask().getId() : -1L
                        ));
                    }
                    connection.send(new FetchGoalsResponsePacket(dtos).encode());
                }

                case FetchCompletedTasksPacket fetchCompletedTasksPacket -> {
                    System.out.println("Fetch Completed Tasks for Child: " + fetchCompletedTasksPacket.getChildId());
                    final var child = Server.getInstance().getChildService().findById(fetchCompletedTasksPacket.getChildId())
                            .orElseThrow(() -> new RuntimeException("Child not found"));

                    if (!child.getParent().getId().equals(client.getParentId())) {
                        throw new RuntimeException("Access denied.");
                    }

                    final var completedTasks = Server.getInstance().getChildService().getCompletedTasks(fetchCompletedTasksPacket.getChildId());
                    final var dtos = new java.util.ArrayList<FetchCompletedTasksResponsePacket.CompletedTaskDto>();
                    for (final var ct : completedTasks) {
                        final var fmt = java.time.format.DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm");
                        dtos.add(new FetchCompletedTasksResponsePacket.CompletedTaskDto(
                                ct.getId(),
                                ct.getTask().getTitle(),
                                ct.getTask().getPointValue(),
                                ct.getCompletedAt().format(fmt)
                        ));
                    }
                    connection.send(new FetchCompletedTasksResponsePacket(dtos).encode());
                }

                case GenerateQRLoginPacket generateQRLoginPacket -> {
                    final String token = java.util.UUID.randomUUID().toString();
                    System.out.println("Generating QR token: " + token);
                    Server.getInstance().getPendingQRLogins().put(token, this);
                    connection.send(new QRLoginResponsePacket(token).encode());
                }

                case ClaimQRLoginPacket claimQRLoginPacket -> {
                    System.out.println("Claiming QR token: " + claimQRLoginPacket.getToken() + " for child " + claimQRLoginPacket.getChildId());
                    final ClientHandler gameHandler = Server.getInstance().getPendingQRLogins().remove(claimQRLoginPacket.getToken());
                    
                    if (gameHandler != null && gameHandler.getConnection().isOpen()) {
                        final var childOpt = Server.getInstance().getChildService().findById(claimQRLoginPacket.getChildId());
                        if (childOpt.isPresent()) {
                            final var child = childOpt.get();
                            
                            // Verify that the parent claiming the token actually owns this child
                            if (child.getParent().getId().equals(client.getParentId())) {
                                final String sessionToken = java.util.UUID.randomUUID().toString();
                                Server.getInstance().getGameSessionService().createOrUpdateSession(child.getId(), sessionToken);

                                gameHandler.getClient().setAuth(true);
                                gameHandler.getClient().setChildId(child.getId());
                                gameHandler.getClient().setParentId(child.getParent().getId());
                                
                                gameHandler.getConnection().send(new ChildAuthResponsePacket(true, child.getId(), child.getName(), sessionToken).encode());
                                connection.send(new ActionResponsePacket(packet.getId(), true, "Child logged into game successfully", child.getId()).encode());
                            } else {
                                connection.send(new ActionResponsePacket(packet.getId(), false, "Access denied: You don't own this child", -1).encode());
                            }
                        } else {
                            connection.send(new ActionResponsePacket(packet.getId(), false, "Child not found", -1).encode());
                        }
                    } else {
                        connection.send(new ActionResponsePacket(packet.getId(), false, "Invalid or expired QR code", -1).encode());
                    }
                }

                case VerifySessionPacket verifySessionPacket -> {
                    System.out.println("Verifying session for child " + verifySessionPacket.getChildId());
                    final boolean isValid = Server.getInstance().getGameSessionService().verifySession(
                            verifySessionPacket.getChildId(), 
                            verifySessionPacket.getSessionToken()
                    );
                    
                    if (isValid) {
                        final var childOpt = Server.getInstance().getChildService().findById(verifySessionPacket.getChildId());
                        if (childOpt.isPresent()) {
                            final var child = childOpt.get();
                            client.setAuth(true);
                            client.setChildId(child.getId());
                            client.setParentId(child.getParent().getId());
                            
                            connection.send(new ChildAuthResponsePacket(true, child.getId(), child.getName(), verifySessionPacket.getSessionToken()).encode());
                        } else {
                            connection.send(new ChildAuthResponsePacket(false, -1, "", "").encode());
                        }
                    } else {
                        connection.send(new ChildAuthResponsePacket(false, -1, "", "").encode());
                    }
                }

                default -> throw new IllegalStateException("Unexpected Packet: " + packet);
            }
        } catch (Exception e) {
            e.printStackTrace();
            if (currentPacketId != -1 && connection.isOpen()) {
                connection.send(new ActionResponsePacket(currentPacketId, false, e.getMessage() != null ? e.getMessage() : "Unknown error", -1).encode());
            }
        }
    }

    public void onOpen() {

    }

    public void onClose() {

    }
}
