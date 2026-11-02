using System;
using System.Collections.Generic;
using System.Text;

namespace ImageConvert.Helpers
{
    public static class FormatHelpers
    {
        internal static bool FileIsSupportedFormat(string filepath)
        {
            var supportedFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".heic"
            };
            var fileExtension = Path.GetExtension(filepath);
            return supportedFormats.Contains(fileExtension);
        }

        internal static bool FormatIsUnsupportedForWrites(string format)
        {
            /*
             * heic: Unsupported due to copyright
             */
            var unsupportedFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "heic"
            };
            return unsupportedFormats.Contains(format.ToLowerInvariant());
        }
    }
}
