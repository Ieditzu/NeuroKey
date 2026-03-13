package io.github.kawase.packet;

import io.github.kawase.exceptions.PacketException;
import io.github.kawase.packet.impl.HandShakePacket;

public class PacketManager {
    public Packet createPacket(final int id) {
        return switch (id) {
            case 1 -> new HandShakePacket();

            default -> throw new PacketException("Unknown packet ID: " + id);
        };
    }
}
