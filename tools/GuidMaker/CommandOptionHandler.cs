using GuidMaker.Models;
using GuidMaker.Providors;
using Rw.Core.StaticValues;
using Rw.Core.Extensions;
using System.CommandLine;
using System.Globalization;

namespace GuidMaker
{
    public static class CommandOptionHandler
    {
        private static readonly Func<string> lowerCaseGuidGenerator = () =>
        {
            return Guid.NewGuid().ToString().ToLowerInvariant();
        };

        private static readonly Func<string> upperCaseGuidGenerator = () =>
        {
            return Guid.NewGuid().ToString().ToUpperInvariant();
        };

        public static int HandleCommandOptions(GuidMakerOptionValues opts, ParseResult parseResult)
        {
            // Early escape for ListPreset
            if (opts.ListPreset)
            {
                parseResult.InvocationConfiguration.Output.WriteLine(PresetsProvidor.ListPresets());
                return ExitCodes.Success;
            }

            // Apply the guards
            if (opts.GuidCount <= 0)
            {
                parseResult.InvocationConfiguration.Error.WriteLine("Cannot generate 0 or less Guids");
                return ExitCodes.InvalidInput;
            }

            var guids = opts.GuidCount.For((opts.Upper ? upperCaseGuidGenerator : lowerCaseGuidGenerator));

            // Get our presets and assign over Beginning and End
            if (opts.Preset != 0)
            {
                var preset = PresetsProvidor.GetPreset(opts.Preset);
                if (preset == null)
                {
                    parseResult.InvocationConfiguration.Error.WriteLine("Could not find the selected preset");
                    return ExitCodes.InvalidInput;
                }

                opts.Beginning = preset.Value.Beginning;
                opts.End = preset.Value.End;
            }

            // Now format the Guids and return
            var output = string.Join(
                Environment.NewLine,
                guids.Select((guid) => string.Create(
                    CultureInfo.InvariantCulture,
                    $"{opts.Beginning}{guid}{opts.End}"
                    ))
                );

            // Attempt to apply type if it is present
            if (string.IsNullOrEmpty(opts.Type) == false)
            {
                output = output.Replace(PresetsProvidor.TypeReplaceableString, output);
            }

            parseResult.InvocationConfiguration.Output.WriteLine(output);
            return ExitCodes.Success;
        }
    }
}
