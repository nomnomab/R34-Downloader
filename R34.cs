using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R34Downloader
{
    public class R34
    {
        public int ImagesPerPage => 41;
        public string BasicUrl => "https://rule34.xxx/";
        public string DefaultUrl => "https://rule34.xxx/index.php?page=dapi&s=post&q=index&tags=";
        public string DefaultPageUrl => "&pid=";

        public int SideDownloading = 0;

        public string Tags, ModifiedTags;
        public string[] TagArray => Tags.Split(',');
        public int Amount;
        public FileType TypesToDownload;
    }
}
