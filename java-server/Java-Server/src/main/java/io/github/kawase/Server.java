package io.github.kawase;

import io.github.kawase.client.Client;
import io.github.kawase.client.ClientHandler;
import io.github.kawase.packet.PacketManager;
import io.github.kawase.socket.ServerSocket;
import lombok.Getter;

import java.util.concurrent.ConcurrentHashMap;

@Getter
public class Server {
    @Getter
    private final static Server instance = new Server();

    private ConcurrentHashMap<Client, ClientHandler> activeConnections;

    private ServerSocket socket;

    // we will init every manager here :pray:.
    private PacketManager packetManager = new PacketManager();

    // todo: socket
    public void init(final int port) {
        this.packetManager = new PacketManager();
        this.activeConnections = new ConcurrentHashMap<>();

        this.socket = new ServerSocket(port);

        socket.setReuseAddr(true);
        socket.setTcpNoDelay(true);

        // start the socket itself.
        socket.start();
    }
}
