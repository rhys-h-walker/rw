using Rw.Core.Extensions;
using Rw.Core.Interfaces;
using ImageConvert.Providors;
using System.CommandLine;
using ImageMagick;
using Rw.Core.Helpers;
using ImageConvert.Models;

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

                // Get the output format specified
                if (!Enum.TryParse<MagickFormat>(format, ignoreCase: true, out var magickFormat))
                {
                    error.WriteLine($"Unknown format: {format}");
                    return;
                }

                // This will give us a set of expanded inputs on wildcard
                // It will also return us just some folders still
                var expandedInputs = InputHelpers.ExpandWildcards(InputHelpers.NormaliseInputs(sources));

                var imagesToProcess = new List<ImageToProcess>();

                /*
                 * 2 modes:
                 * Check if each input is a folder or a file
                 * 
                 * If file:
                 *      - Check it is supported format
                 *      - Convert to a Path obejct and save
                 *      
                 */
                foreach (var input in expandedInputs)
                {
                    if (File.Exists(input))
                    {
                        // We have a file and can check if it is a supported format
                        imagesToProcess.Add(new ImageToProcess
                        {
                            SourcePath = input,
                            TargetPath = Path.GetFullPath(outputDirectory),
                            OutputType = format,
                        });
                    }
                    else if (Directory.Exists(input))
                    {
                        // We have a directory and we can find all image files in this directory
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

                Directory.CreateDirectory(outputDirectory);

                var failures = new List<string>();
                int success = 0;
                foreach (var image in imagesToProcess)
                {
                    try
                    {
                        using var mimage = new MagickImage(image.SourcePath);
                        mimage.Write(image.FinalOutputPath, magickFormat);
                        success++;
                    }
                    catch (Exception ex) when (ex is MagickException or IOException)
                    {
                        error.WriteLine($"Error: {image.SourcePath}: {ex.Message}");
                        failures.Add(image.SourcePath);
                    }
                }

                output.WriteLine($"Successfully converted {success} images");
                error.WriteLine($"Failed to convert {failures.Count}");
            });

            return command;
        }

        private static bool FileIsSupportedFormat(string filepath)
        {
            var supportedFormats = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".heic"
            };
            var fileExtension = Path.GetExtension(filepath);
            return supportedFormats.Contains(fileExtension);
        }
    }
}
