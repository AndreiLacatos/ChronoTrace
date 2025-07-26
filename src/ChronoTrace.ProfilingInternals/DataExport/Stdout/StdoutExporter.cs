using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChronoTrace.ProfilingInternals.DataExport.Stdout
{
    /// <summary>
    /// An internal implementation of <see cref="ITraceVisitor"/> that collects trace data
    /// and outputs it as text to stdout.
    /// </summary>
    internal sealed class StdoutExporter : ITraceVisitor
    {
        private IList<Trace> _traces;

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
            var sb = new StringBuilder();
            foreach (var trace in _traces ?? Enumerable.Empty<Trace>())
            {
                var totalMinutes = (int)trace.ExecutionTime.TotalMinutes;
                var seconds = trace.ExecutionTime.Seconds;
                var milliseconds = trace.ExecutionTime.Milliseconds;

                sb
                    .Append(trace.MethodName)
                    .Append(": ")
                    .Append($"{totalMinutes:D2}:{seconds:D2}.{milliseconds:D3}")
                    .AppendLine();
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
