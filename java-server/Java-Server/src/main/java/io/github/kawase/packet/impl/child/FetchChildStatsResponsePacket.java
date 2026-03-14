package io.github.kawase.packet.impl.child;

import io.github.kawase.packet.Packet;
import lombok.Getter;
import java.nio.ByteBuffer;

@Getter
public class FetchChildStatsResponsePacket extends Packet {
    private String name;
    private int totalPoints;
    private String gameStatsJson;

    public FetchChildStatsResponsePacket(final String name, final int totalPoints, final String gameStatsJson) {
        super(24);
        this.name = name;
        this.totalPoints = totalPoints;
        this.gameStatsJson = gameStatsJson;
    }

    public FetchChildStatsResponsePacket() {
        super(24);
    }

    @Override
    protected void write(final ByteBuffer buffer) {
        putString(name == null ? "" : name, buffer);
        buffer.putInt(totalPoints);
        putString(gameStatsJson == null ? "{}" : gameStatsJson, buffer);
    }

    @Override
    protected void read(final ByteBuffer buffer) {
        this.name = readString(buffer);
        this.totalPoints = buffer.getInt();
        this.gameStatsJson = readString(buffer);
    }
}
