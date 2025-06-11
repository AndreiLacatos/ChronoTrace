using ChronoTrace.ProfilingInternals.Settings;
using Shouldly;

namespace ChronoTrace.ProfilingInternals.Tests;

public class ProfilingSettingsProviderTests
{
    /// <summary>
    /// Tests that GetSettings returns the default settings initially.
    /// </summary>
    [Fact]
    public void GetSettings_InitialState_ShouldReturnDefaultSettings()
    {
        // Act
        var settings = ProfilingSettingsProvider.GetSettings();
        
        // Assert
        settings.ShouldNotBeNull();
        settings.ShouldBeEquivalentTo(ProfilingSettings.Default);
    }

    /// <summary>
    /// Tests that UpdateSettings correctly changes the internal settings.
    /// </summary>
    [Fact]
    public void UpdateSettings_WithNewSettings_ShouldUpdateInternalSettings()
    {
        // Arrange
        var originalSettings = ProfilingSettingsProvider.GetSettings();
        var newSettings = new ProfilingSettings
        {
            OutputPath = "CustomOutputPath",
        };
        
        // Act
        ProfilingSettingsProvider.UpdateSettings(newSettings);
        
        // Assert
        var updatedSettings = ProfilingSettingsProvider.GetSettings();
        updatedSettings.ShouldBe(newSettings);
        updatedSettings.ShouldNotBe(originalSettings);
    }

    /// <summary>
    /// Tests that GetSettings returns the same reference until UpdateSettings is called.
    /// </summary>
    [Fact]
    public void GetSettings_MultipleCallsWithoutUpdate_ShouldReturnSameReference()
    {
        // Act
        var settings1 = ProfilingSettingsProvider.GetSettings();
        var settings2 = ProfilingSettingsProvider.GetSettings();
        var settings3 = ProfilingSettingsProvider.GetSettings();
        
        // Assert
        settings1.ShouldBe(settings2);
        settings1.ShouldBe(settings3);
        settings2.ShouldBe(settings3);
    }

    /// <summary>
    /// Tests that UpdateSettings preserves the exact settings object reference.
    /// </summary>
    [Fact]
    public void UpdateSettings_WithSpecificInstance_ShouldPreserveExactReference()
    {
        // Arrange
        var customSettings = new ProfilingSettings
        {
            OutputPath = "CustomPath", 
        };
       
        // Act
        ProfilingSettingsProvider.UpdateSettings(customSettings);
        
        // Assert
        var retrievedSettings = ProfilingSettingsProvider.GetSettings();
        retrievedSettings.ShouldBe(customSettings);
    }

    /// <summary>
    /// Tests the thread safety of GetSettings under concurrent access.
    /// </summary>
    [Fact]
    public async Task GetSettings_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        const int operationsPerThread = 100;
        var results = new ProfilingSettings[threadCount * operationsPerThread];
        var tasks = new Task[threadCount];
        
        // Act
        for (var i = 0; i < threadCount; i++)
        {
            var threadIndex = i;
            tasks[i] = Task.Run(() =>
            {
                for (var j = 0; j < operationsPerThread; j++)
                {
                    results[threadIndex * operationsPerThread + j] = ProfilingSettingsProvider.GetSettings();
                }
            });
        }
        
        await Task.WhenAll(tasks);
        
        // Assert - All results should be the same reference (thread-safe read)
        var firstResult = results[0];
        for (var i = 1; i < results.Length; i++)
        {
            results[i].ShouldBe(firstResult);
        }
    }
}