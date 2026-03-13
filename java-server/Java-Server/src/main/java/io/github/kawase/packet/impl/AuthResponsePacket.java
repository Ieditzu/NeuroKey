package io.github.kawase.packet.impl;

import io.github.kawase.packet.Packet;
import lombok.Getter;
import java.nio.ByteBuffer;

@Getter
public class AuthResponsePacket extends Packet {
    private boolean success;
    private long parentId;
    private String message;

    public AuthResponsePacket(final boolean success, final long parentId, final String message) {
        super(0x0A);
        this.success = success;
        this.parentId = parentId;
        this.message = message;
    }

    public AuthResponsePacket() {
        super(0x0A);
    }

    @Override
    protected void write(final ByteBuffer buffer) {
        buffer.put((byte) (success ? 1 : 0));
        buffer.putLong(parentId);
        putString(message == null ? "" : message, buffer);
    }

    @Override
    protected void read(final ByteBuffer buffer) {
        this.success = buffer.get() == 1;
        this.parentId = buffer.getLong();
        this.message = readString(buffer);
    }
}
