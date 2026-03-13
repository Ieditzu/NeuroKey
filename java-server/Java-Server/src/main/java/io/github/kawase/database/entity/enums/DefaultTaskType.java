package io.github.kawase.database.entity.enums;

import lombok.Getter;
import lombok.RequiredArgsConstructor;

@Getter
@RequiredArgsConstructor
public enum DefaultTaskType {
    CLEAN_ROOM("Clean your room", 10),
    DO_HOMEWORK("Complete homework", 15),
    READ_BOOK("Read a book for 30 minutes", 5),
    BRUSH_TEETH("Brush teeth", 5),
    WASH_DISHES("Wash the dishes", 10),
    MAKE_BED("Make the bed", 5);

    private final String title;
    private final int pointValue;
}