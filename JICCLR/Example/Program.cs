using Benchmark;
using JIC;

namespace Example;

public class Program {
    [MainEntry]
    public static void F() {
        using var _ = new Benchmark.Benchmark("ClrMain::Entry", x => {
            Console.WriteLine($"Spent {x.Ticks}t {x.Ticks / 10.0}us inside main loop");
        });

        foreach (var iter in Enumerable.Range(1, 1000)) {
            using var __ = BenchmarkManager.CreateRepeatableBenchmark("Entry");
            dynamic x = new Test();
            x.test(12);
            x.test2(1.0);

            x.setVar(12, 13);
            Console.WriteLine(x.getVar());

            dynamic other = new Test();
            other.setVar(10, 155);
            Console.WriteLine(other.getVar());
            Console.WriteLine(x.apply(other));

            x.self(15)
                .self(30)
                .self(13);

            dynamic a = new Test(55);
            x.apply(a);
            Console.WriteLine($"A: {a.getVar()}");
            Console.WriteLine($"X: {x.getVar()}");

            Console.WriteLine(x.k());

            Console.WriteLine(((dynamic)new Integer(15)).doubleValue());
            Console.WriteLine(x.asdf(12, 15.0));

            Console.WriteLine(x.abtest(10));
        }

        var result = BenchmarkManager.GetResults("Entry");
        Console.WriteLine(string.Join(" ", result.Results));
        Console.WriteLine($"Max: {result.Max} Min: {result.Min} Mean: {result.Mean}");
    }
}