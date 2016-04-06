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
using System.Threading.Tasks;
using Fildo.Core.Entities;

namespace Fildo.Droid.Adapters
{
    public class AutoCompleteAdapter : BaseAdapter<AutocompleteSearch>
    {
        private Context context;
        private List<AutocompleteSearch> autocompleteSearches;
        private static Dictionary<string, Bitmap> autoCompleteImages = new Dictionary<string, Bitmap>();

        public AutoCompleteAdapter(Context context, List<AutocompleteSearch> autocompleteSearches)
        {
            this.context = context;
            this.autocompleteSearches = autocompleteSearches;;
        }
        public override AutocompleteSearch this[int position]
        {
            get
            {
                return this.autocompleteSearches[position];
            }
        }

        public override int Count
        {
            get
            {
                return this.autocompleteSearches.Count;
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
                ViewHolderAutoComplete holder;
                View row = convertView;
                if (row == null)
                {
                    row = LayoutInflater.From(context).Inflate(Resource.Layout.AutocompleteRow, null, false);
                    holder = new ViewHolderAutoComplete();
                    holder.imageView = row.FindViewById<ImageView>(Resource.Id.ImageAutocomplete);
                    holder.nameTextView = row.FindViewById<TextView>(Resource.Id.NameTextView);
                    holder.typeTextView = row.FindViewById<TextView>(Resource.Id.TypeTextView);
                    row.Tag = holder;
                }
                else
                {
                    holder = row.Tag as ViewHolderAutoComplete;
                }

                holder.typeTextView.Text = this.autocompleteSearches[position].ResultType;
                holder.nameTextView.Text = this.autocompleteSearches[position].Name;
                //holder.imageView.SetImageBitmap(GetImageBitmapFromUrl(this.autocompleteSearches[position].PicUrl));
                if (autoCompleteImages.ContainsKey(this.autocompleteSearches[position].PicUrl))
                {
                    holder.imageView.SetImageBitmap(autoCompleteImages[this.autocompleteSearches[position].PicUrl]);
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

        private async void UpdateImage(ViewHolderAutoComplete holder, int position)
        {
            autoCompleteImages[this.autocompleteSearches[position].PicUrl] = await GetImageBitmapFromUrl(this.autocompleteSearches[position].PicUrl);
            holder.imageView.SetImageBitmap(autoCompleteImages[this.autocompleteSearches[position].PicUrl]);
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

    public class ViewHolderAutoComplete : Java.Lang.Object
    {
        public TextView nameTextView;
        public TextView typeTextView;
        public ImageView imageView;
    }
}