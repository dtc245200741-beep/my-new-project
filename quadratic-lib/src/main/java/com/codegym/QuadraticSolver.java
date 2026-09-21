package com.codegym;

public final class QuadraticSolver {
    private QuadraticSolver() {
    }

    public static String solve(double a, double b, double c) {
        if (a == 0) {
            if (b == 0) {
                return c == 0 ? "Infinitely many solutions" : "No solution";
            }
            return "One solution: x = " + format(-c / b);
        }

        double delta = b * b - 4 * a * c;
        if (delta > 0) {
            double x1 = (-b + Math.sqrt(delta)) / (2 * a);
            double x2 = (-b - Math.sqrt(delta)) / (2 * a);
            return "Two solutions: x1 = " + format(x1) + ", x2 = " + format(x2);
        }
        if (delta == 0) {
            return "Double solution: x = " + format(-b / (2 * a));
        }
        return "No real solution";
    }

    private static String format(double value) {
        if (value == (long) value) {
            return Long.toString((long) value);
        }
        return Double.toString(value);
    }
}
