using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R34Downloader
{
    public static class ConsoleExtensions
    {
        public static void GotoLine(int line)
        {
            Console.SetCursorPosition(0, line);
        }

        public static void ClearLine(int line)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, line);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void GotoBottom()
        {
            Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - 1);
        }

        public static void ClearToTop(int offset = 0)
        {
            int index = Console.CursorTop;
            for(int i = index; i >= Program.TopLine + offset; i--)
            {
                ClearLine(i);
                index--;
            }
            GotoLine(Program.TopLine);
        }

        public static void AddEmptyLine()
        {
            Console.WriteLine(string.Empty);
        }

        /// <summary>
        /// Display error at currentPosition + line
        /// After displaying, go back line + 1
        /// </summary>
        /// <param name="error"></param>
        /// <param name="line"></param>
        public static void DisplayError(string error, int line = 1)
        {
            int current = Console.CursorTop;
            Console.Write($"\r[Error] {error}");
            current -= line;
            GotoLine(current);
        }

        public static void ClearError()
        {
            ClearLine(Console.CursorTop);
        }

        private static int CurrentBottom;

        public static object AddInput(string message, InputSettings settings = null)
        {
            CurrentBottom = 0;
            if (settings == null) settings = new InputSettings();
            while (true)
            {
                Console.Write('\r' + message + " ");
                CurrentBottom = settings.SubtitleLines.Length;
                if (settings.SubtitleLines.Length > 0)
                {
                    GotoLine(Console.CursorTop + 1);
                    foreach (string subLine in settings.SubtitleLines)
                        Console.WriteLine(subLine);
                    if (!string.IsNullOrEmpty(settings.CustomInputLine))
                        Console.Write(settings.CustomInputLine + " ");
                }
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) && !settings.CanBeEmpty)
                {
                    if (settings.SubtitleLines.Length == 0)
                        DisplayError(settings.EmptyMessage);
                    else
                        DisplayError(settings.EmptyMessage, settings.SubtitleLines.Length + 2);
                    continue;
                }
                if (settings.IsNumber)
                {
                    int number = 0;
                    bool valid = int.TryParse(input, out number);
                    if (!valid && !settings.CanBeEmpty)
                    {
                        if (settings.SubtitleLines.Length == 0)
                            DisplayError(settings.InvalidNumberMessage);
                        else
                            DisplayError(settings.InvalidNumberMessage, settings.SubtitleLines.Length + 2);
                        continue;
                    }
                    ResetInput(settings);
                    return number;
                }
                ResetInput(settings);
                return input;
            }
        }

        private static void ResetInput(InputSettings settings)
        {
            ClearError();
            if (settings.RemoveOnCompletion)
            {
                ClearToTop();
            }
        }
    }
}
