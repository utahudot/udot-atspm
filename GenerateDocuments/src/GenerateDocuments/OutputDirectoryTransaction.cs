namespace AtspmDocsGenerator;

public static class OutputDirectoryTransaction
{
    public static void Run(string outputRoot, Action<string> generate)
    {
        var target = Path.GetFullPath(outputRoot);
        var parent = Path.GetDirectoryName(target)
            ?? throw new InvalidDataException($"Output directory has no parent: {target}");
        Directory.CreateDirectory(parent);

        var name = Path.GetFileName(target);
        var transactionId = Guid.NewGuid().ToString("N");
        var staging = Path.Combine(parent, $".{name}.staging-{transactionId}");
        var backup = Path.Combine(parent, $".{name}.backup-{transactionId}");
        var movedOriginal = false;

        Directory.CreateDirectory(staging);
        try
        {
            generate(staging);

            if (Directory.Exists(target))
            {
                Directory.Move(target, backup);
                movedOriginal = true;
            }

            try
            {
                Directory.Move(staging, target);
            }
            catch
            {
                if (movedOriginal && !Directory.Exists(target))
                {
                    Directory.Move(backup, target);
                    movedOriginal = false;
                }
                throw;
            }

            if (movedOriginal)
            {
                Directory.Delete(backup, recursive: true);
                movedOriginal = false;
            }
        }
        finally
        {
            if (Directory.Exists(staging))
            {
                Directory.Delete(staging, recursive: true);
            }

            if (movedOriginal && Directory.Exists(backup) && !Directory.Exists(target))
            {
                Directory.Move(backup, target);
            }
        }
    }
}
