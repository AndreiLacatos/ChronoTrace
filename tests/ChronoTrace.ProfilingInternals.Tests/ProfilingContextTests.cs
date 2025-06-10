using ChronoTrace.ProfilingInternals.DataExport;
using NSubstitute;

namespace ChronoTrace.ProfilingInternals.Tests;

public partial class ProfilingContextTests
{
    private readonly ITraceVisitor _mockVisitor;
    private readonly ProfilingContext _profilingContext;

    public ProfilingContextTests()
    {
        _mockVisitor = Substitute.For<ITraceVisitor>();
        _profilingContext = new ProfilingContext(_mockVisitor);
    }
}
