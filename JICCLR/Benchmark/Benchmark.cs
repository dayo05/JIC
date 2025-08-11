using System.Diagnostics;

namespace Benchmark;


public class Benchmark: IDisposable {
    public string Name { get; }
    private Action<TimeSpan>? Callback = null;
    public bool Mute;
    public Benchmark(string name, Action<TimeSpan>? callback = null) {
        Name = name;
        Callback = callback;
        Start();
    }

    public Benchmark(string name) {
        Name = name;
        Callback = x => { Console.WriteLine($"Benchmark for {Name}: {x}"); };
        Start();
    }
    
    private readonly Stopwatch stopwatch = new();

    public void Start() {
        stopwatch.Start();
    }
    
    public void Dispose() {
        stopwatch.Stop();
        Callback?.Invoke(stopwatch.Elapsed);
        GC.SuppressFinalize(this);
    }

    public TimeSpan Elapsed => stopwatch.Elapsed;
}