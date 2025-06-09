using ChronoTrace.SourceGenerators.DataStructures;

namespace ChronoTrace.SourceGenerators.Tests;

public class MethodTypeAnalysisTests
{
    
    [Theory]
    [InlineData(MethodType.SyncVoid, true, false)]
    [InlineData(MethodType.SimpleAsync, true, true)]
    [InlineData(MethodType.SyncNonVoid, false, false)]
    [InlineData(MethodType.GenericAsync, false, true)]
    internal void MethodTypeExtensionMethods_ShouldReturnCorrectFlags(MethodType methodType, bool isVoidType, bool isAsync)
    {
        Assert.Equal(isVoidType, methodType.IsVoidType());
        Assert.Equal(isAsync, methodType.IsAsync());
    }
}
