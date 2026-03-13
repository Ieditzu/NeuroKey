package io.github.kawase.packet.impl;

import io.github.kawase.packet.Packet;

import java.nio.ByteBuffer;

public class AuthPacket extends Packet {
    private String email, passwordHash;

    public AuthPacket(final String email, final String passwordHash) {
        super(0x02);
        this.email = email;
        this.passwordHash = passwordHash;
    }

    public AuthPacket() {
        super(0x02);
    }

    @Override
    protected void write(ByteBuffer buffer) {

    }

    @Override
    protected void read(ByteBuffer buffer) {

    }
}
