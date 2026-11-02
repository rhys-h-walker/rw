using ImageMagick;

namespace ImageConvert.Models
{
    public class ImageToProcess
    {
        public string SourcePath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public string OutputType { get; set; } = string.Empty;
        public string FileName
        { 
            get
            {
                return Path.GetFileNameWithoutExtension(SourcePath);
            }
        } 
        public string FinalOutputPath
        {
            get
            {
                return Path.Combine(TargetPath, FileName) + $".{OutputType}";
            }
        }
    }
}
