package io.github.kawase.database.entity.enums;

import lombok.Getter;
import lombok.RequiredArgsConstructor;

@Getter
@RequiredArgsConstructor
public enum DefaultTaskType {
    CPP_BASICS("C++ Basics: Hello World Quiz", 10),
    VARIABLES("Variables & Data Types Challenge", 15),
    LOGIC_GATES("The 'If-Else' Logic Gate", 20),
    LOOP_MASTER("Loop Master: For-Loops Challenge", 25),
    ARRAY_ARCHITECT("Array Architect: Storing Data", 30),
    FUNCTION_HERO("Function Hero: Reusable Code", 35),
    POINTER_PIONEER("Pointer Pioneer: Memory Basics", 50),
    CLASS_CREATOR("Class Creator: Intro to OOP", 60),
    OOP_INHERITANCE("Object Oriented: Inheritance Quiz", 75),
    MEMORY_MANAGEMENT("Final Boss: Memory Management", 100);

    private final String title;
    private final int pointValue;
}
