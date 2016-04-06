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
    public class PlaylistAdapter : BaseAdapter<Playlist>
    {
        private Context context;
        private List<Playlist> playlists;

        public PlaylistAdapter(Context context, List<Playlist> playlists)
        {
            this.context = context;
            this.playlists = playlists;
        }
        public override Playlist this[int position]
        {
            get
            {
                return this.playlists[position];
            }
        }

        public override int Count
        {
            get
            {
                return this.playlists.Count;
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
                ViewplaylistHolder holder;
                View row = convertView;
                if (row == null)
                {
                    row = LayoutInflater.From(context).Inflate(Resource.Layout.PlaylistRow, null, false);
                    holder = new ViewplaylistHolder();
                    holder.playlistName = row.FindViewById<TextView>(Resource.Id.PlaylistRowName);
                    row.Tag = holder;
                }
                else
                {
                    holder = row.Tag as ViewplaylistHolder;
                }

                holder.playlistName.Text = this.playlists[position].Name;

                return row;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }

    public class ViewplaylistHolder : Java.Lang.Object
    {
        public TextView playlistName;
	}
}