package io.github.kawase.socket;

import io.github.kawase.packet.Packet;
import io.github.kawase.packet.PacketManager;
import io.github.kawase.packet.impl.HandShakePacket;
import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;

import java.net.URI;
import java.nio.ByteBuffer;
import java.util.concurrent.CountDownLatch;

public class ClientSocket extends WebSocketClient {
    private final CountDownLatch connectLatch = new CountDownLatch(1);
    private final PacketManager packetManager = new PacketManager();

    public ClientSocket(final URI serverUri) {
        super(serverUri);
    }

    @Override
    public void onOpen(final ServerHandshake handshake) {
        System.out.println("Connected to server.");
        connectLatch.countDown();
    }

    @Override
    public void onMessage(String message) {
        /* w */
    }

    @Override
    public void onMessage(final ByteBuffer bytes) {
        try {
            final Packet packet = Packet.construct(bytes, packetManager);

            switch (packet) {
                case HandShakePacket handShakePacket -> {
                    System.out.println("Got handshake");
                }

                default -> System.out.println("Unexpected packet received: " + packet.getClass().getSimpleName());
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    public void onClose(int code, String reason, boolean remote) {
        System.out.println("Connection closed: " + reason);
    }

    @Override
    public void onError(Exception ex) {
        ex.printStackTrace();
    }

    public void awaitConnection() throws InterruptedException {
        connectLatch.await();
    }

    public static void main(String[] args) {
        try {
            final ClientSocket client = new ClientSocket(new URI("ws://127.0.0.1:8887"));
            client.connect();

            client.awaitConnection();

            System.out.println("Connected");

        } catch (Exception e) {
            System.out.println("Failed to connected.");
            e.printStackTrace();
        }
    }
}