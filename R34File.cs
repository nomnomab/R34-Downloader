using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace R34Downloader
{
    public class R34File
    {
        public XmlDocument Xml;
        public string Url;
        public string FileExtension;
        public string Name;
        public string Path;
        public long Size;

        public R34File(string url, string tags)
        {
            Url = url;
            string[] split = url.Split('/');
            if (!url.EndsWith(".webm")) Name = split[split.Length - 1].Split('?')[0];
            else Name = split[split.Length - 1];
            FileExtension = Name.Split('.')[1];
            Path = $"{Environment.CurrentDirectory}/Images/{tags}/{Name}";
        }
    }
}
