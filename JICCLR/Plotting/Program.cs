using System.Diagnostics;
using ScottPlot;
using ScottPlot.Plottables;

var proc = new ProcessStartInfo {
    FileName = "java",
    Arguments = "-cp /Users/dayo/JIC/JICJVM/build/classes/java/main me.ddayo.jic.JIC",
    WorkingDirectory = "/Users/dayo/JIC/JICJVM/",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true
};

const int toIterate = 100;
var rst = Enumerable.Range(1, toIterate).Select(x => {
    Console.WriteLine($"Running on {x} / {toIterate}");
    return ExecuteProc(proc);
}).ToList();

SavePhasePlot(Enumerable.Range(0, rst.Count).Select(x => rst[0].Results[x]), "1");
SavePhasePlot(Enumerable.Range(0, rst.Count).Select(x => rst[0].Results[x]), "1-exclude-first", 3000);
SavePhasePlot(Enumerable.Range(0, rst.Count).Select(x => rst.Sum(y => y.Results[x]) / rst.Count), "All");
SavePhasePlot(Enumerable.Range(0, rst.Count).Select(x => rst.Sum(y => y.Results[x]) / rst.Count), "All-exclude-first", 3000);
SaveTotalPlot(rst);

MaxMinMean(rst.Select(x => x.Results[0]), "First");
MaxMinMean(rst.Select(x => x.Results.Last()), "Last");
MaxMinMean(rst.Select(x => x.Results[0] / x.Results.Sum()), "Bias");
return;

void SavePhasePlot(IEnumerable<double> result, string iteration, double? limit = null) {
    var plot = new Plot();
    plot.XAxis.Label.Text = "Iteration";
    plot.YAxis.Label.Text = "Elapsed time per iteration (us)";
    var plottable = result.ToList().Select((x, ix) => new Coordinates(ix + 1, x));
    var scatter =
        plot.Add.Scatter(plottable.Where(x => x.Y < (limit ?? double.MaxValue)).ToList());
    scatter.LineStyle.Color = Colors.Transparent;
    plot.YAxis.Max = limit ?? plot.YAxis.Max;
    plot.YAxis.Range.Set(0, limit ?? plot.YAxis.Range.Max);
    plot.SavePng($"IndividualPlot{iteration}.png", 640, 480);
}

void SaveTotalPlot(IEnumerable<ExecutionResult> result) {
    var resultCache = result.ToList();
    var l = resultCache.Select(x => x.Span.Ticks).ToList();

    MaxMinMean(l.Select(x => (double)x), "Total");

    var plot = new Plot();
    plot.YAxis.Label.Text = "Total elapsed time (sec)";
    plot.XAxis.Label.Text = "Repeat time";
    var scatter = plot.Add.Scatter(Generate.Consecutive(l.Count), l.Select(x => (double)x / 10_000_000).ToArray());
    scatter.LineStyle.Color = Colors.Transparent;
    scatter.Label = "Execution time";
    var firstScatter = plot.Add.Scatter(Generate.Consecutive(l.Count),
        resultCache.Select(x => x.Results[0] / 1000_000).ToArray());
    firstScatter.LineStyle.Color = Colors.Transparent;
    firstScatter.MarkerStyle.Fill.Color = Colors.Red;
    firstScatter.Label = "First iteration time";
    var totalScatter = plot.Add.Scatter(Generate.Consecutive(l.Count),
            resultCache.Select(x => x.Results.Sum() / 1000_000).ToArray());
    totalScatter.LineStyle.Color = Colors.Transparent;
    totalScatter.MarkerStyle.Fill.Color = Colors.Green;
    totalScatter.Label = "Total iteration time";

    var legend = plot.GetLegend();
    legend.Alignment = Alignment.UpperRight;
    
    plot.SavePng("totalExecutionPlot.png", 640, 480);
}

ExecutionResult ExecuteProc(ProcessStartInfo p) {
    var pp = Process.Start(p)!;
    var st = pp.StartTime;
    pp.WaitForExit();

    var o = pp.StandardOutput.ReadToEnd().Split("\n");
    var rsts = o[^7].Split(" ").Select(double.Parse).ToList();
    var x = o[^6..^3];
    var x1 = x[0].Split(" ");
    var x2 = x[1].Split(" ");
    var x3 = x[2].Split(" ");
    var mx = double.Parse(x1[1]);
    var mi = double.Parse(x1[3]);
    var mean = double.Parse(x1[5]);
    var spent = double.Parse(x2[1][..^1]);
    var loop = double.Parse(x3[^1][..^2]);
    Console.WriteLine(string.Join("  ", rsts));
    Console.WriteLine($"{mx} {mi} {mean} {spent} {loop}");

    return new ExecutionResult {
        Max = mx,
        Min = mi,
        Mean = mean,
        Spent = spent,
        Loop = loop,
        Span = pp.ExitTime - st,
        Results = rsts.ToList()
    };
}

void MaxMinMean(IEnumerable<double> e, string type) {
    var cache = e.ToList();
    Console.WriteLine($"Max {type}: {cache.Max()}");
    Console.WriteLine($"Min {type}: {cache.Min()}");
    Console.WriteLine($"Mean {type}: {cache.Sum() / cache.Count}");
}

struct ExecutionResult {
    public double Max, Min, Mean, Spent, Loop;
    public List<double> Results;
    public TimeSpan Span;
}
