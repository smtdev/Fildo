using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Fildo.Core.ViewModels;
using Fildo.Droid.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.FullFragging.Fragments;
using MvvmCross.Droid.Support.V7.RecyclerView;
using System.Globalization;

namespace Fildo.Droid.Fragments
{
    public class MainContentView : MvxFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.MainContentView, null);

            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            // ensure the initialization is done

            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            var cultureInfo = ((MainView)this.Activity).CultureInfo;
            if (!string.IsNullOrEmpty(((BaseViewModel)((MainView)this.Activity).ViewModel).PlayingArtist))
            {
                ((MainView)this.Activity).FindViewById<Android.Widget.LinearLayout>(Resource.Id.miniPlayer).Visibility = ViewStates.Visible;
            }

            if (this.ViewModel != null)
            {

                ((BaseViewModel) this.ViewModel).SetCulture(cultureInfo);

                view.FindViewById<Android.Widget.TextView>(Resource.Id.topalbumstextview).Text = ((BaseViewModel) this.ViewModel).GetString("TopAlbums", cultureInfo);
                view.FindViewById<Android.Widget.TextView>(Resource.Id.topartiststextview).Text = ((BaseViewModel) this.ViewModel).GetString("TopArtists", cultureInfo);
                view.FindViewById<Android.Widget.TextView>(Resource.Id.topneteaseplayliststextview).Text = ((BaseViewModel) this.ViewModel).GetString("TopPlaylists", cultureInfo);
            }
            
            /****** SET HORIZONTAL RECYCLERVIEW ******/
            var topalbumsview = view.FindViewById<MvxRecyclerView>(Resource.Id.topalbumsview);
            LinearLayoutManager layoutManager = new LinearLayoutManager(view.Context, LinearLayoutManager.Horizontal, false);
            topalbumsview.SetLayoutManager(layoutManager);
            
            var topartistsview = view.FindViewById<MvxRecyclerView>(Resource.Id.topartistsview);
            LinearLayoutManager layoutManager2 = new LinearLayoutManager(view.Context, LinearLayoutManager.Horizontal, false);
            topartistsview.SetLayoutManager(layoutManager2);

            var topplaylistsview = view.FindViewById<MvxRecyclerView>(Resource.Id.topplaylistsview);
            LinearLayoutManager layoutManager3 = new LinearLayoutManager(view.Context, LinearLayoutManager.Horizontal, false);
            topplaylistsview.SetLayoutManager(layoutManager3);

            var recommendationsview = view.FindViewById<MvxRecyclerView>(Resource.Id.recommendationsview);
            LinearLayoutManager layoutManager4 = new LinearLayoutManager(view.Context, LinearLayoutManager.Horizontal, false);
            recommendationsview.SetLayoutManager(layoutManager4);
            

            topalbumsview.NestedScrollingEnabled = false;
            topartistsview.NestedScrollingEnabled = false;
            topplaylistsview.NestedScrollingEnabled = false;
            recommendationsview.NestedScrollingEnabled = false;

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        public override void OnDetach()
        {
            base.OnDetach();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
        }
    }
}