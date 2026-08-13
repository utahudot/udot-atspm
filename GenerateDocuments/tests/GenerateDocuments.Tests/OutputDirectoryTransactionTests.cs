namespace AtspmDocsGenerator.Tests;

public sealed class OutputDirectoryTransactionTests
{
    [Fact]
    public void RunPreservesExistingOutputWhenGenerationFails()
    {
        using var directory = new TemporaryDirectory();
        var output = System.IO.Path.Combine(directory.Path, "output");
        Directory.CreateDirectory(output);
        File.WriteAllText(System.IO.Path.Combine(output, "existing.md"), "existing");

        Assert.Throws<InvalidDataException>(() =>
            OutputDirectoryTransaction.Run(output, staging =>
            {
                File.WriteAllText(System.IO.Path.Combine(staging, "partial.md"), "partial");
                throw new InvalidDataException("invalid");
            }));

        Assert.Equal("existing", File.ReadAllText(System.IO.Path.Combine(output, "existing.md")));
        Assert.False(File.Exists(System.IO.Path.Combine(output, "partial.md")));
    }

    [Fact]
    public void RunReplacesExistingOutputAfterSuccessfulGeneration()
    {
        using var directory = new TemporaryDirectory();
        var output = System.IO.Path.Combine(directory.Path, "output");
        Directory.CreateDirectory(output);
        File.WriteAllText(System.IO.Path.Combine(output, "stale.md"), "stale");

        OutputDirectoryTransaction.Run(
            output,
            staging => File.WriteAllText(System.IO.Path.Combine(staging, "current.md"), "current"));

        Assert.False(File.Exists(System.IO.Path.Combine(output, "stale.md")));
        Assert.Equal("current", File.ReadAllText(System.IO.Path.Combine(output, "current.md")));
    }
}
