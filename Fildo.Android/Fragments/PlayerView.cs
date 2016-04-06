namespace Fildo.Droid.Fragments
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Support.Design.Widget;
    using Android.Support.V4.Widget;
    using Android.Support.V7.Widget;
    using Android.Text;
    using Android.Views;
    using Android.Widget;
    using Bindables;
    using Core.IPlatform;
    using Core.ViewModels;
    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Binding.Droid.BindingContext;
    using MvvmCross.Droid.FullFragging.Fragments;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using MvvmCross.Droid.Support.V7.RecyclerView;
    using MvvmCross.Platform;
    using Services;
    using Views;
    using Toolbar = Android.Support.V7.Widget.Toolbar;

    [Activity(Label = "Player Queue / Lyrics", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PlayerView : MvxFragment
    {
        private AppCompatImageButton btnNext;
        private AppCompatImageButton btnPrev;
        private CultureInfo cultureInfo;
        private RecyclerView listQueue;
        private TextView lyricContainer;
        private NestedScrollView lyricScroll;
        private TextView playingSong;
        private ToggleButton playpause;
        private BindableProgress progress;
        private ToggleButton repeat;
        private ToggleButton shuffle;

        public Dictionary<double, string> Lyrics { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.PlayerQueue, null);

            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.cultureInfo = ((MainView) this.Activity).CultureInfo;

            this.Lyrics = new Dictionary<double, string>();

            this.playpause = view.FindViewById<ToggleButton>(Resource.Id.btnPlayPause);
            this.btnNext = view.FindViewById<AppCompatImageButton>(Resource.Id.btnNext);
            this.btnPrev = view.FindViewById<AppCompatImageButton>(Resource.Id.btnPrevious);
            this.shuffle = view.FindViewById<ToggleButton>(Resource.Id.btnShuffle);
            this.repeat = view.FindViewById<ToggleButton>(Resource.Id.btnRepeat);
            this.playingSong = view.FindViewById<TextView>(Resource.Id.playingSong);
            this.lyricScroll = view.FindViewById<NestedScrollView>(Resource.Id.lyricScroll);
            this.lyricContainer = view.FindViewById<TextView>(Resource.Id.lyricContainer);
            this.listQueue = view.FindViewById<MvxRecyclerView>(Resource.Id.listQueue);

            ((MainView) this.Activity).FindViewById<LinearLayout>(Resource.Id.miniPlayer).Visibility = ViewStates.Gone;
            this.lyricScroll.Visibility = ViewStates.Gone;

            BackgroundStreamingService.posChanged -= this.BackgroundStreamingService_posChanged;
            BackgroundStreamingService.percentChanged -= this.BackgroundStreamingService_percentChanged;
            BackgroundStreamingService.posChanged += this.BackgroundStreamingService_posChanged;
            BackgroundStreamingService.percentChanged += this.BackgroundStreamingService_percentChanged;
            if (BackgroundStreamingService.Player == null)
            {
                this.playpause.Checked = false;
                this.shuffle.Checked = false;
                this.repeat.Checked = false;
            }
            else
            {
                try
                {
                    BackgroundStreamingService.SongNameChanged -= this.BackgroundStreamingService_SongName;
                    BackgroundStreamingService.SongNameChanged += this.BackgroundStreamingService_SongName;
                }
                catch (Exception)
                {
                }
                this.playingSong.Text = BackgroundStreamingService.SongName;
                this.SetLyric();
                if (BackgroundStreamingService.Player.PlayWhenReady)
                {
                    this.playpause.Checked = true;
                }
                if (BackgroundStreamingService.IsRepeat)
                {
                    this.repeat.Checked = true;
                }
                if (BackgroundStreamingService.IsShuffle)
                {
                    this.shuffle.Checked = true;
                }
            }

            this.playpause.CheckedChange += this.Playpause_CheckedChange;
            this.btnNext.Click += this.BtnNext_Click;
            this.btnPrev.Click += this.BtnPrev_Click;
            this.shuffle.CheckedChange += this.Shuffle_CheckedChange;
            this.repeat.CheckedChange += this.Repeat_CheckedChange;

            TabLayout tabs = view.FindViewById<TabLayout>(Resource.Id.tabs);
            tabs.Visibility = ViewStates.Visible;
            tabs.AddTab(tabs.NewTab().SetText("QUEUE"));
            tabs.AddTab(tabs.NewTab().SetText("LYRICS"));
            tabs.TabSelected += this.Tabs_TabSelected;

            if (this.ViewModel != null)
            {

            }
            else
            {
                
            }

            this.progress = new BindableProgress(view.Context, this.ViewModel);

            var set = this.CreateBindingSet<PlayerView, PlayerViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Apply();
            this.SetHasOptionsMenu(true);
            AppCompatSeekBar seekbar = view.FindViewById<AppCompatSeekBar>(Resource.Id.seekBar);
            seekbar.ProgressChanged += this.Seekbar_ProgressChanged;

            GAService.GetGASInstance().Track_App_Page("Player");

            return view;
        }

        private void Seekbar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                long ms = BackgroundStreamingService.Player.Duration*e.Progress/100;
                BackgroundStreamingService.Player.SeekTo(ms);
            }
        }

        private void BackgroundStreamingService_percentChanged(object sender, int e)
        {
            if (this.ViewModel != null)
            {
                ((BaseViewModel)this.ViewModel).PlayingArtistPercent = e;
            }
        }

        private void Tabs_TabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            if (e.Tab.Position == 0)
            {
                this.lyricScroll.Visibility = ViewStates.Gone;
                this.listQueue.Visibility = ViewStates.Visible;
            }
            else
            {
                this.listQueue.Visibility = ViewStates.Gone;
                this.lyricScroll.Visibility = ViewStates.Visible;
            }
        }

        private void BackgroundStreamingService_posChanged(object sender, long e)
        {
            var temp = this.Lyrics.Where(p => p.Key < e).ToList();
            string result = string.Empty;
            if (temp.Count > 0)
            {
                //Log.Error("Error", temp.Last().Value);
                foreach (var item in this.Lyrics)
                {
                    if (item.Key != temp.Last().Key)
                    {
                        result += item.Value + "<br>";
                    }
                    else
                    {
                        result += "<font color='#914700'><big>" + item.Value + "</big></font><br>";
                    }
                }
                ((MainView) this.Activity).RunOnUiThread(
                    () =>
                    {
                        this.lyricContainer.SetText(Html.FromHtml(result), TextView.BufferType.Spannable);
                    });
            }
        }

        private void BackgroundStreamingService_SongName(object sender, string e)
        {
            try
            {
                if (this.Activity != null)
                {
                    ((MainView) this.Activity).FindViewById<LinearLayout>(Resource.Id.miniPlayer).Visibility =
                        ViewStates.Gone;
                    this.playingSong.Text = e;
                    this.SetLyric();
                }
            }
            catch (Exception)
            {
            }
        }

        private async void SetLyric()
        {
            if (this.ViewModel == null)
            {
                return;
            }

            var songId = BackgroundStreamingService.GetCurrentSong();
            var lyricTemp = await ((PlayerViewModel) this.ViewModel).GetLyric(songId);
            var parts = lyricTemp.Split('\n');
            string result = string.Empty;
            this.Lyrics = new Dictionary<double, string>();
            foreach (var part in parts)
            {
                var tempPart = part.Split(']');
                if (tempPart.Length > 1)
                {
                    var clean = tempPart[0].Replace("[", "");
                    TimeSpan timeSpan;
                    if (TimeSpan.TryParseExact(clean, @"mm\:ss\.fff", CultureInfo.CurrentCulture, out timeSpan))
                    {
                        this.Lyrics.Add(timeSpan.TotalMilliseconds, tempPart[1].Replace("[", ""));
                    }
                    result += tempPart[1].Replace("[", "") + "\n";
                }
                else
                {
                    result += tempPart[0].Replace("[", "") + "\n";
                }
            }
            this.lyricContainer.Text = result;
        }

        private void Repeat_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            BackgroundStreamingService.IsRepeat = e.IsChecked;
        }

        private void Shuffle_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            BackgroundStreamingService.IsShuffle = e.IsChecked;
        }

        private void BtnPrev_Click(object sender, EventArgs e)
        {
            var intent = new Intent(BackgroundStreamingService.ActionPrev);
            Application.Context.StartService(intent);
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            var intent = new Intent(BackgroundStreamingService.ActionNext);
            Application.Context.StartService(intent);
        }

        private void Playpause_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (BackgroundStreamingService.Player == null)
            {
                return;
            }
            if (e.IsChecked)
            {
                var intent = new Intent(BackgroundStreamingService.ActionPause);
                Application.Context.StartService(intent);
            }
            else
            {
                var intent = new Intent(BackgroundStreamingService.ActionPause);
                Application.Context.StartService(intent);
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Layout.SavePLMenu, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.SavePL)
            {
                var vm = (PlayerViewModel) this.ViewModel;
                var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
                string IdUser = prefs.GetString("IdUser", string.Empty);
                string UserHash = prefs.GetString("UserHash", string.Empty);

                if (string.IsNullOrEmpty(UserHash))
                {
                    Mvx.Resolve<IDialog>().ShowAlert("You must be logged to use this.", 5000);
                }
                else
                {
                    vm.SavePL(IdUser, UserHash);
                }

                return true;
            }

            return true;
        }
    }
}