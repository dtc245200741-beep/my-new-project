package com.codegym;

public class Main {
    public static void main(String[] args) {
        System.out.println("2x^2 - 3x - 2 = 0: " + QuadraticSolver.solve(2, -3, -2));
        System.out.println("x^2 - 4x + 4 = 0: " + QuadraticSolver.solve(1, -4, 4));
        System.out.println("x^2 + x + 1 = 0: " + QuadraticSolver.solve(1, 1, 1));
    }
}
