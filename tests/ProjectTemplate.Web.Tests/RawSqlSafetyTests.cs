namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Provides static safety checks for raw SQL and manual command construction patterns.
/// </summary>
public sealed class RawSqlSafetyTests
{
    private static readonly string[] _unsafeRawSqlPatterns =
    [
        "FromSqlRaw",
        "ExecuteSqlRaw",
        "SqlQueryRaw",
        "CommandText",
        "CreateCommand",
        "new SqlCommand"
    ];

    [Fact]
    public void SourceFiles_DoNotUseUnsafeRawSqlOrManualCommandConstructionPatterns()
    {
        string solutionRoot = GetSolutionRoot();

        string[] searchRoots =
        [
            Path.Combine(solutionRoot, "src"),
            Path.Combine(solutionRoot, ".template.content", "src")
        ];

        List<string> violations = [];

        foreach (string searchRoot in searchRoots.Where(Directory.Exists))
        {
            foreach (string filePath in Directory.EnumerateFiles(searchRoot, "*.cs", SearchOption.AllDirectories))
            {
                string source = File.ReadAllText(filePath);

                foreach (string pattern in _unsafeRawSqlPatterns)
                {
                    if (source.Contains(pattern, StringComparison.Ordinal))
                    {
                        violations.Add($"{Path.GetRelativePath(solutionRoot, filePath)} contains '{pattern}'.");
                    }
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Unsafe raw SQL or manual command construction patterns were found. " +
            "Prefer LINQ, FromSqlInterpolated, ExecuteSqlInterpolated, or explicit DbParameter usage for dynamic values." +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    private static string GetSolutionRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.EnumerateFiles(directory.FullName, "*.slnx").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate solution root from '{AppContext.BaseDirectory}'.");
    }
}
