using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace R34Downloader
{
    public enum FileType
    {
        Everything,
        ImagesWithGifs,
        ImagesWithoutGifs,
        Videos
    }

    class Program
    {
        public static string Version = "3.31.18";
        public static int TopLine = 4;
        public static ObservableCollection<ProgressBar> Bars = new ObservableCollection<ProgressBar>();

        static void Main(string[] args) => Program.MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            Bars.CollectionChanged += Bars_CollectionChanged;

            //ProgressBarOptions options = new ProgressBarOptions()
            //{
            //    AppearOnNewLine = true,
            //    RemoveOnComplete = true
            //};

            //ProgressBarOptions downloadOptions = new ProgressBarOptions()
            //{
            //    HideMessage = true
            //};

            //ProgressBar bar = new ProgressBar("MESSAGE", 10, options);
            //ProgressBar bar2 = new ProgressBar("MESSAGE2", 100, downloadOptions);

            //Bars.Add(bar);
            //Bars.Add(bar2);

            //float value = 0f;
            //bool sample = false;
            //while (true)
            //{
            //    while (sample) continue;
            //    bar.Report(value);
            //    sample = true;
            //    while (value < 100)
            //    {
            //        bar2.Report(value);
            //        value += 0.1f;
            //    }
            //}

            //while (true) { }
            //return;

            Console.WriteLine($"https://rule34.xxx Batch Downloader\nMade by: Nomie\nVersion: {Version}");
            ConsoleExtensions.AddEmptyLine();

            InputSettings settings = new InputSettings()
            {
                RemoveOnCompletion = true,
            };
            string tags = (string)ConsoleExtensions.AddInput("[Tags: Seperate tags with ,] :", settings);
            settings = new InputSettings()
            {
                RemoveOnCompletion = true,
                IsNumber = true,
                CanBeEmpty = true
            };
            int amountToDownload = (int)ConsoleExtensions.AddInput("[Amount to Download: Leave blank for max] :", settings);
            settings = new InputSettings()
            {
                RemoveOnCompletion = true,
                IsNumber = true,
                SubtitleLines = new string[]
                {
                    "[0]\t= Everything",
                    "[1]\t= Images and Gifs",
                    "[2]\t= Images without Gifs",
                    "[3]\t= Videos"
                },
                CustomInputLine = "[#] :"
            };
            int typesToDownload = (int)ConsoleExtensions.AddInput("[Types to Download]", settings);

            Console.WriteLine($"Downloading {(amountToDownload == 0 ? "all" : amountToDownload.ToString())} files with the tag(s) [{tags}]");
            FileType fType = (FileType)typesToDownload;
            Console.WriteLine($"Types downloading : [{fType.ToString()}]");

            Downloader downloader = new Downloader();
            R34 r34 = new R34()
            {
                Tags = tags,
                Amount = amountToDownload,
                TypesToDownload = fType
            };
            TopLine += 3;
            await downloader.Download(r34);
            while (true) { }
        }

        /// <summary>
        /// When a progress bar gets removed or added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Bars_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateBars();
        }

        public static void UpdateBars()
        {
            int addition = 0;
            ProgressBar[] bars = Bars.ToArray();
            //bool orderByPriority = Bars.FirstOrDefault(x => x.Options.Priority > -1) != null;
            //if(orderByPriority) bars = Bars.OrderBy(x => x.Options.Priority).ToArray();
            for (int i = 0; i < bars.Length; i++)
            {
                int newLine = TopLine + (i + (i > 0 ? 1 : 0));
                bool shownOnNewLine = bars[i].Options.AppearOnNewLine;
                addition += shownOnNewLine ? 1 : 0;
                ConsoleExtensions.ClearToTop();
                if (bars[i].OverriddenConsoleLine > -1) bars[i].Move(bars[i].OverriddenConsoleLine);
                else bars[i].Move(newLine + addition);
                bars[i].Show();
            }
        }
    }
}
