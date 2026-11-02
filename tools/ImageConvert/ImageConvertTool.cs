using Rw.Core.Extensions;
using Rw.Core.Interfaces;
using ImageConvert.Providors;
using System.CommandLine;
using ImageMagick;
using Rw.Core.Helpers;
using ImageConvert.Models;
using Rw.Core.StaticValues;
using static ImageConvert.Helpers.FormatHelpers;

namespace ImageConvert
{
    public sealed class ImageConvertTool : IRwTool
    {
        public Command CreateCommand()
        {
            var command = new Command("image-convert", "Convert images between formats");
            command.Options.AddRange(OptionsProvider.GetOptions());

            command.SetAction(parseResult =>
            {
                var sources = parseResult.GetValue(OptionsProvider.sourceOption) ?? Array.Empty<string>();
                var format = parseResult.GetValue(OptionsProvider.formatOption) ?? string.Empty;
                var outputDirectory = parseResult.GetValue(OptionsProvider.outputOption) ?? "./";
                var output = parseResult.InvocationConfiguration.Output;
                var error = parseResult.InvocationConfiguration.Error;

                if (FormatIsUnsupportedForWrites(format))
                {
                    error.WriteLine($"The format {format} is unsupported for writes");
                    return ExitCodes.Failure;
                }

                // Get the output format specified
                if (!Enum.TryParse<MagickFormat>(format, ignoreCase: true, out var magickFormat))
                {
                    error.WriteLine($"Unknown format: {format}");
                    return ExitCodes.Failure;
                }

                // This will give us a set of expanded inputs on wildcard
                // It will also return us just some folders
                var expandedInputs = InputHelpers.ExpandWildcards(InputHelpers.NormaliseInputs(sources));
                var imagesToProcess = CreateImageToProcessObjects(expandedInputs, format, outputDirectory);
                Directory.CreateDirectory(outputDirectory);
                var failures = WriteNewImageFiles(imagesToProcess, magickFormat, error);

                // Output results
                output.WriteLine($"Successfully converted {imagesToProcess.Count-failures.Count} images");
                error.WriteLine($"Failed to convert {failures.Count}");

                return (failures.Count > 0) ? ExitCodes.Failure : ExitCodes.Success;
            });

            return command;
        }

        private static List<string> WriteNewImageFiles(List<ImageToProcess> imagesToProcess, MagickFormat magickFormat, TextWriter error)
        {
            var failures = new List<string>();
            foreach (var image in imagesToProcess)
            {
                try
                {
                    using var mimage = new MagickImage(image.SourcePath);
                    mimage.Write(image.FinalOutputPath, magickFormat);
                }
                catch (Exception ex) when (ex is MagickException or IOException)
                {
                    error.WriteLine($"Error: {image.SourcePath}: {ex.Message}");
                    failures.Add(image.SourcePath);
                }
            }

            return failures;
        }

        private static List<ImageToProcess> CreateImageToProcessObjects(IEnumerable<string> expandedInputs, string format, string outputDirectory)
        {
            var imagesToProcess = new List<ImageToProcess>();

            // Create the ImageToProcess objects for each folder
            foreach (var input in expandedInputs)
            {
                if (File.Exists(input) && FileIsSupportedFormat(input))
                {
                    imagesToProcess.Add(new ImageToProcess
                    {
                        SourcePath = input,
                        TargetPath = Path.GetFullPath(outputDirectory),
                        OutputType = format,
                    });
                }
                else if (Directory.Exists(input))
                {
                    foreach (var file in Directory.EnumerateFiles(input))
                    {
                        if (FileIsSupportedFormat(file))
                        {
                            imagesToProcess.Add(new ImageToProcess
                            {
                                SourcePath = file,
                                TargetPath = Path.GetFullPath(outputDirectory),
                                OutputType = format,
                            });
                        }
                    }
                }
            }

            return imagesToProcess;
        }
    }
}
