using ChronoTrace.ProfilingInternals.DataExport;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests.DataExport;

public class CallGraphBuilderTests
{
    private readonly CallGraphBuilder _builder = new CallGraphBuilder();

    [Fact]
    public void AssembleCallGraph_WithSimpleLinearTrace_ShouldBuildCorrectHierarchy()
    {
        // Arrange
        var traces = new List<Trace>
        {
            new Trace { MethodName = "A", ExecutionTime = TimeSpan.FromSeconds(10), Caller = null },
            new Trace { MethodName = "B", ExecutionTime = TimeSpan.FromSeconds(5), Caller = "A" },
            new Trace { MethodName = "C", ExecutionTime = TimeSpan.FromSeconds(1), Caller = "B" },
        };

        // Act
        var results = _builder.AssembleCallGraph(traces).ToList();

        // Assert
        results.ShouldNotBeNull();
        results.ShouldHaveSingleItem();
        var root = results.First().Root;
        AssertNode(root, "A", 1, TimeSpan.FromSeconds(10));

        var childB = root.Nodes.ShouldHaveSingleItem();
        AssertNode(childB, "B", 1, TimeSpan.FromSeconds(5));

        var childC = childB.Nodes.ShouldHaveSingleItem();
        AssertNode(childC, "C", 0, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AssembleCallGraph_WithMultipleBranches_ShouldPreserveChildOrder()
    {
        // Arrange
        var traces = new List<Trace>
        {
            new Trace { MethodName = "Main", ExecutionTime = TimeSpan.FromMilliseconds(100), Caller = null },
            new Trace { MethodName = "Init", ExecutionTime = TimeSpan.FromMilliseconds(10), Caller = "Main" },
            new Trace { MethodName = "Process", ExecutionTime = TimeSpan.FromMilliseconds(50), Caller = "Main" },
            new Trace { MethodName = "Cleanup", ExecutionTime = TimeSpan.FromMilliseconds(5), Caller = "Main" },
        };

        // Act
        var results = _builder.AssembleCallGraph(traces).ToList();

        // Assert
        results.ShouldNotBeNull();
        results.ShouldHaveSingleItem();
        var root = results.First().Root;
        AssertNode(root, "Main", 3, TimeSpan.FromMilliseconds(100));

        // Verifying order and node correctness for all children
        var initNode = root.Nodes.ElementAt(0);
        AssertNode(initNode, "Init", 0, TimeSpan.FromMilliseconds(10));

        var processNode = root.Nodes.ElementAt(1);
        AssertNode(processNode, "Process", 0, TimeSpan.FromMilliseconds(50));

        var cleanupNode = root.Nodes.ElementAt(2);
        AssertNode(cleanupNode, "Cleanup", 0, TimeSpan.FromMilliseconds(5));
    }

    [Fact]
    public void AssembleCallGraph_WithRecursiveCall_ShouldBuildCorrectRecursiveHierarchy()
    {
        // Arrange
        // Represents a call stack of Rec(n=2) -> Rec(n=1) -> Rec(n=0)
        var traces = new List<Trace>
        {
            new Trace { MethodName = "Rec", ExecutionTime = TimeSpan.FromMilliseconds(150), Caller = null },
            new Trace { MethodName = "Rec", ExecutionTime = TimeSpan.FromMilliseconds(100), Caller = "Rec" },
            new Trace { MethodName = "Rec", ExecutionTime = TimeSpan.FromMilliseconds(50), Caller = "Rec" },
        };

        // Act
        var results = _builder.AssembleCallGraph(traces).ToList();

        // Assert
        results.ShouldNotBeNull();
        results.ShouldHaveSingleItem();
        var root = results.First().Root;
        AssertNode(root, "Rec", 1, TimeSpan.FromMilliseconds(150));

        var child = root.Nodes.ShouldHaveSingleItem();
        AssertNode(child, "Rec", 1, TimeSpan.FromMilliseconds(100));

        var grandchild = child.Nodes.ShouldHaveSingleItem();
        AssertNode(grandchild, "Rec", 0, TimeSpan.FromMilliseconds(50)); // The leaf of the recursion
    }

    [Fact]
    public void AssembleCallGraph_WithMultipleRoots_ShouldReturnGraphForFirstRootOnly()
    {
        // Arrange
        var traces = new List<Trace>
        {
            new Trace { MethodName = "RootA", ExecutionTime = TimeSpan.FromSeconds(1), Caller = null },
            new Trace { MethodName = "ChildOfA", ExecutionTime = TimeSpan.FromMilliseconds(100), Caller = "RootA" },
            new Trace { MethodName = "RootB", ExecutionTime = TimeSpan.FromSeconds(2), Caller = null },
        };

        // Act
        var results = _builder.AssembleCallGraph(traces).ToList();

        // Assert
        results.ShouldNotBeNull();
        results.Count.ShouldBe(2);
        var root = results.First().Root;
        AssertNode(root, "RootA", 1, TimeSpan.FromSeconds(1));
        
        var child = root.Nodes.ShouldHaveSingleItem();
        AssertNode(child, "ChildOfA", 0, TimeSpan.FromMilliseconds(100));
        
        AssertNode(results[1].Root, "RootB", 0, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void AssembleCallGraph_WithComplexMixedRecursionAndBranching_ShouldBuildCorrectHierarchy()
    {
        // ARRANGE
        var traces = new List<Trace>
        {
            new Trace { MethodName = "Root", ExecutionTime = TimeSpan.FromMilliseconds(1000), Caller = null },
            new Trace { MethodName = "Alpha", ExecutionTime = TimeSpan.FromMilliseconds(500), Caller = "Root" },
            new Trace { MethodName = "Bravo", ExecutionTime = TimeSpan.FromMilliseconds(400), Caller = "Alpha" },
            new Trace { MethodName = "Delta", ExecutionTime = TimeSpan.FromMilliseconds(300), Caller = "Bravo" },
            new Trace { MethodName = "Echo", ExecutionTime = TimeSpan.FromMilliseconds(200), Caller = "Delta" },
            new Trace { MethodName = "Echo", ExecutionTime = TimeSpan.FromMilliseconds(150), Caller = "Echo" },
            new Trace { MethodName = "Echo", ExecutionTime = TimeSpan.FromMilliseconds(100), Caller = "Echo" },
            new Trace { MethodName = "Echo", ExecutionTime = TimeSpan.FromMilliseconds(50), Caller = "Echo" },
            new Trace { MethodName = "Charlie", ExecutionTime = TimeSpan.FromMilliseconds(20), Caller = "Alpha" },
            new Trace { MethodName = "Foxtrot", ExecutionTime = TimeSpan.FromMilliseconds(10), Caller = "Root" },
            new Trace { MethodName = "Echo", ExecutionTime = TimeSpan.FromMilliseconds(80), Caller = "Root" },
            new Trace { MethodName = "Echo", ExecutionTime = TimeSpan.FromMilliseconds(40), Caller = "Echo" },
        };

        // ACT
        var results = _builder.AssembleCallGraph(traces).ToList();

        // ASSERT
        results.ShouldNotBeNull();
        results.ShouldHaveSingleItem();
        var root = results.First().Root;
        AssertNode(root, "Root", 3, TimeSpan.FromMilliseconds(1000));

        // -- Branch 1: Alpha --
        var alphaNode = root.Nodes.ElementAt(0);
        AssertNode(alphaNode, "Alpha", 2, TimeSpan.FromMilliseconds(500));

        // -- Branch 1a: Bravo -> Delta -> Echo (deep recursion) --
        var bravoNode = alphaNode.Nodes.ElementAt(0);
        AssertNode(bravoNode, "Bravo", 1, TimeSpan.FromMilliseconds(400));
        
        var charlieNode = alphaNode.Nodes.ElementAt(1);
        AssertNode(charlieNode, "Charlie", 0, TimeSpan.FromMilliseconds(20));

        var deltaNode = bravoNode.Nodes.ShouldHaveSingleItem();
        AssertNode(deltaNode, "Delta", 1, TimeSpan.FromMilliseconds(300));
        
        // Verify the deep recursive Echo call (4 levels)
        var echo1Level1 = deltaNode.Nodes.ShouldHaveSingleItem();
        AssertNode(echo1Level1, "Echo", 1, TimeSpan.FromMilliseconds(200));

        var echo1Level2 = echo1Level1.Nodes.ShouldHaveSingleItem();
        AssertNode(echo1Level2, "Echo", 1, TimeSpan.FromMilliseconds(150));
        
        var echo1Level3 = echo1Level2.Nodes.ShouldHaveSingleItem();
        AssertNode(echo1Level3, "Echo", 1, TimeSpan.FromMilliseconds(100));

        var echo1Level4 = echo1Level3.Nodes.ShouldHaveSingleItem();
        AssertNode(echo1Level4, "Echo", 0, TimeSpan.FromMilliseconds(50));
        
        // -- Branch 2: Foxtrot --
        var foxtrotNode = root.Nodes.ElementAt(1);
        AssertNode(foxtrotNode, "Foxtrot", 0, TimeSpan.FromMilliseconds(10));
        
        // -- Branch 3: Echo (shallow recursion) --
        var echo2Level1 = root.Nodes.ElementAt(2);
        AssertNode(echo2Level1, "Echo", 1, TimeSpan.FromMilliseconds(80));

        var echo2Level2 = echo2Level1.Nodes.ShouldHaveSingleItem();
        AssertNode(echo2Level2, "Echo", 0, TimeSpan.FromMilliseconds(40));
    }
    
    [Fact]
    public void AssembleCallGraph_WithComplexMutualRecursion_ShouldBuildCorrectlyNestedHierarchy()
    {
        // ARRANGE
        // This trace list simulates a complex mutual recursion:
        // Root -> Alpha -> Bravo -> Charlie -> Alpha -> Bravo -> Charlie -> Delta
        // After that entire chain completes, Root -> Foxtrot.
        var traces = new List<Trace>
        {
            // The call stack as it would appear in the trace log (invocation order)
            new Trace { MethodName = "Root",      ExecutionTime = TimeSpan.FromMilliseconds(1000), Caller = null },
            new Trace { MethodName = "Alpha",     ExecutionTime = TimeSpan.FromMilliseconds(900),  Caller = "Root" },      // Call 1
            new Trace { MethodName = "Bravo",     ExecutionTime = TimeSpan.FromMilliseconds(800),  Caller = "Alpha" },     // Call 1
            new Trace { MethodName = "Charlie",   ExecutionTime = TimeSpan.FromMilliseconds(700),  Caller = "Bravo" },     // Call 1
            new Trace { MethodName = "Alpha",     ExecutionTime = TimeSpan.FromMilliseconds(600),  Caller = "Charlie" },   // Call 2 (recursive)
            new Trace { MethodName = "Bravo",     ExecutionTime = TimeSpan.FromMilliseconds(500),  Caller = "Alpha" },     // Call 2 (recursive)
            new Trace { MethodName = "Charlie",   ExecutionTime = TimeSpan.FromMilliseconds(400),  Caller = "Bravo" },     // Call 2 (recursive)
            new Trace { MethodName = "Delta",     ExecutionTime = TimeSpan.FromMilliseconds(300),  Caller = "Charlie" },   // Exit from recursion
            new Trace { MethodName = "Foxtrot",   ExecutionTime = TimeSpan.FromMilliseconds(200),  Caller = "Root" },       // Final call
        };

        // ACT
        var results = _builder.AssembleCallGraph(traces).ToList();

        // ASSERT
        // We will now traverse the entire expected graph, asserting every single link.
        // This proves the algorithm can untangle the twisted stack correctly.

        // Level 0: Root
        results.ShouldNotBeNull();
        results.ShouldHaveSingleItem();
        var root = results.First().Root;
        AssertNode(root, "Root", 2, TimeSpan.FromMilliseconds(1000)); // Has 2 children: Alpha, Foxtrot

        // Check children order
        root.Nodes.ElementAt(0).MethodName.ShouldBe("Alpha");
        root.Nodes.ElementAt(1).MethodName.ShouldBe("Foxtrot");

        // Level 1: First Alpha call
        var alpha1 = root.Nodes.ElementAt(0);
        AssertNode(alpha1, "Alpha", 1, TimeSpan.FromMilliseconds(900));

        // Level 2: First Bravo call
        var bravo1 = alpha1.Nodes.ShouldHaveSingleItem();
        AssertNode(bravo1, "Bravo", 1, TimeSpan.FromMilliseconds(800));

        // Level 3: First Charlie call
        var charlie1 = bravo1.Nodes.ShouldHaveSingleItem();
        AssertNode(charlie1, "Charlie", 1, TimeSpan.FromMilliseconds(700));

        // ---- The recursion begins ----

        // Level 4: Second Alpha call (called by Charlie)
        var alpha2 = charlie1.Nodes.ShouldHaveSingleItem();
        AssertNode(alpha2, "Alpha", 1, TimeSpan.FromMilliseconds(600));

        // Level 5: Second Bravo call
        var bravo2 = alpha2.Nodes.ShouldHaveSingleItem();
        AssertNode(bravo2, "Bravo", 1, TimeSpan.FromMilliseconds(500));

        // Level 6: Second Charlie call
        var charlie2 = bravo2.Nodes.ShouldHaveSingleItem();
        AssertNode(charlie2, "Charlie", 1, TimeSpan.FromMilliseconds(400));
        
        // ---- The recursion exits ----

        // Level 7: Delta, the leaf of the recursive chain
        var delta = charlie2.Nodes.ShouldHaveSingleItem();
        AssertNode(delta, "Delta", 0, TimeSpan.FromMilliseconds(300)); // Leaf node

        // ---- The final branch from Root ----
        
        // Check the Foxtrot leaf node
        var foxtrot = root.Nodes.ElementAt(1);
        AssertNode(foxtrot, "Foxtrot", 0, TimeSpan.FromMilliseconds(200)); // Leaf node
    }
    
    /// <summary>
    /// Helper method to avoid repetitive assertions on a CallGraph.Node.
    /// </summary>
    private static void AssertNode(
        CallGraph.Node node,
        string expectedName,
        int expectedChildrenCount,
        TimeSpan expectedExecutionTime)
    {
        node.ShouldNotBeNull();
        node.MethodName.ShouldBe(expectedName);
        node.ExecutionTime.ShouldBe(expectedExecutionTime);
        node.Nodes.ShouldNotBeNull();
        node.Nodes.Count().ShouldBe(expectedChildrenCount);
    }
}