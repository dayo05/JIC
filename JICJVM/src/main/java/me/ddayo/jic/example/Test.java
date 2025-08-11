package me.ddayo.jic.example;

import java.io.FileNotFoundException;
import java.io.PrintWriter;

public class Test extends AbstractTest {
    private int v = 0;

    @Override
    public int abtest(int a) {
        return super.abtest(a);
    }

    public Test() throws FileNotFoundException {
        super(123);
    }

    public Test(int x) throws FileNotFoundException {
        super(x);
        v = x;
    }

    public void setVar(int x, int y) {
        v = x + y;
    }

    public int getVar() {
        return v;
    }

    public int apply(Test other) {
        v += other.v;
        return v;
    }

    public void test(int a) throws FileNotFoundException {
        var x = new PrintWriter("asdfasdf.asdf");
        x.println("Hello, world!");
        x.println(a);
        x.close();
    }

    public void test2(double a) throws FileNotFoundException {
        var x = new PrintWriter("asdfasdf2.asdf");
        x.println(a);
        x.close();
    }

    PrintWriter x = new PrintWriter("asdf.test.output");
    public Test self(int a) throws FileNotFoundException {
        x.append(String.valueOf(a)).append("\n");
        x.flush();
        return this;
    }

    public double k() {
        return 10.5;
    }
    public int asdf(int a, double b) {
        PrintWriter x = null;
        try {
            x = new PrintWriter("asdfasdfasdf.asdf");
        } catch (FileNotFoundException e) {
            throw new RuntimeException(e);
        }
        x.println(a + " " + b);
        x.close();
        return (int) (a + b);
    }
}
