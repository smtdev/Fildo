using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Java.IO;
using Java.Net;
using Android.Graphics;
using System.Net;
using Android.Media;
using Fildo.Core.Entities;
using Fildo.Core;
using Fildo.Droid.SpecificPlatform;

namespace Fildo.Droid.Adapters
{
    public class SongsAdapter : BaseAdapter<Song>
    {
        private Context context;
        private List<Song> songs;
        private int? playingPosition;

        private static Downloader downloader = new Downloader();
        
        public SongsAdapter(Context context, List<Song> songs)
        {
            this.context = context;
            this.songs = songs;
        }
        public override Song this[int position]
        {
            get
            {
                return this.songs[position];
            }
        }

        public override int Count
        {
            get
            {
                return this.songs.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                View row = convertView;
                if (row == null)
                {
                    row = LayoutInflater.From(context).Inflate(Resource.Layout.SongsRow, null, false);
                }
                ImageView playIcon = row.FindViewById<ImageView>(Resource.Id.PlayIcon);
                ImageView downloadIcon = row.FindViewById<ImageView>(Resource.Id.DownloadIcon);
                playIcon.Tag = position;
                TextView artistTextViewSongList = row.FindViewById<TextView>(Resource.Id.ArtistTextViewSongList);
                TextView songTextViewSongList = row.FindViewById<TextView>(Resource.Id.SongTextViewSongList);
                artistTextViewSongList.Text = this.songs[position].Artist;
                songTextViewSongList.Text = this.songs[position].Title;
                if (position == playingPosition)
                {
                    string url = this.songs[position].Url;
                    if (string.IsNullOrEmpty(url))
                    {
                        playIcon.SetImageDrawable(this.context.Resources.GetDrawable(Resource.Drawable.pause));
                        this.GetSong(this.songs[position].Id);
                    }
                    else
                    {
                        if (!Container.Player.IsPlaying)
                        {
                            Container.Player.Reset();
                            Container.Player.SetDataSource(this.songs[position].Url);
                            Container.Player.Prepare();
                            Container.Player.Start();
                        }
                        else
                        {
                            playIcon.SetImageDrawable(this.context.Resources.GetDrawable(Resource.Drawable.pause));
                        }
                    }
                }
                else
                {
                    playIcon.SetImageDrawable(this.context.Resources.GetDrawable(Resource.Drawable.play));
                }

                playIcon.Click -= PlayIcon_Click;
                playIcon.Click += PlayIcon_Click;
                playIcon.Tag = position;
                downloadIcon.Click -= DownloadIcon_Click;
                downloadIcon.Click += DownloadIcon_Click;
                downloadIcon.Tag = position;
                //imageView.Visibility = ViewStates.Gone;

                return row;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private async void GetSong(string id)
        {
            var progress = new ProgressDialog(context);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetMessage("Searching. Please wait...");
            progress.SetCancelable(false);
            progress.Show();
            NetEase netEase = new NetEase();
            Song song = await netEase.GetSong(id);
            Container.Player.Reset();
            Container.Player.SetDataSource(song.Url);
            Container.Player.Prepare();
            Container.Player.Start();
            progress.Hide();
        }

        private async void DownloadSong(string id, int position)
        {
            NetEase netEase = new NetEase();
            Song song = await netEase.GetSong(id);
            downloader.DownloadMp3(song.Url, song.Artist, song.Title, position);
        }

        private void DownloadIcon_Click(object sender, EventArgs e)
        {
            int position = (int)((ImageView)sender).Tag;
            Song song = this.songs[position];
            downloader.Downloaded -= Downloader_Downloaded;
            downloader.Downloaded += Downloader_Downloaded;
            if (!string.IsNullOrEmpty(song.Url))
            {
                downloader.DownloadMp3(song.Url, song.Artist, song.Title, position);
            }
            else
            {
                this.DownloadSong(song.Id, position);
            }
        }

        private void Downloader_Downloaded(object sender, int e)
        {
            
        }

        private void PlayIcon_Click(object sender, EventArgs e)
        {
            ImageView button = (ImageView)sender;
            if (this.playingPosition == null || ((int)button.Tag != (int)this.playingPosition) || !Container.Player.IsPlaying)
            {
                button.SetImageDrawable(this.context.Resources.GetDrawable(Resource.Drawable.pause));
                Container.Player.Stop();
                this.playingPosition = (int)button.Tag;
            }
            else
            {
                this.playingPosition = null;
                button.SetImageDrawable(this.context.Resources.GetDrawable(Resource.Drawable.play));
                Container.Player.Pause();
            }
            this.NotifyDataSetChanged();
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }

    }
}