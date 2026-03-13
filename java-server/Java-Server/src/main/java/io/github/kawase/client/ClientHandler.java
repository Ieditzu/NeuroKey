package io.github.kawase.client;

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


    public void onMessage(final ByteBuffer encryptedBuffer) {

    }

    public void onOpen() {

    }

    public void onClose() {

    }
}
