using Fildo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.IPlatform
{
    public interface IDownloader
    {
        void DownloadMp3(Download downloadItem, string uri, string artist, string title, int? index, bool isAlbum = false, string albumName = "", bool isPlaylist = false, string plname = "");


        event EventHandler<int> Downloaded; 
        event EventHandler<int> ProgressChanged;
    }
}
