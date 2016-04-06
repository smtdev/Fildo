using Android.App;
using Android.Content;
using Android.Widget;
using Fildo.Core;
using Fildo.Core.Entities;
using Fildo.Core.IPlatform;
using Java.IO;
using MvvmCross.Platform;
using MvvmCross.Plugins.File;
using System;
using System.Linq;
using System.Net;
using NETIO = System.IO;
using Uri = Android.Net.Uri;

namespace Fildo.Droid.SpecificPlatform
{
    using Fildo.Core.Resources;

    public class Downloader : IDownloader
    {
        private int? index;
        private readonly INetEase netEase;
        public event EventHandler<int> ProgressChanged;
        public event EventHandler<int> Downloaded;
        private Download actualDownload;

        public Downloader(INetEase netEase)
        {
            this.netEase = netEase;
        }

        public async void DownloadMp3(Download downloadItem, string uri, string artist, string title, int? index, bool isAlbum = false, string albumName = "", bool isPlaylist = false, string plname = "")
        {
            this.actualDownload = downloadItem;
            try
            {
                var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);

                bool useProxy = prefs.GetBoolean("UseProxy", false);
                string proxy = string.Empty;
                if (useProxy)
                {
                    proxy = prefs.GetString("ProxyConf", string.Empty);
                }

                if (!useProxy)
                {
                    uri = (await this.netEase.FixUrl(new SongNetease() { Artist = artist, Title = title, Url = uri })).Url;
                }

                title = title.Replace("/", " ").Replace("\\", " ").Replace("&#039;", " ");
                artist = artist.Replace("/", " ").Replace("\\", " ").Replace("&#039;", " ");
                albumName = albumName.Replace("/", " ").Replace("\\", " ").Replace("&#039;", " ");
                plname = plname.Replace("/", " ").Replace("\\", " ").Replace("&#039;", " ");

                if (uri == null)
                {
                    if (this.Downloaded != null)
                    {
                        this.Downloaded(this, index ?? default(int));
                    }
                    return;
                }
                
                this.index = index;
                
                string dest = string.Empty;
                File externalFilesDirs = null;
                try
                {
                    externalFilesDirs = Application.Context.GetExternalMediaDirs().Where(p => !p.AbsolutePath.ToLowerInvariant().Contains("emula")).FirstOrDefault();
                }
                catch (Exception)
                {
                    // Avoid exception on Android 4.4.
                }

                if (externalFilesDirs == null)
                {
                    dest = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                }
                else
                {
                    dest = externalFilesDirs.AbsolutePath.Replace("Android", string.Empty).Replace("/media", string.Empty).Replace("/net.fildo.app", string.Empty);
                }

                string extension = ".mp3";
                if (uri.EndsWith(".m4a")  || uri.Contains(".m4a?"))
                {
                    extension = ".m4a";
                }

                //dest = dest.Replace("Android", string.Empty).Replace("/media", string.Empty).Replace("/Fildo.Fildo", string.Empty);

                if (prefs.GetBoolean("SaveExternalSD", false) && (externalFilesDirs != null))
                {

                    var sdFolder = dest;//.Replace("Android", string.Empty).Replace("/media", string.Empty).Replace("/Fildo.Fildo", string.Empty);

                    if (prefs.GetBoolean("SaveUnderArtistFolder", false) && isAlbum && index != null)
                    {
                        File root = new File(sdFolder, "Fildo/" + artist + "/" + albumName + "/" + index.Value.ToString("00") + " " + title + extension);
                        dest = root.AbsolutePath;
                    }
                    else if (prefs.GetBoolean("SaveUnderArtistFolder", false) && !isAlbum && isPlaylist)
                    {
                        File root = new File(sdFolder, "Fildo/" + plname + "/" + artist + " - " + title + extension);
                        dest = root.AbsolutePath;
                    }
                    else if (prefs.GetBoolean("SaveUnderArtistFolder", false) && !isAlbum && !isPlaylist)
                    {
                        File root = new File(sdFolder, "Fildo/" + artist + "/" + title + extension);
                        dest = root.AbsolutePath;
                    }
                    else
                    {
                        File root = new File(sdFolder, "Fildo/" + artist + " - " + title + extension);
                        dest = root.AbsolutePath;
                    }
                }
                else
                {
                    dest = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                    if (prefs.GetBoolean("SaveUnderArtistFolder", false) && isAlbum && index != null)
                    {
                        dest += "/" + artist + "/" + albumName + "/" + index.Value.ToString("00") + " " + title + extension;
                    }
                    else if (prefs.GetBoolean("SaveUnderArtistFolder", false) && !isAlbum && isPlaylist)
                    {
                        dest += "/" + plname + "/" + artist + " - " + title + extension;
                    }
                    else if (prefs.GetBoolean("SaveUnderArtistFolder", false) && !isAlbum && !isPlaylist)
                    {
                        dest += "/" + artist + "/" + title + extension;
                    }
                    else
                    {
                        dest += "/" + artist + " - " + title + extension;
                    }
                }
                /**********************************************/
                //request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads + "/" + artist + "/" + albumName + "/", index.Value.ToString("00") + " " + title + ".mp3");
                //request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, plname + "/" + artist + " - " + title + ".mp3");
                //request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, artist + "/" + title + ".mp3");
                //request.SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, artist + " - " + title + ".mp3");
                /**********************************************/
                
                var fileService = Mvx.Resolve<IMvxFileStore>();

                if (!fileService.Exists(dest))
                {
                    Toast.MakeText(Application.Context, "Start Download:" + artist + " - " + title + extension, ToastLength.Long).Show();
                    
                    //var javafile = 
                    string lastPart = dest.Split('/').Last();
                    string folderTemp = dest.Replace(lastPart, string.Empty);

                    fileService.EnsureFolderExists(folderTemp);

                    using (WebClient client = new WebClient())
                    {
                        client.DownloadProgressChanged += this.Client_DownloadProgressChanged;
                        if (uri.StartsWith("http://221.228.64.228/"))
                        {
                            client.Headers.Add("Host", "m1.music.126.net");
                        }
                        if (useProxy && !string.IsNullOrEmpty(proxy))
                        {
                            WebProxy webProxy = new WebProxy();
                            webProxy.Address = new System.Uri(proxy);
                            client.Proxy = webProxy;
                        }

                        await client.DownloadFileTaskAsync(new System.Uri(uri), dest);
                    }

                    if (!uri.EndsWith(".m4a"))
                    {
                        using (TagLib.File f = TagLib.File.Create(new TagFileTest(dest), TagLib.ReadStyle.None))
                        {
                            f.Tag.Performers = null; //clearing out performers
                            f.Tag.Performers = new[] { artist }; //works now
                            if (isAlbum)
                            {
                                f.Tag.Album = albumName;
                            }
                            f.Tag.Title = title;
                            f.Save();
                        }
                    }


                    Uri contentUri = Uri.Parse("file://" + dest);
                    Intent mediaScanIntent2 = new Intent(Intent.ActionMediaScannerScanFile, contentUri);
                    Application.Context.SendBroadcast(mediaScanIntent2);
                    
                    if (this.Downloaded != null)
                    {
                        this.Downloaded(this, index ?? default(int));
                    }

                    Toast.MakeText(Application.Context, "Downloaded: " + artist + " - " + title + extension +".", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(Application.Context, 
                        string.Format(Texts.SkipExistDownload, artist, title, extension), 
                        ToastLength.Long).Show();
                    if (this.Downloaded != null)
                    {
                        this.Downloaded(this, index ?? default(int));
                    }

                }
            }
            catch (Exception ex)
            {
                if (this.Downloaded != null)
                {
                    this.Downloaded(this, index ?? default(int));
                }

                Toast.MakeText(Application.Context, 
                    string.Format(Texts.ErrorDownloading, artist, title), 
                    ToastLength.Long).Show();
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.actualDownload.Percent = e.ProgressPercentage;
        }
    }

    public class TagFileTest : TagLib.File.IFileAbstraction
    {
        private string dest;
        private IMvxFileStore fileService;
        private System.IO.Stream openStream;

        public TagFileTest(string dest)
        {
            this.dest = dest;
            this.fileService = Mvx.Resolve<IMvxFileStore>();
        }

        public string Name
        {
            get
            {
                return this.dest;
            }
        }

        public System.IO.Stream ReadStream
        {
            get
            {
                this.openStream  = System.IO.File.Open(this.dest, NETIO.FileMode.Open);
                return this.openStream;
            }
        }

        public System.IO.Stream WriteStream
        {
            get
            {
                return this.openStream;
            }
        }

        public void CloseStream(System.IO.Stream stream)
        {
            stream.Close();
        }
    }
}