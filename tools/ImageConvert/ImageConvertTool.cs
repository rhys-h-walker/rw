using Rw.Core.Extensions;
using Rw.Core.Interfaces;
using ImageConvert.Providors;
using System.CommandLine;
using ImageMagick;
using Rw.Core.Helpers;
using ImageConvert.Models;
using Rw.Core.StaticValues;
using static ImageConvert.Helpers.FormatHelpers;
using System.Reflection.Metadata.Ecma335;

namespace ImageConvert
{
    public sealed class ImageConvertTool : IRwTool
    {
        private TextWriter? Error;
        private TextWriter? Output;
        private List<string> Failures = new List<string>();

        public Command CreateCommand()
        {
            var command = new Command("image-convert", "Convert images between formats");
            command.Options.AddRange(OptionsProvider.GetOptions());

            command.SetAction(parseResult =>
            {
                var sources = parseResult.GetValue(OptionsProvider.sourceOption) ?? Array.Empty<string>();
                var format = parseResult.GetValue(OptionsProvider.formatOption) ?? string.Empty;
                var outputDirectory = parseResult.GetValue(OptionsProvider.outputOption) ?? "./";
                var displaySupported = parseResult.GetValue(OptionsProvider.supportedFormatsOption);
                Output = parseResult.InvocationConfiguration.Output;
                Error = parseResult.InvocationConfiguration.Error;

                if (displaySupported)
                {
                    DisplaySupportedUnsupportedOptions(Output);
                    return ExitCodes.Success;
                }

                if (string.IsNullOrWhiteSpace(format))
                {
                    Error.WriteLine("Format was empty exiting");
                    return ExitCodes.Failure;
                }

                if (FormatIsUnsupportedForWrites(format))
                {
                    Error.WriteLine($"The format {format} is unsupported for writes");
                    return ExitCodes.Failure;
                }

                if (!Enum.TryParse<MagickFormat>(format, ignoreCase: true, out var magickFormat))
                {
                    Error.WriteLine($"Unknown format: {format}");
                    return ExitCodes.Failure;
                }

                // This will give us a set of expanded inputs on wildcard
                // It will also return us just some folders
                var expandedInputs = InputHelpers.ExpandWildcards(InputHelpers.NormaliseInputs(sources));
                var imagesToProcess = CreateImageToProcessObjects(expandedInputs, format, outputDirectory);

                if (imagesToProcess.Count == 0)
                {
                    Output.WriteLine("Nothing to convert");
                    return ExitCodes.Success;
                }

                Directory.CreateDirectory(outputDirectory);
                Failures = WriteNewImageFiles(imagesToProcess, magickFormat);

                // Output results
                Output.WriteLine($"Successfully converted {imagesToProcess.Count-Failures.Count} images");
                if (Failures.Count > 0) Error.WriteLine($"Failed to convert {Failures.Count}");

                return (Failures.Count > 0) ? ExitCodes.Failure : ExitCodes.Success;
            });

            return command;
        }

        private List<string> WriteNewImageFiles(List<ImageToProcess> imagesToProcess, MagickFormat magickFormat)
        {
            foreach (var image in imagesToProcess)
            {
                try
                {
                    using var mimage = new MagickImage(image.SourcePath);
                    mimage.Write(image.FinalOutputPath, magickFormat);
                }
                catch (Exception ex) when (ex is MagickException or IOException)
                {
                    if (Error == null) continue;
                    Error.WriteLine($"Error: {image.SourcePath}: {ex.Message}");
                    Failures.Add(image.SourcePath);
                }
            }

            return Failures;
        }

        private List<ImageToProcess> CreateImageToProcessObjects(IEnumerable<string> expandedInputs, string format, string outputDirectory)
        {
            var imagesToProcess = new List<ImageToProcess>();

            // Create the ImageToProcess objects for each folder
            foreach (var input in expandedInputs)
            {
                if (File.Exists(input))
                {
                    if (FileIsSupportedFormat(input) == false)
                    {
                        if (Error == null) continue;
                        Error.WriteLine($"File: {input} is unsupported");
                        Failures.Add(input);
                        continue;
                    }

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
