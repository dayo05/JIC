package me.ddayo.jic.example;

public abstract class AbstractTest {
    private int aa = -1;
    AbstractTest(int a) {
        aa = a;
    }

    public int abstractFunc() {
        return aa;
    }

    public int abtest(int a) {
        return a + 5;
    }
}
