using System.Text;
using System.Text.Json;

namespace ChronoTrace.ProfilingInternals.DataExport.Stdout;

/// <summary>
/// An internal implementation of <see cref="ITraceVisitor"/> that collects trace data
/// and outputs it as text to stdout.
/// </summary>
internal sealed class StdoutExporter : ITraceVisitor
{
    private readonly CallGraphBuilder _callGraphBuilder;
    private List<Trace>? _traces;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
    };

    public StdoutExporter(CallGraphBuilder callGraphBuilder)
    {
        _callGraphBuilder = callGraphBuilder;
    }
    
    public void BeginVisit()
    {
        _traces = new List<Trace>();
    }

    public void VisitTrace(Trace trace)
    {
        _traces?.Add(trace);
    }

    public void Complete()
    {
        var callGraphs = _callGraphBuilder.AssembleCallGraph(_traces ?? []);
        var sb = new StringBuilder();
        foreach (var graph in callGraphs)
        {
            Format(graph.Root, sb);
        }

        Console.WriteLine(sb.ToString());
    }

    private static void Format(CallGraph.Node node, StringBuilder accumulator, int level = 0)
    {
        var totalMinutes = (int)node.ExecutionTime.TotalMinutes;
        var seconds = node.ExecutionTime.Seconds;
        var milliseconds = node.ExecutionTime.Milliseconds;

        var prefix = new string(' ', 4 * level);
        accumulator
            .Append(prefix)
            .Append(node.MethodName)
            .Append(": ")
            .Append($"{totalMinutes:D2}:{seconds:D2}.{milliseconds:D3}")
            .AppendLine();

        foreach (var child in node.Nodes)
        {
            Format(child, accumulator, level + 1);
        }
    }
}
