using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R34Downloader
{
    public class InputSettings
    {
        public bool RemoveOnCompletion;
        public bool IsNumber;
        public bool CanBeEmpty;
        public string EmptyMessage = "No input given";
        public string InvalidNumberMessage = "Invalid number";
        public string[] SubtitleLines = new string[0];
        public string CustomInputLine;
    }
}
