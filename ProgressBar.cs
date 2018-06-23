using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace R34Downloader
{
    public class ProgressBar : IProgress<float>
    {
        public float Current, Total;
        public bool Completed { get; private set; }
        public string Message;
        public string Header;
        public ProgressBarOptions Options;
        public int OverriddenConsoleLine = -1;
        private int consoleLine;
        private int numberOfBars;
        private string progressBarChars;
        private bool showing;
        private Stopwatch stopwatch;

        public ProgressBar(string message, float totalValue, ProgressBarOptions options)
        {
            Options = new ProgressBarOptions(options);
            Total = totalValue;
            Message = message;
            Options = options;
            Startup();
        }

        public ProgressBar(ProgressBar bar)
        {
            Options = new ProgressBarOptions(bar.Options);
            Total = bar.Total;
            Message = bar.Message;
            Options = bar.Options;
            Startup();
        }

        private void Startup()
        {
            for (int i = 0; i < Options.NumberOfProgressBars; i++) progressBarChars += '―';
            if (Options.HideMessage && string.IsNullOrEmpty(Header) && !string.IsNullOrEmpty(Message)) Header = Message;
            FixLengthOfBars();
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        private void FixLengthOfBars()
        {
            if (!string.IsNullOrEmpty(Header)) numberOfBars = Options.NumberOfProgressBars - Header.Length - 1 -
                    (Options.ShowProgressNumbers ? $" [{Current}/{Total}]".Length : 0);
            else numberOfBars = Options.NumberOfProgressBars;
        }

        public void Show()
        {
            showing = true;
            if (!Program.Bars.Contains(this)) Program.Bars.Add(this);
            UpdateText();
        }

        public void Clear()
        {
            if (consoleLine < Program.TopLine) return;
            ConsoleExtensions.ClearLine(consoleLine);
            if (Options.AppearOnNewLine) ConsoleExtensions.ClearLine(consoleLine + 1);
        }

        public void Remove()
        {
            stopwatch.Stop();
            showing = false;
            Clear();
            Program.Bars.Remove(this);
        }

        public void UpdateHeader(string header)
        {
            Header = header;
            Message = Header;
            Clear();
            UpdateText();
        }

        public void Move(int newLine)
        {
            if (!Program.Bars.Contains(this)) return;
            Clear();
            consoleLine = newLine;
            if(showing) UpdateText();
        }

        public void MoveLocal(int amount)
        {
            if (!Program.Bars.Contains(this)) return;
            Clear();
            consoleLine += amount;
            if (showing) UpdateText();
        }

        public void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Remove();
        }

        public void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Total = 10000;
            Report(1);
        }

        public void Report(float value)
        {
            if (!Program.Bars.Contains(this)) return;
            if (Completed || !showing) return;
            Current = Clamp(value, 0f, Total);
            UpdateText();
            if (Current >= Total)
            {
                Completed = true;
                if (Options.RemoveOnComplete)
                {
                    Remove();
                    Program.Bars.Remove(this);
                }
            }
        }

        private double secondsPerPercent;
        private double minutesPerPercent;
        private double hoursPerPercent;

        private void UpdateText()
        {
            if (consoleLine < Program.TopLine) return;
            FixLengthOfBars();
            ConsoleExtensions.GotoLine(consoleLine);
            GetProgressLines();
            if (!Options.HideMessage)
            {
                secondsPerPercent = (stopwatch.Elapsed.TotalSeconds / Current) * (Total - Current);
                minutesPerPercent = Math.Round(secondsPerPercent / 60, 2);
                hoursPerPercent = Math.Round(minutesPerPercent / 60, 2);

                Console.ForegroundColor = Options.ColorMessage;
                Console.Write($"\r{this.Message} {(Options.ShowProgressNumbers ? $"[{Current}/{Total}]" : "")} \t[ETA: {(secondsPerPercent == 0 ? "Calculating" : $"{minutesPerPercent} minute(s) | {hoursPerPercent} hour(s)")}]");
            }
            if (Options.AppearOnNewLine) ConsoleExtensions.GotoLine(consoleLine + 1);
            if (!string.IsNullOrEmpty(Header))
            {
                Console.ForegroundColor = Options.ColorHeader;
                Console.Write($"{Header} {(Options.ShowProgressNumbers ? $"[{Current}/{Total}] " : " ")}");
            }
            foreach (char c in progressBarChars)
            {
                if(c == Options.ProgressBarDoneChar)
                    Console.ForegroundColor = Options.ColorLineCompletePercent;
                else
                    Console.ForegroundColor = Options.ColorLinePercent;
                Console.Write(c);
            }
            Console.ForegroundColor = Options.ColorPercent;
            Console.Write($" {Math.Round((Current/Total) * 100, 2)}%\t");
            Console.ForegroundColor = ConsoleColor.White;
            //string line = $"\r{this.message}{(AppearOnNewLine ? "\n" : "\t")} [{progressBarChars}] {(Current / Total) * 100}%\t";
        }

        private void GetProgressLines()
        {
            int progressDone = (int)((Current / Total) * numberOfBars);
            progressBarChars = string.Empty;
            progressBarChars += '[';
            for (int i = 0; i < numberOfBars; i++)
            {
                progressBarChars += i < progressDone ? Options.ProgressBarDoneChar : Options.ProgressBarChar;
            }
            progressBarChars += ']';
        }

        private static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }

    public class ProgressBarOptions
    {
        public int Priority = -1;
        public int NumberOfProgressBars = 100;
        public double EtaInterval = 1d;
        public char ProgressBarChar = '|';
        public char ProgressBarDoneChar = '\u25A0';
        public bool AppearOnNewLine;
        public bool HideMessage;
        public bool RemoveOnComplete;
        public bool ShowProgressNumbers;
        public ConsoleColor ColorMessage = ConsoleColor.Green;
        public ConsoleColor ColorHeader = ConsoleColor.Cyan;
        public ConsoleColor ColorPercent = ConsoleColor.Green;
        public ConsoleColor ColorLinePercent = ConsoleColor.Magenta;
        public ConsoleColor ColorLineCompletePercent = ConsoleColor.Cyan;

        public ProgressBarOptions() { }
        public ProgressBarOptions (ProgressBarOptions options)
        {
            Priority = options.Priority;
            NumberOfProgressBars = options.NumberOfProgressBars;
            EtaInterval = options.EtaInterval;
            ProgressBarChar = options.ProgressBarChar;
            ProgressBarDoneChar = options.ProgressBarDoneChar;
            AppearOnNewLine = options.AppearOnNewLine;
            HideMessage = options.HideMessage;
            RemoveOnComplete = options.RemoveOnComplete;
            ShowProgressNumbers = options.ShowProgressNumbers;
            ColorMessage = options.ColorMessage;
            ColorHeader = options.ColorHeader;
            ColorPercent = options.ColorPercent;
            ColorLinePercent = options.ColorLinePercent;
            ColorLineCompletePercent = options.ColorLineCompletePercent;
        }
    }
}
