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
    using Android.Support.V7.Widget;
    using Android.Widget;
    using Core.ViewModels;
    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using Services;

    [Activity(Label = "", ScreenOrientation = ScreenOrientation.Portrait)]
    public class LyricView : MvxAppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        private ToggleButton playpause;
        private ToggleButton shuffle;
        private ToggleButton repeat;
        private AppCompatImageButton btnNext;
        private AppCompatImageButton btnPrev;
        private Bindables.BindableProgress progress;
        private TextView playingSong;
        private TextView lyricContainer;
        public Dictionary<double, string> Lyrics { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.LyricLayout);

            this.toolbar = this.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            
            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);
            this.Lyrics = new Dictionary<double, string>();
            this.playpause = this.FindViewById<ToggleButton>(Resource.Id.btnPlayPause);
            this.btnNext = this.FindViewById<AppCompatImageButton>(Resource.Id.btnNext);
            this.btnPrev = this.FindViewById<AppCompatImageButton>(Resource.Id.btnPrevious);
            this.shuffle = this.FindViewById<ToggleButton>(Resource.Id.btnShuffle);
            this.repeat = this.FindViewById<ToggleButton>(Resource.Id.btnRepeat);
            this.playingSong = this.FindViewById<TextView>(Resource.Id.playingSong);
            this.lyricContainer = this.FindViewById<TextView>(Resource.Id.lyricContainer);
            BackgroundStreamingService.posChanged += this.BackgroundStreamingService_posChanged;
            if (BackgroundStreamingService.Player == null)
            {
                this.playpause.Checked = false;
                this.shuffle.Checked = false;
                this.repeat.Checked = false;
                
            }
            else
            {
                BackgroundStreamingService.SongNameChanged += this.BackgroundStreamingService_SongName;
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

            this.progress = new Bindables.BindableProgress(this, this.ViewModel);

            var set = this.CreateBindingSet<LyricView, LyricViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Apply();
            
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
                    if (Math.Abs(item.Key - temp.Last().Key) > 0.1)
                    {
                        result += item.Value + "<br>";
                    }
                    else
                    {
                        result += "<font color='#914700'><big>" + item.Value + "</big></font><br>";
                    }
                }
                this.RunOnUiThread(() =>
                {
                    this.lyricContainer.SetText(Android.Text.Html.FromHtml(result), TextView.BufferType.Spannable);
                });
            }

        }

        private void BackgroundStreamingService_SongName(object sender, string e)
        {
            this.playingSong.Text = e;
            this.SetLyric();
        }
        private async void SetLyric()
        {
            var songId = BackgroundStreamingService.GetCurrentSong();
            var lyricTemp = await ((LyricViewModel) this.ViewModel).GetLyric(songId);
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
                    result += tempPart[1].Replace("[","") + "\n";
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
            var a = BackgroundStreamingService.GetCurrentSong();
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
                return;
            if (e.IsChecked)
            {
                var intent = new Intent(BackgroundStreamingService.ActionPlay);
                Application.Context.StartService(intent);
            }
            else
            {
                var intent = new Intent(BackgroundStreamingService.ActionPause);
                Application.Context.StartService(intent);
            }
        }
    }
}