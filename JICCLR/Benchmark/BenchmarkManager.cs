namespace Benchmark; 

public static class BenchmarkManager {
    private static Dictionary<string, BenchmarkState> benchmarks = new();
    public static Benchmark CreateRepeatableBenchmark(string name) {
        benchmarks.TryAdd(name, new());
        return new Benchmark(name, x => {
            benchmarks[name].AddResult(x);
        });
    }

    public static BenchmarkState GetResults(string name)
        => benchmarks[name];

    public class BenchmarkState {
        public readonly List<double> Results = new();
        public double Max => Results.Max();
        public double Mean => Results.Sum() / Results.Count;
        public double Min => Results.Min();

        public void AddResult(TimeSpan t) {
            Results.Add(t.Ticks / 10.0);
        }
    }
}