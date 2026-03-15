package io.github.kawase.socket.packet.impl;

import io.github.kawase.socket.packet.Packet;
import java.nio.ByteBuffer;

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

    public String getName() {
        return name;
    }

    public int getTotalPoints() {
        return totalPoints;
    }

    public String getGameStatsJson() {
        return gameStatsJson;
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
