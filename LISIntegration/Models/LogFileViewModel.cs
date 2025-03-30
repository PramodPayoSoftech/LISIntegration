using System;

namespace LISIntegration.Models
{
    public class LogFileViewModel
    {
        public string FileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FilePath { get; set; }
        public long SizeInBytes { get; set; }

        public string FormattedSize
        {
            get
            {
                if (SizeInBytes < 1024)
                    return $"{SizeInBytes} B";
                else if (SizeInBytes < 1024 * 1024)
                    return $"{SizeInBytes / 1024.0:F2} KB";
                else
                    return $"{SizeInBytes / (1024.0 * 1024.0):F2} MB";
            }
        }
    }
}