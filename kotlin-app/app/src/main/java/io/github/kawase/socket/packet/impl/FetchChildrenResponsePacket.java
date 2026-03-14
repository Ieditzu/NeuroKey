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
        public boolean isOnline;

        public ChildDto(long id, String name, int totalPoints, boolean isOnline) {
            this.id = id;
            this.name = name;
            this.totalPoints = totalPoints;
            this.isOnline = isOnline;
        }
    }

    private List<ChildDto> children = new ArrayList<>();

    public FetchChildrenResponsePacket(final List<ChildDto> children) {
        super(0x0D);
        this.children = children;
    }

    public FetchChildrenResponsePacket() {
        super(0x0D);
    }

    public List<ChildDto> getChildren() {
        return children;
    }

    @Override
    protected void write(final ByteBuffer buffer) {
        buffer.putInt(children.size());
        for (ChildDto child : children) {
            buffer.putLong(child.id);
            putString(child.name, buffer);
            buffer.putInt(child.totalPoints);
            buffer.put((byte) (child.isOnline ? 1 : 0));
        }
    }

    @Override
    protected void read(final ByteBuffer buffer) {
        int size = buffer.getInt();
        for (int i = 0; i < size; i++) {
            long id = buffer.getLong();
            String name = readString(buffer);
            int points = buffer.getInt();
            boolean isOnline = buffer.get() == 1;
            children.add(new ChildDto(id, name, points, isOnline));
        }
    }
}
