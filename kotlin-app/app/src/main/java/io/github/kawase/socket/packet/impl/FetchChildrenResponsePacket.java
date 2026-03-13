package io.github.kawase.socket.packet.impl;

import io.github.kawase.socket.packet.Packet;

import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.List;

public class FetchChildrenResponsePacket extends Packet {
    public static class ChildDto {
        public long id;
        public String name;
        public int totalPoints;

        public ChildDto(long id, String name, int totalPoints) {
            this.id = id;
            this.name = name;
            this.totalPoints = totalPoints;
        }
    }

    private List<ChildDto> children = new ArrayList<>();

    public FetchChildrenResponsePacket(final List<ChildDto> children) {
        super(0x10);
        this.children = children;
    }

    public FetchChildrenResponsePacket() {
        super(0x10);
    }

    @Override
    protected void write(final ByteBuffer buffer) {
        buffer.putInt(children.size());
        for (ChildDto child : children) {
            buffer.putLong(child.id);
            putString(child.name, buffer);
            buffer.putInt(child.totalPoints);
        }
    }

    @Override
    protected void read(final ByteBuffer buffer) {
        int size = buffer.getInt();
        for (int i = 0; i < size; i++) {
            long id = buffer.getLong();
            String name = readString(buffer);
            int points = buffer.getInt();
            children.add(new ChildDto(id, name, points));
        }
    }

    public List<ChildDto> getChildren() {
        return children;
    }
}
