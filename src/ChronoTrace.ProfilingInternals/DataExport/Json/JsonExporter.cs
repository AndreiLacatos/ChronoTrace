using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using ChronoTrace.ProfilingInternals.DataExport.FileRotation;

namespace ChronoTrace.ProfilingInternals.DataExport.Json
{
    /// <summary>
    /// An internal implementation of <see cref="ITraceVisitor"/> that collects trace data
    /// and exports it as a JSON file. It utilizes providers for determining the output
    /// directory and file name, and a strategy for file rotation.
    /// </summary>
    internal sealed class JsonExporter : ITraceVisitor
    {
        private TimingReport _timingReport;
        private readonly IExportDirectoryProvider _exportDirectoryProvider;
        private readonly IJsonFileNameProvider _jsonFileNameProvider;
        private readonly IFileRotationStrategy _fileRotator;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private static readonly SemaphoreSlim FileSystemLock = new SemaphoreSlim(1, 1);

        internal JsonExporter(
            IExportDirectoryProvider exportDirectoryProvider,
            IJsonFileNameProvider jsonFileNameProvider,
            IFileRotationStrategy fileRotator)
        {
            _exportDirectoryProvider = exportDirectoryProvider;
            _jsonFileNameProvider = jsonFileNameProvider;
            _fileRotator = fileRotator;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public void BeginVisit()
        {
            _timingReport = new TimingReport
            {
                MethodTimings = new List<TimingReport.MethodTiming>(),
            };
        }

        public void VisitTrace(Trace trace)
        {
            var timing = new TimingReport.MethodTiming
            {
                MethodName = trace.MethodName,
                ExecutionTime = trace.ExecutionTime,
            };
            _timingReport.MethodTimings.Add(timing);
        }

        public void Complete()
        {
            var json = JsonSerializer.Serialize(_timingReport, _jsonSerializerOptions);
            var directory = _exportDirectoryProvider.GetExportDirectory();
            FileSystemLock.Wait();
            try
            {
                var fileName = _fileRotator.RotateName(
                    directory,
                    _jsonFileNameProvider.GetJsonFileName());
                var path = Path.Combine(
                    directory,
                    fileName);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(path, json);
            }
            finally
            {
                FileSystemLock.Release();
            }
        }
    }
}
