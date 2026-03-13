package io.github.kawase.socket;

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
        final String type = handshake.getFieldValue("Type");

        final String remoteID = conn.getRemoteSocketAddress().getHostString() + ":" +
                conn.getRemoteSocketAddress().getPort();

    }

    @Override
    public void onMessage(WebSocket conn, String message) {

    }

    @Override
    public void onMessage(WebSocket conn, ByteBuffer blob) {

    }

    @Override
    public void onClose(WebSocket conn, int code, String reason, boolean remote) {

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