using System.IO.Enumeration;

namespace Rw.Core.Helpers;

public static class InputHelpers
{
    private static readonly char[] WildcardCharacters = ['*', '?'];

    /// <summary>
    /// Splits comma-separated values, so "a.HEIC,b.HEIC" becomes two inputs.
    /// Blank entries are dropped.
    /// </summary>
    public static IEnumerable<string> NormaliseInputs(IEnumerable<string> sources) =>
        sources.SelectMany(source =>
            source.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    /// <summary>
    /// Replaces each wildcard input with the files in its folder that match it.
    /// Inputs without wildcards pass through unchanged, and so does a wildcard that matches nothing,
    /// so the validation step can report it.
    /// </summary>
    public static IEnumerable<string> ExpandWildcards(IEnumerable<string> inputs)
    {
        foreach (var input in inputs)
        {
            if (File.Exists(input) || input.IndexOfAny(WildcardCharacters) < 0)
            {
                yield return input;
                continue;
            }

            var folder = Path.GetDirectoryName(input);
            var directory = string.IsNullOrEmpty(folder)
                ? Directory.GetCurrentDirectory()
                : Path.GetFullPath(folder);
            var pattern = Path.GetFileName(input);

            var matched = false;
            if (Directory.Exists(directory))
            {
                foreach (var file in Directory.EnumerateFiles(directory))
                {
                    // Matching by hand keeps behaviour the same on Windows and Linux.
                    // Directory.EnumerateFiles with a pattern is case-sensitive on Linux
                    // and matches longer extensions on Windows.
                    if (FileSystemName.MatchesSimpleExpression(pattern, Path.GetFileName(file), ignoreCase: true))
                    {
                        matched = true;
                        yield return file;
                    }
                }
            }

            if (!matched)
            {
                yield return input;
            }
        }
    }
}
