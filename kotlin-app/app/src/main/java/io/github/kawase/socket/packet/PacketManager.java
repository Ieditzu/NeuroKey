package io.github.kawase.socket.packet;

import io.github.kawase.socket.exceptions.PacketException;
import io.github.kawase.socket.packet.impl.*;

public class PacketManager {
    public Packet createPacket(final int id) {
        switch (id) {
            case 1: return new HandShakePacket();
            case 2: return new AuthPacket();
            case 3: return new RegisterParentPacket();
            case 4: return new AddChildPacket();
            case 5: return new AddGoalPacket();
            case 8: return new CompleteTaskPacket();
            case 9: return new ActionResponsePacket();
            case 10: return new AuthResponsePacket();
            case 11: return new FetchTasksPacket();
            case 12: return new FetchTasksResponsePacket();
            case 13: return new FetchGoalsPacket();
            case 14: return new FetchGoalsResponsePacket();
            case 15: return new FetchChildrenPacket();
            case 16: return new FetchChildrenResponsePacket();
            case 17: return new FetchCompletedTasksPacket();
            case 18: return new FetchCompletedTasksResponsePacket();
            case 21: return new ClaimQRLoginPacket();
            case 26: return new UpdatePfpPacket();
        }

        return null;
    }
}
