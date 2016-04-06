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
using System.Threading;
using System.Threading.Tasks;
using Fildo.Core.Entities;

namespace Fildo.Droid.Adapters
{
    public class TopAlbumsAdapter : BaseAdapter<Album>
    {
        private Context context;
        private List<Album> albums;
        private static Dictionary<string, Bitmap> albumsImages = new Dictionary<string, Bitmap>();

        public TopAlbumsAdapter(Context context, List<Album> albums)
        {
            this.context = context;
            this.albums = albums;
        }
        public override Album this[int position]
        {
            get
            {
                return this.albums[position];
            }
        }

        public override int Count
        {
            get
            {
                return this.albums.Count;
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
                ViewHolder holder;
                View row = convertView;
                if (row == null)
                {
                    row = LayoutInflater.From(context).Inflate(Resource.Layout.toprowsalbum, null, false);
                    holder = new ViewHolder();

                    //holder.imageView = row.FindViewById<ImageView>(Resource.Id.AlbumCover);
                    holder.artistTextView = row.FindViewById<TextView>(Resource.Id.ArtistTextView);
                    holder.albumTextView = row.FindViewById<TextView>(Resource.Id.AlbumTextView);
                    /*artistTextView.Text = this.albums[position].Artist;
                    albumTextView.Text = this.albums[position].Name;
                    imageView.SetImageBitmap(GetImageBitmapFromUrl(this.albums[position].ImageUrl));*/
                    row.Tag = holder;
                }
                else
                {
                    holder = row.Tag as ViewHolder;
                }

                holder.artistTextView.Text = this.albums[position].Artist;
                holder.albumTextView.Text = this.albums[position].Name;
                
                if (albumsImages.ContainsKey(this.albums[position].ImageUrl))
                {
                    holder.imageView.SetImageBitmap(albumsImages[this.albums[position].ImageUrl]);
                }
                else
                {
                    this.UpdateImage(holder, position);
                }

                return row;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private async void UpdateImage(ViewHolder holder, int position)
        {
            albumsImages[this.albums[position].ImageUrl] = await GetImageBitmapFromUrl(this.albums[position].ImageUrl);
            holder.imageView.SetImageBitmap(albumsImages[this.albums[position].ImageUrl]);
        }

        private async Task<Bitmap> GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = await webClient.DownloadDataTaskAsync(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }

    }

    public class ViewHolder : Java.Lang.Object
    {
        public TextView artistTextView;
        public TextView albumTextView;
        public ImageView imageView;
	}
}