﻿//HintName: S_Do_ProfilingInterceptors.g.cs
// <auto-generated>
//
// ╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
// ║                                                                                                            ║
// ║    This file was auto-generated by ChronoTrace.SourceGenerators                                            ║
// ║                                                                                                            ║
// ║    This file should not be edited! Manual changes may cause incorrect behavior and will be overwritten.    ║
// ║                                                                                                            ║
// ╠════════════════════════════════════════════════════════════════════════════════════════════════════════════╣
// ║    Generated: 2020-04-29 13:17:19 UTC                                      ChronoTrace version: Testing    ║
// ╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
//
// </auto-generated>


using System.Runtime.CompilerServices;

namespace ProfilingInterceptors;
public static class SProfilingInterceptorExtensions
{
    [global::System.Runtime.CompilerServices.InterceptsLocationAttribute(version: 1, data: "TESTING")]
    public static global::System.Int32 InterceptDo(this global::S __ChronoTrace_Subject)
    {
        var __ChronoTrace_Profiling_Context = global::ChronoTrace.ProfilingInternals.ProfilingContextAccessor.Current;
        var __ChronoTrace_Method_Invocation_Id = __ChronoTrace_Profiling_Context.BeginMethodProfiling("Do");
        try
        {
            return __ChronoTrace_Subject.Do();
        }
        finally
        {
            __ChronoTrace_Profiling_Context.EndMethodProfiling(__ChronoTrace_Method_Invocation_Id);
            __ChronoTrace_Profiling_Context.CollectTraces();
        }
    }
}