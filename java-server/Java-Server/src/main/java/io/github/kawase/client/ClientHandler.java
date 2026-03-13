package io.github.kawase.client;

import io.github.kawase.Server;
import io.github.kawase.packet.Packet;
import io.github.kawase.packet.impl.HandShakePacket;
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

            switch (packet) {
                case HandShakePacket handShakePacket -> {
                    System.out.println("Got Hand shake");
                }
                default -> System.out.println("Unknown packet!: " + currentPacketId);
            }
        } catch (Exception e ) {
            e.printStackTrace();
        }
    }

    public void onOpen() {

    }

    public void onClose() {

    }
}
