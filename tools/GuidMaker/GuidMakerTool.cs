using System.CommandLine;
using Rw.Core.Extensions;
using Rw.Core.Interfaces;
using GuidMaker.Providors;
using GuidMaker.Models;
using Rw.Core.Generators;

namespace GuidMaker
{
    public sealed class GuidMakerTool : IRwTool
    {
        private readonly IGuidGenerator _guidGenerator;

        public GuidMakerTool(IGuidGenerator guidGenerator)
        {
            _guidGenerator = guidGenerator;
        }

        public Command CreateCommand()
        {
            var command = new Command("guid-maker", "Generates some guids ready to copy into a project");
            command.Options.AddRange(OptionsProvidor.GetOptions());

            command.SetAction(parseResult =>
            {
                var optionValues = new GuidMakerOptionValues
                {
                    GuidCount = parseResult.GetValue(OptionsProvidor.guidCountOption),
                    Beginning = parseResult.GetValue(OptionsProvidor.beginingTextOption) ?? string.Empty,
                    End = parseResult.GetValue(OptionsProvidor.endingTextOption) ?? string.Empty,
                    Preset = parseResult.GetValue(OptionsProvidor.presetOption),
                    Type = parseResult.GetValue(OptionsProvidor.typeOption) ?? string.Empty,
                    Upper = parseResult.GetValue(OptionsProvidor.upperOption),
                    ListPreset = parseResult.GetValue(OptionsProvidor.listPresetOptions),
                };
                
                return CommandOptionHandler.HandleCommandOptions(optionValues, parseResult, _guidGenerator);
            });

            return command;
        }
    }
}
