
namespace ImageConvert.Helpers
{
    public static class FormatHelpers
    {
        internal static readonly HashSet<string> supportedOptions = new HashSet<string>
        {
            ".jpg", ".jpeg", ".png", ".heic"
        };

        /*
         * heic: Unsupported due to copyright
         */
        internal static readonly HashSet<string> unsupportedOptions = new HashSet<string>
        {
            "heic"
        };

        internal static bool FileIsSupportedFormat(string filepath)
        {
            var fileExtension = Path.GetExtension(filepath);
            return supportedOptions.Contains(fileExtension.ToLowerInvariant());
        }

        internal static bool FormatIsUnsupportedForWrites(string format)
        {
            return unsupportedOptions.Contains(format.ToLowerInvariant());
        }

        internal static void DisplaySupportedUnsupportedOptions(TextWriter output)
        {
            var supported = $"SUPPORTED OPTIONS\n-----------------\n{String.Join("\n", supportedOptions.ToList())}";
            var unsupported = $"UNSUPPORTED TARGET OPTIONS\n--------------------------\n{String.Join("\n", unsupportedOptions.ToList())}";
            output.WriteLine($"{supported}\n\n{unsupported}");
        }
    }
}
