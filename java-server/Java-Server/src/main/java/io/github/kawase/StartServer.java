package io.github.kawase;

import io.github.kawase.packet.Packet;
import io.github.kawase.packet.PacketManager;
import io.github.kawase.packet.impl.HandShakePacket;

import java.nio.ByteBuffer;

public class StartServer {
    public static void main(String[] args) throws Exception {
        Server.getInstance().init(69420);
    }
}