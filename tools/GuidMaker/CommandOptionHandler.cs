using GuidMaker.Models;
using GuidMaker.Providors;
using Rw.Core.StaticValues;
using Rw.Core.Extensions;
using System.CommandLine;
using System.Globalization;
using Rw.Core.Interfaces;
using Rw.Core.Generators;

namespace GuidMaker
{
    public class CommandOptionHandler
    {
        public static int HandleCommandOptions(GuidMakerOptionValues opts, ParseResult parseResult, IGuidGenerator guidGenerator)
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

            Func<string> upperCaseGuidGenerator = () => { return guidGenerator.NewGuid().ToString().ToUpperInvariant(); };
            Func<string> lowerCaseGuidGenerator = () => { return guidGenerator.NewGuid().ToString().ToLowerInvariant(); };

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
                output = output.Replace(PresetsProvidor.TypeReplaceableString, opts.Type);
            }

            parseResult.InvocationConfiguration.Output.WriteLine(output);
            return ExitCodes.Success;
        }
    }
}
