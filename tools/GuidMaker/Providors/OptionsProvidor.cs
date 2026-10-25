using Rw.Core.Extensions;
using System.CommandLine;
using System.Text;

namespace GuidMaker.Providors
{
    public static class OptionsProvidor
    {
        public static readonly Option<int> guidCountOption = new Option<int>("-n", "-count")
        {
            Description = "How many guids to generate",
            DefaultValueFactory = _ => 1,
        };

        public static readonly Option<string> beginingTextOption = new Option<string>("-b", "--begin")
        {
            Description = "The text to place before the Guid",
            DefaultValueFactory = _ => string.Empty,
        };

        public static readonly Option<string> endingTextOption = new Option<string>("-e", "--end")
        {
            Description = "The text to place after the Guid",
            DefaultValueFactory = _ => string.Empty
        };

        public static readonly Option<int> presetOption = new Option<int>("-p", "--preset")
        {
            Description = "The preset to choose for the -b & -e tags, 0 means no preset",
            DefaultValueFactory = _ => 0,
        };

        public static readonly Option<string> typeOption = new Option<string>("-t", "--type")
        {
            Description = "Option to replace <ObjectName> if one is created by a preset",
        };

        public static readonly Option<bool> upperOption = new Option<bool>("--upper")
        {
            Description = "Upper case Guids?"
        };

        public static readonly Option<bool> listPresetOptions = new Option<bool>("--list-presets")
        {
            Description = "List all of the presets",
        };

        public static IList<Option> GetOptions()
        {
            var options = new List<Option>();
            options.AddRange(guidCountOption, 
                            beginingTextOption, 
                            endingTextOption, 
                            presetOption, 
                            typeOption, 
                            upperOption, 
                            listPresetOptions);
            return options;
        }
    }
}
