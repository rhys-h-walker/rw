using System.Globalization;
using System.Text;

namespace GuidMaker.Providors
{
    public static class PresetsProvidor
    {
        public static readonly string TypeReplaceableString = "<ObjectName>";
        private static readonly (string Beginning, string End) Preset1 = 
        (
            Beginning: $"new {TypeReplaceableString} {{ Id = Guid.Parse(\"",
            End: "\"), Name = \"\" },"
        );

        public static (string Beginning, string End)? GetPreset(int preset)
        {
            switch (preset)
            {
                case 1:
                    return Preset1;
                default:
                    return null;
            }
        }

        public static string ListPresets()
        {
            var presets = new List<(string Beginning, string End)> { Preset1 };

            return string.Join(
                Environment.NewLine,
                presets.Select((p, i) => string.Create(
                    CultureInfo.InvariantCulture,
                    $"Preset {i + 1} start: {p.Beginning} | end: {p.End}")));
        }
    }
}
