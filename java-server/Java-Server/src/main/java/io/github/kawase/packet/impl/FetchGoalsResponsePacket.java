package io.github.kawase.packet.impl;

import io.github.kawase.packet.Packet;
import lombok.Getter;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.List;

@Getter
public class FetchGoalsResponsePacket extends Packet {
    public static class GoalDto {
        public long id;
        public String title;
        public String reward;
        public boolean isCompleted;

        public GoalDto(long id, String title, String reward, boolean isCompleted) {
            this.id = id;
            this.title = title;
            this.reward = reward;
            this.isCompleted = isCompleted;
        }
    }

    private List<GoalDto> goals = new ArrayList<>();

    public FetchGoalsResponsePacket(final List<GoalDto> goals) {
        super(0x0E);
        this.goals = goals;
    }

    public FetchGoalsResponsePacket() {
        super(0x0E);
    }

    @Override
    protected void write(final ByteBuffer buffer) {
        buffer.putInt(goals.size());
        for (GoalDto goal : goals) {
            buffer.putLong(goal.id);
            putString(goal.title, buffer);
            putString(goal.reward, buffer);
            buffer.put((byte) (goal.isCompleted ? 1 : 0));
        }
    }

    @Override
    protected void read(final ByteBuffer buffer) {
        int size = buffer.getInt();
        for (int i = 0; i < size; i++) {
            long id = buffer.getLong();
            String title = readString(buffer);
            String reward = readString(buffer);
            boolean isCompleted = buffer.get() == 1;
            goals.add(new GoalDto(id, title, reward, isCompleted));
        }
    }
}
