namespace Fildo.Droid.Fragments
{
    using System;
    using System.Globalization;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Gms.Ads;
    using Android.OS;
    using Android.Support.Design.Widget;
    using Android.Support.V4.Widget;
    using Android.Support.V7.Widget;
    using Android.Util;
    using Android.Views;
    using Core.ViewModels;

    using Fildo.Droid.Services;

    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Binding.Droid.BindingContext;
    using MvvmCross.Binding.Droid.Views;
    using MvvmCross.Droid.FullFragging.Fragments;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using MvvmCross.Droid.Support.V7.RecyclerView;
    using MvvmCross.Platform;
    using Views;

    [Activity(Label = "Songs & Albums", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ListSongView : MvxFragment
    {
        private MvxRecyclerView songList;
        private MvxRecyclerView albumList;
        private MvxRecyclerView similarList;
        private Bindables.BindableProgress progress;
        //private FloatingActionButton fab;
        private CultureInfo cultureInfo;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.ListSong, null);

            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.cultureInfo = ((MainView)this.Activity).CultureInfo;

            if (!string.IsNullOrEmpty(((BaseViewModel)((MainView)this.Activity).ViewModel).PlayingArtist))
            {
                ((MainView)this.Activity).FindViewById<Android.Widget.LinearLayout>(Resource.Id.miniPlayer).Visibility = ViewStates.Visible;
            }
            //AndroidEnvironment.UnhandledExceptionRaiser -= HandleAndroidException;
            //AndroidEnvironment.UnhandledExceptionRaiser += HandleAndroidException;
            TabLayout tabs = view.FindViewById<TabLayout>(Resource.Id.tabs);
            if (this.ViewModel != null)
            {
                ((BaseViewModel)((MainView)this.Activity).ViewModel).PicUrl = ((ListSongViewModel) this.ViewModel).PicUrl;
                string image = ((ListSongViewModel) this.ViewModel).PicUrl;
                ((MainView)this.Activity).FindViewById<MvxImageView>(Resource.Id.artistImageTest).ImageUrl = image;
                ListSongViewModel listSongViewModel = (ListSongViewModel) this.ViewModel;
                if (listSongViewModel.IsArtist)
                {
                    tabs.Visibility = ViewStates.Visible;
                    tabs.AddTab(tabs.NewTab().SetText("SONGS"));
                    tabs.AddTab(tabs.NewTab().SetText("ALBUMS"));
                    tabs.AddTab(tabs.NewTab().SetText("SIMILAR"));
                    tabs.TabSelected += this.Tabs_TabSelected;
                }
                else
                {
                    tabs.Visibility = ViewStates.Gone;
                }
            }

            AppBarLayout appBarLayout = ((MainView)this.Activity).FindViewById<AppBarLayout>(Resource.Id.appbar);
            CollapsingToolbarLayout collapsingToolbarLayout = ((MainView)this.Activity).FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            float density = this.Resources.DisplayMetrics.Density;
            
            float heightDp = 200 * density;
            CoordinatorLayout.LayoutParams lp = (CoordinatorLayout.LayoutParams)appBarLayout.LayoutParameters;
            lp.Height = (int)heightDp;
            appBarLayout.SetExpanded(true);

            
            //this.fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            //var collapsibleToolbar = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            this.SetHasOptionsMenu(true);
            this.progress = new Bindables.BindableProgress(view.Context, this.ViewModel);
            var set = this.CreateBindingSet<ListSongView, ListSongViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(collapsingToolbarLayout).For(p => p.Title).To(vm => vm.TitleView);

            //set.Bind(collapsibleToolbar).For(p => p.Title).To(vm => vm.TitleView);
            set.Apply();

            this.songList = view.FindViewById<MvxRecyclerView>(Resource.Id.SongList);
            this.albumList = view.FindViewById<MvxRecyclerView>(Resource.Id.AlbumList);
            this.similarList = view.FindViewById<MvxRecyclerView>(Resource.Id.SimilarList);
            this.similarList.Visibility = ViewStates.Gone;
            this.albumList.Visibility = ViewStates.Gone;

            DisplayMetrics displayMetrics = this.Resources.DisplayMetrics;
            float dpWidth = displayMetrics.WidthPixels / displayMetrics.Density;
            var columnFloat = dpWidth / 160;
            GridLayoutManager lLayout = new GridLayoutManager(view.Context, (int)Math.Truncate(columnFloat));
            GridLayoutManager lLayout2 = new GridLayoutManager(view.Context, (int)Math.Truncate(columnFloat));

            this.albumList.HasFixedSize = true;
            this.albumList.SetLayoutManager(lLayout);
            this.similarList.HasFixedSize = true;
            this.similarList.SetLayoutManager(lLayout2);

            GAService.GetGASInstance().Track_App_Page("List Songs");
            return view;
        }

        private void Tabs_TabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            if (e.Tab.Position == 0)
            {
                this.songList.Visibility = ViewStates.Visible;
                this.albumList.Visibility = ViewStates.Gone;
                this.similarList.Visibility = ViewStates.Gone;
                //fab.Visibility = Android.Views.ViewStates.Visible;
            }
            else if (e.Tab.Position == 1)
            {
                this.songList.Visibility = ViewStates.Gone;
                this.albumList.Visibility = ViewStates.Visible;
                this.similarList.Visibility = ViewStates.Gone;
                //fab.Visibility = Android.Views.ViewStates.Gone;
            }
            else
            {
                this.songList.Visibility = ViewStates.Gone;
                this.albumList.Visibility = ViewStates.Gone;
                this.similarList.Visibility = ViewStates.Visible;
            }
        }
        
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.DownloadAll)
            {
                var vm = (ListSongViewModel) this.ViewModel;
                vm.DownloadAll();
                return true;
            }
            if (item.ItemId == Resource.Id.PlayAll)
            {
                var vm = (ListSongViewModel) this.ViewModel;
                vm.PlayAll();
                return true;
            }
            if (item.ItemId == Resource.Id.SavePL)
            {
                var vm = (ListSongViewModel) this.ViewModel;
                var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
                string IdUser = prefs.GetString("IdUser", string.Empty);
                string UserHash = prefs.GetString("UserHash", string.Empty);

                if (string.IsNullOrEmpty(UserHash))
                {
                    Mvx.Resolve<Core.IPlatform.IDialog>().ShowAlert("You must be logged to use this.", 5000);
                }
                else
                {
                    vm.SavePL(IdUser, UserHash);
                }
                
                return true;
            }
            
            return true;
        }


        /*public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            if (!menu.HasVisibleItems)
            {
                MenuInflater.Inflate(Resource.Layout.DownloadAllMenu, menu);
            }
           
            return base.OnPrepareOptionsMenu(menu);
        }*/

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (this.ViewModel == null)
            {
                return;
            }

            if (((ListSongViewModel) this.ViewModel).IsPlaylist())
            {
                inflater.Inflate(Resource.Layout.DownloadAllMenuPlaylist, menu);
            }
            else
            {
                inflater.Inflate(Resource.Layout.DownloadAllMenu, menu);
            }
        }

        public override void OnPause()
        {
            AppBarLayout appBarLayout = ((MainView)this.Activity).FindViewById<AppBarLayout>(Resource.Id.appbar);
            CollapsingToolbarLayout collapsingToolbarLayout = ((MainView)this.Activity).FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            float density = this.Resources.DisplayMetrics.Density;

            float heightDp = 56 * density;
            CoordinatorLayout.LayoutParams lp = (CoordinatorLayout.LayoutParams)appBarLayout.LayoutParameters;
            lp.Height = (int)heightDp;
            appBarLayout.SetExpanded(false);
            base.OnPause();
        }

        public override void OnDetach()
        {
            base.OnDetach();
        }
    }
}