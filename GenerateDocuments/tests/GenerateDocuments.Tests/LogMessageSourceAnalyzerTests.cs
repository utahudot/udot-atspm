namespace AtspmDocsGenerator.Tests;

public sealed class LogMessageSourceAnalyzerTests
{
    [Fact]
    public void AnalyzeReadsLoggerMessagesAndSortsByEventId()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteFile(
            "Logs/SampleLogMessages.cs",
            """
            public partial class SampleLogMessages
            {
                /// <summary>Second message with a <paramref name="value"/>.</summary>
                [LoggerMessage(EventId = 20, EventName = "Second", Level = LogLevel.Warning, Message = "{value}")]
                public partial void Second(string value);

                /// <summary>
                /// First message.
                /// </summary>
                [LoggerMessage(EventId = 10, EventName = "First", Level = LogLevel.Information, Message = "First")]
                public partial void First();
            }
            """);

        var messages = new LogMessageSourceAnalyzer().Analyze(directory.Path, "Logs");

        Assert.Collection(
            messages,
            first =>
            {
                Assert.Equal(10, first.EventId);
                Assert.Equal("First", first.EventName);
                Assert.Equal("Information", first.Level);
                Assert.Equal("First message.", first.Summary);
            },
            second =>
            {
                Assert.Equal(20, second.EventId);
                Assert.Equal("Second", second.EventName);
                Assert.Equal("Warning", second.Level);
                Assert.Equal("Second message with a value.", second.Summary);
            });
    }

    [Fact]
    public void AnalyzeRejectsSourcePathsOutsideTheRepository()
    {
        using var directory = new TemporaryDirectory();

        var exception = Assert.Throws<InvalidDataException>(
            () => new LogMessageSourceAnalyzer().Analyze(directory.Path, ".."));

        Assert.Contains("inside the source root", exception.Message);
    }

    [Theory]
    [InlineData(42, false)]
    [InlineData(201, true)]
    public void AnalyzeOnlyAllowsExplicitDuplicateEventIds(int eventId, bool isAllowed)
    {
        using var directory = new TemporaryDirectory();
        directory.WriteFile(
            "Logs/Duplicates.cs",
            $$"""
            public partial class DuplicateLogMessages
            {
                [LoggerMessage(EventId = {{eventId}}, EventName = "First", Level = LogLevel.Information, Message = "First")]
                public partial void First();

                [LoggerMessage(EventId = {{eventId}}, EventName = "Second", Level = LogLevel.Information, Message = "Second")]
                public partial void Second();
            }
            """);

        if (isAllowed)
        {
            Assert.Equal(2, new LogMessageSourceAnalyzer().Analyze(directory.Path, "Logs").Count);
        }
        else
        {
            var exception = Assert.Throws<InvalidDataException>(
                () => new LogMessageSourceAnalyzer().Analyze(directory.Path, "Logs"));
            Assert.Contains(eventId.ToString(), exception.Message);
        }
    }
}
