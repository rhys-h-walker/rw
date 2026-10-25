using System;
using System.Collections.Generic;
using System.Text;

namespace GuidMaker.Models
{
    public class GuidMakerOptionValues
    {
        public int GuidCount { get; set; }
        public string Beginning { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public int Preset { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool Upper { get; set; }
        public bool ListPreset { get; set; }
    }
}
