package io.github.kawase.socket;

import io.github.kawase.Server;
import io.github.kawase.client.Client;
import io.github.kawase.client.ClientHandler;
import org.java_websocket.server.WebSocketServer;
import org.java_websocket.handshake.ClientHandshake;
import org.java_websocket.WebSocket;

import java.net.InetSocketAddress;
import java.nio.ByteBuffer;

public class ServerSocket extends WebSocketServer {
    public ServerSocket(int port) {
        super(new InetSocketAddress("0.0.0.0", port));
    }

    @Override
    public void onOpen(WebSocket conn, ClientHandshake handshake) {
        final String remoteID = conn.getRemoteSocketAddress().getHostString() + ":" +
                conn.getRemoteSocketAddress().getPort();

        final Client client = new Client(remoteID, Server.getInstance().getPacketManager());
        final ClientHandler clientHandler = new ClientHandler(client);

        // we prob shouldn't make the client object as the key for the hash map.
        Server.getInstance().getActiveConnections().put(client, clientHandler);

        conn.setAttachment(clientHandler);

        System.out.println("Started server socket.");
    }

    @Override
    public void onMessage(WebSocket conn, String message) {
        final ClientHandler clientHandler = conn.getAttachment();

        System.out.println("Invalid protocol detected, disconnecting: " + clientHandler.getClient().getHostID());

        Server.getInstance().getActiveConnections().remove(clientHandler.getClient());
        conn.close();
    }

    @Override
    public void onMessage(WebSocket conn, ByteBuffer blob) {
        final ClientHandler clientHandler = conn.getAttachment();

        clientHandler.onMessage(blob);
    }

    @Override
    public void onClose(WebSocket conn, int code, String reason, boolean remote) {
        final ClientHandler clientHandler = conn.getAttachment();

        Server.getInstance().getActiveConnections().remove(clientHandler.getClient());
        conn.close();

        System.out.println("Closed connection: " + clientHandler.getClient().getHostID());
    }

    @Override
    public void onError(WebSocket conn, Exception ex) {
        ex.printStackTrace();
    }

    @Override
    public void onStart() {
        System.out.println("Server started on port: " + getPort());
    }
}