package io.github.kawase;

import io.github.kawase.packet.Packet;
import io.github.kawase.packet.PacketManager;
import io.github.kawase.packet.impl.HandShakePacket;

import java.nio.ByteBuffer;

public class StartServer {
    public static void main(String[] args) throws Exception {
        Server.getInstance().init(69420);

        final ByteBuffer encodedPacket =  new HandShakePacket("Test").encode();

        printBufferInfo(encodedPacket);

        // attempts to decode it.
        final Packet decodedPacket = Packet.construct(encodedPacket, Server.getInstance().getPacketManager());
        if (decodedPacket instanceof HandShakePacket handShakePacket) {
            System.out.println(handShakePacket.getClientFingerPrint());
        }
    }

    // temporary method used for testing the packets for now.
    public static void printBufferInfo(ByteBuffer buffer) {
        if (buffer == null) {
            System.out.println("Buffer is null.");
            return;
        }

        System.out.println("--- ByteBuffer Info ---");
        System.out.printf("Direct:   %b%n", buffer.isDirect());
        System.out.printf("ReadOnly: %b%n", buffer.isReadOnly());
        System.out.printf("Position: %d%n", buffer.position());
        System.out.printf("Limit:    %d%n", buffer.limit());
        System.out.printf("Capacity: %d%n", buffer.capacity());
        System.out.printf("Remaining:%d%n", buffer.remaining());

        System.out.print("Data (Hex): [ ");
        for (int i = 0; i < buffer.limit(); i++) {
            System.out.printf("%02X ", buffer.get(i));
        }

        System.out.println("]");
        System.out.println("-----------------------");
    }
}