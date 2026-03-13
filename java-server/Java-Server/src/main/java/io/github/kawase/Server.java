package io.github.kawase;

import io.github.kawase.packet.PacketManager;
import lombok.Getter;

@Getter
public class Server {
    @Getter
    private final static Server instance = new Server();

    // we will init every manager here :pray:.
    private PacketManager packetManager = new PacketManager();

    // todo: socket
    public void init(final int port) {
        this.packetManager = new PacketManager();
    }
}
