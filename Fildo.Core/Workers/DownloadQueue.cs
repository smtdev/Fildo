using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fildo.Core.Entities;
using Fildo.Core.IPlatform;
using System.Collections.ObjectModel;

namespace Fildo.Core.Workers
{
    public class DownloadQueue : IDownloadQueue
    {
        private readonly ObservableCollection<Download> queue;
        private readonly IDownloader downloader;
        private readonly INetEase netEase;
        private bool isDownloading;

        public event EventHandler<System.Collections.Specialized.NotifyCollectionChangedEventArgs> QueueChanged;

        public DownloadQueue(IDownloader downloader, INetEase netEase)
        {
            this.downloader = downloader;
            this.netEase = netEase;
            this.queue = new ObservableCollection<Download>();
            this.queue.CollectionChanged += this.Queue_CollectionChanged;
            this.downloader.ProgressChanged += this.Downloader_ProgressChanged;
            this.downloader.Downloaded += this.Downloader_Downloaded;
            this.isDownloading = false;
        }

        private void Queue_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.QueueChanged != null)
            {
                this.QueueChanged(this, e);
            }
        }

        public void Add(Song song)
        {
            this.queue.Add(new Download(song));
            if (!this.isDownloading)
            {
                this.isDownloading = true;
                this.StartDownload(this.queue.First());
            }
        }

        public void Add(Song song, bool isAlbum, string name, int? index)
        {
            if (isAlbum)
            {
                this.queue.Add(new Download(song, index, true, name,string.Empty));
            }
            else if (!isAlbum && string.IsNullOrEmpty(name))
            {
                this.queue.Add(new Download(song, index, null, string.Empty, string.Empty));
            }
            else
            {
                this.queue.Add(new Download(song, index, false, string.Empty, name));
            }

            if (!this.isDownloading)
            {
                this.isDownloading = true;
                this.StartDownload(this.queue.First());
            }
        }
        
        public void DownloadNext()
        {
            throw new NotImplementedException();
        }

        public void Remove(Song song)
        {
            var temp = this.queue.Where(p => p.Song.Id == song.Id).First();
            if (temp != null)
            {
                this.queue.Remove(temp);
            }
        }

        public void RemoveFirst()
        {
            if (this.queue.Count > 0)
            {
                this.queue.RemoveAt(0);
            }
        }
        
        public ObservableCollection<Download> GetAll()
        {
            return this.queue;
        }

        private void Downloader_Downloaded(object sender, int e)
        {
            if (this.queue.Count > 0)
            {
                this.RemoveFirst();
                if (this.queue.Count > 0)
                {
                    Download item = this.queue.First();
                    this.StartDownload(item);
                }
                else
                {
                    this.isDownloading = false;
                }
            }
            else
            {
                this.isDownloading = false;
            }
        }

        private async void StartDownload(Download item)
        {
            if (string.IsNullOrEmpty(item.Song.Url))
            {
                var tempsong = await this.netEase.GetSong(item.Song.Id);
                this.downloader.DownloadMp3(item, tempsong.Url, item.Song.Artist, item.Song.Title, item.Index, item.IsAlbum, item.AlbumName, item.IsPlaylist, item.PlaylistName);
            }
            else
            {
                this.downloader.DownloadMp3(item, item.Song.Url, item.Song.Artist, item.Song.Title, item.Index, item.IsAlbum, item.AlbumName, item.IsPlaylist, item.PlaylistName);
            }
        }

        private void Downloader_ProgressChanged(object sender, int e)
        {
            //this.Progress = e;
            /*if (e >= 100)
            {
                this.IsDownloading = false;
            }
            else
            {
                this.IsDownloading = true;
            }*/
        }

        public void ClearAll()
        {
            this.queue.Clear();
        }
    }
}
