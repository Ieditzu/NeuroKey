package io.github.kawase.client;

import lombok.Getter;
import lombok.RequiredArgsConstructor;

import java.nio.ByteBuffer;

@RequiredArgsConstructor
@Getter
public class ClientHandler {
    private final Client client;

    public void onMessage(final ByteBuffer encryptedBuffer) {

    }

    public void onOpen() {

    }

    public void onClose() {

    }
}
