using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace R34Downloader
{
    class Downloader
    {
        ProgressBarOptions options = new ProgressBarOptions()
        {
            AppearOnNewLine = true,
            ShowProgressNumbers = true
        };
        ProgressBarOptions oneLine = new ProgressBarOptions()
        {
            HideMessage = true,
            ShowProgressNumbers = true
        };

        private readonly string saveLocation = Environment.CurrentDirectory + "/Images/";
        private R34 r34;
        private Queue<R34File> retrievedFiles;
        private ProgressBar masterProgressBar;
        private ProgressBar downloadingProgressBar;
        private Thread downloadingThread;
        private Stopwatch stopwatch;
        private bool isDownloading;

        public Downloader()
        {
            if (!Directory.Exists(saveLocation)) Directory.CreateDirectory(saveLocation);
        }

        public async Task Download(R34 r34)
        {
            Console.CursorVisible = false;

            this.r34 = r34;
            retrievedFiles = new Queue<R34File>();

            stopwatch = new Stopwatch();
            stopwatch.Start();

            await FetchPages();
        }

        private async Task FetchPages()
        {
            CheckForSaveLocation(r34.Tags);

            string url = r34.DefaultUrl;
            string[] tags = r34.TagArray;
            int index = 0;
            foreach (string tag in tags)
            {
                r34.ModifiedTags += tag;
                if (index < tags.Length - 1) r34.ModifiedTags += '+';
            }
            url += r34.ModifiedTags;

            //int totalCount = FetchTotalImages(url);
            await Fetch(url);
        }

        private async Task Fetch(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)(await req.GetResponseAsync());
            if (response != null
                && response.StatusCode != HttpStatusCode.Unauthorized
                && response.SupportsHeaders
                && response.Headers["Content-Type"] != null
                && response.Headers["Content-Type"].Contains("xml"))
            {
                StreamReader ResponseDataStream = new StreamReader(response.GetResponseStream());
                String res = ResponseDataStream.ReadToEnd();

                XmlDocument XMLDoc = new XmlDocument();
                XMLDoc.LoadXml(res);

                XmlNode totalNode = XMLDoc.ChildNodes[1];
                int count = r34.Amount > 0 ? r34.Amount : int.Parse(totalNode.Attributes["count"].Value);

                masterProgressBar = new ProgressBar("Retrieving File Paths", count, options);
                masterProgressBar.Show();

                int pageCount = count / r34.ImagesPerPage;
                Log("Pagecount: " + pageCount, "Log");

                r34.SideDownloading = 2;
                // grab all image links
                // batch fetch

                for(int i = 0; i < pageCount; i++)
                {
                    await FetchPageImages(url, i, count);
                }
            }
        }

        private async Task FetchPageImages(string url, int page, int amount)
        {
            HttpWebRequest pageReq = (HttpWebRequest)WebRequest.Create(url + r34.DefaultPageUrl + page);
            HttpWebResponse pageResponse = (HttpWebResponse)(await pageReq.GetResponseAsync());

            if (pageResponse != null
            && pageResponse.StatusCode != HttpStatusCode.Unauthorized
            && pageResponse.SupportsHeaders
            && pageResponse.Headers["Content-Type"] != null
            && pageResponse.Headers["Content-Type"].Contains("xml"))
            {
                StreamReader PageResponseDataStream = new StreamReader(pageResponse.GetResponseStream());
                String pageRes = PageResponseDataStream.ReadToEnd();

                XmlDocument xmlPage = new XmlDocument();
                xmlPage.LoadXml(pageRes);

                ProgressBar pageBar = new ProgressBar("Retrieving ", r34.ImagesPerPage, oneLine);
                pageBar.Show();

                int index = 0;
                foreach (XmlNode post in xmlPage.ChildNodes[1].ChildNodes)
                {
                    if(retrievedFiles.Count >= amount)
                    {
                        // stop
                        return;
                    }
                    string file = post.Attributes["file_url"].Value;
                    R34File r34File = new R34File(file, r34.ModifiedTags);
                    retrievedFiles.Enqueue(r34File);
                    pageBar.Report(index);
                    masterProgressBar.Report(retrievedFiles.Count);
                    index++;
                }
                pageBar.Remove();
            }

            await DownloadImages();
        }

        private bool updatedLine = false;
        private int sizeOfRetrieved;

        private async Task DownloadImages()
        {
            sizeOfRetrieved = retrievedFiles.Count;
            while (true)
            {
                if (retrievedFiles.Count <= 0) break;
                R34File file = retrievedFiles.Dequeue();

                if (File.Exists(file.Path)) continue;

                // goes once we can
                //ProgressBar bar = new ProgressBar($"Downloading Image [{file.Name}]", 100, oneLine);
                WebClient client = new WebClient();
                while (client.IsBusy) { }
                //client.DownloadProgressChanged += bar.Client_DownloadProgressChanged;
                //client.DownloadFileCompleted += bar.Client_DownloadFileCompleted;
                //bar.Show();
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                await client.DownloadFileTaskAsync(new Uri(file.Url), file.Path);
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!updatedLine)
            {
                updatedLine = true;
                ConsoleExtensions.GotoLine(Console.CursorTop + 1);
            }
            Console.Write($"\r[{Math.Abs(sizeOfRetrieved - retrievedFiles.Count)}/{sizeOfRetrieved}] Downloading file {e.BytesReceived}/{e.TotalBytesToReceive} bytes ({e.ProgressPercentage}%)\t\t");
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ConsoleExtensions.ClearLine(Console.CursorTop);
        }

        //private int FetchTotalImages(string url)
        //{
        //    try
        //    {
        //        HtmlWeb web = new HtmlWeb();
        //        var htmlDoc = web.Load(url);
        //        var node = htmlDoc.DocumentNode.SelectSingleNode("//a[@alt='last page']");
        //        if (node == null)
        //        {
        //            // only this page
        //            return r34.ImagesPerPage;
        //        }
        //        string href = node.Attributes.FirstOrDefault(a => a.Name == "href").Value;
        //        string[] split = href.Split('=');
        //        return int.Parse(split[split.Length - 1]);
        //    }
        //    catch(Exception e)
        //    {
        //        AddError(e.Message);
        //    }

        //    return 0;
        //}

        //private void FetchPage(string url, int page)
        //{
        //    int pageImageCount = (r34.ImagesPerPage * page) + 1;
        //    if (pageImageCount / r34.ImagesPerPage <= 0) pageImageCount = 0;

        //    HtmlWeb web = new HtmlWeb();
        //    while (true)
        //    {
        //        try
        //        {
        //            var htmlDoc = web.Load(url + r34.DefaultPageUrl + pageImageCount);
        //            var nodes = htmlDoc.DocumentNode.SelectNodes("//div/span/a").ToArray();
        //            if (nodes == null || nodes.Length <= 0) break;
        //            foreach (HtmlNode node in nodes)
        //                if (node.HasAttributes)
        //                {
        //                    string value = node.Attributes.FirstOrDefault(a => a.Name == "href").Value;
        //                    R34File file = new R34File(value);
        //                    switch (r34.TypesToDownload)
        //                    {
        //                        case FileType.Videos:
        //                            if (file.Name.EndsWith(".webm")) retrievedFiles.Add(new R34File(value));
        //                            break;
        //                        case FileType.ImagesWithoutGifs:
        //                            if (file.Name.EndsWith(".gif")) continue;
        //                            break;
        //                        default:
        //                            retrievedFiles.Add(new R34File(value));
        //                            break;
        //                    }
        //                    masterProgressBar.Report(retrievedFiles.Count);
        //                }
        //        }
        //        catch(Exception e)
        //        {
        //            AddError(e.Message);
        //        }
        //        break;
        //    }
        //}

        //private void FetchImages()
        //{
        //    ProgressBarOptions options = new ProgressBarOptions()
        //    {
        //        AppearOnNewLine = true,
        //        RemoveOnComplete = true
        //    };

        //    masterProgressBar = new ProgressBar("Retrieving Actual Image URLs...", retrievedFiles.Count, options);
        //    Program.Bars.Add(masterProgressBar);

        //    List<R34File> copyList = new List<R34File>(retrievedFiles);
        //    retrievedFiles = new List<R34File>();
        //    int index = 0;
        //    while (index < copyList.Count)
        //    {
        //        try
        //        {
        //            string url = copyList[index].Url;
        //            url = r34.BasicUrl + url.Replace(';', '&');
        //            HtmlWeb web = new HtmlWeb();
        //            var htmlDoc = web.Load(url);
        //            var node = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='right-col']/div/img");
        //            if (node == null) node = htmlDoc.DocumentNode.SelectSingleNode("//video[@id='gelcomVideoPlayer']/source");
        //            string file = node.Attributes.FirstOrDefault(att => att.Name == "src").Value;
        //            url = file;

        //            retrievedFiles.Add(new R34File(url));
        //            masterProgressBar.Report(index);
        //            index++;
        //        }
        //        catch(Exception e)
        //        {
        //            AddError(e.Message);
        //            index++;
        //        }
        //    }

        //    masterProgressBar.Remove();
        //}

        private void Log(string line, string file = "error")
        {
            using (StreamWriter writer = 
                !File.Exists(saveLocation + file + ".txt") 
                ? File.CreateText(saveLocation + file + ".txt") 
                : File.AppendText(saveLocation + file + ".txt"))
            {
                writer.WriteLine(line);
                writer.Close();
            }
        }

        //private void FetchImageSizes()
        //{
        //    ProgressBarOptions options = new ProgressBarOptions()
        //    {
        //        AppearOnNewLine = true,
        //        RemoveOnComplete = true
        //    };

        //    masterProgressBar = new ProgressBar("Retrieving File Sizes...", retrievedFiles.Count, options);
        //    Program.Bars.Add(masterProgressBar);

        //    int index = 0;
        //    while (index < retrievedFiles.Count)
        //    {
        //        R34File file = retrievedFiles[index];
        //        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(file.Url);
        //        req.Method = "HEAD";
        //        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        //        long len = resp.ContentLength;
        //        file.Size = len;

        //        index++;
        //        masterProgressBar.Report(index);
        //    }

        //    masterProgressBar.Remove();
        //}

        private int currentNumberOfDownloads;

        private void CheckForSaveLocation(string folder)
        {
            if (!Directory.Exists(saveLocation + folder))
                Directory.CreateDirectory(saveLocation + folder);
        }
    }
}
