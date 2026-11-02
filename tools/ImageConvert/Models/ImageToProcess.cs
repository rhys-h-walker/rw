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
                int tryCounter = 0;
                var finalPath = Path.Combine(TargetPath, FileName) + $".{OutputType}";
                while (Path.Exists(finalPath))
                {
                    finalPath = Path.Combine(TargetPath, FileName) + $"({tryCounter}).{OutputType}";
                    tryCounter++;
                }
                return finalPath;
            }
        }
    }
}
