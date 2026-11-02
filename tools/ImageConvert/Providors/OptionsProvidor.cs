using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace ImageConvert.Providors
{
    public static class OptionsProvider
    {
        public static readonly Option<string[]> sourceOption = new Option<string[]>("-s", "--sources")
        {
            Description = "The source file, files or directory to convert from",
            Required = true,
            AllowMultipleArgumentsPerToken = true
        };

        public static readonly Option<string> formatOption = new Option<string>("-f", "--format")
        {
            Description = "The format to convert the images to, no default available",
            Required = true
        };

        public static readonly Option<string> outputOption = new Option<string>("-o", "--output")
        {
            Description = "The output directory for the converted images, default is current directory",
            DefaultValueFactory = _ => "./"
        };

        public static readonly Option<string[]> nameOption = new Option<string[]>("-n", "--names")
        {
            Description = "The names of the images to convert, default is the same as source name",
            AllowMultipleArgumentsPerToken = true
        };

        public static IList<Option> GetOptions()
        {
            var options = new List<Option>();
            options.AddRange(sourceOption, formatOption, outputOption);
            return options;
        }
    }
}
