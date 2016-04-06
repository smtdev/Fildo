namespace Fildo.Droid.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;
    using Android.Media;
    using Android.Net;
    using Android.Net.Wifi;
    using Android.OS;
    using Android.Support.V4.App;

    using Com.Google.Android.Exoplayer;

    using Fildo.Core;
    using Fildo.Core.Entities;
    using Fildo.Core.IPlatform;
    using Fildo.Droid.Receivers;
    using Fildo.Droid.Views;

    using MvvmCross.Platform;

    using Uri = Android.Net.Uri;

    [Service]
    [IntentFilter(new[] { ActionPlay, ActionPause, ActionStop, ActionNext, ActionPrev, ActionPlayWithoutClear, ActionClose })]
    public class BackgroundStreamingService : Service, AudioManager.IOnAudioFocusChangeListener, IExoPlayerListener
    {
        //Actions
        public const string ActionPlay = "net.fildo.app.action.PLAY";
        public const string ActionPause = "net.fildo.app.action.PAUSE";
        public const string ActionStop = "net.fildo.app.action.STOP";
        public const string ActionNext = "net.fildo.app.action.NEXT";
        public const string ActionPrev = "net.fildo.app.action.PREV";
        public const string ActionClose = "net.fildo.app.action.CLOSE";
        public const string ActionPlayWithoutClear = "net.fildo.app.action.PLAYWITHOUT";
        private const int NotificationId = 600223194;
        public static Context Main;
        public static string CurrentTrack;
        public static string CurrentTrackId;
        public static IExoPlayer Player;
        public static bool IsShuffle;
        public static bool IsRepeat;

        public static string SongName;
        private AudioManager audioManager;

        private bool paused;
        private bool prevPlay;
        private ComponentName remoteComponentName;
        private RemoteControlClient remoteControlClient;
        private bool starting;
        private WifiManager.WifiLock wifiLock;
        private WifiManager wifiManager;
        public BackgroundStreamingService()
        {
            Queue.CollectionChanged += this.Queue_CollectionChanged;
        }

        public static ObservableCollection<Song> Queue { get; set; } = new ObservableCollection<Song>();

        public void OnPlayWhenReadyCommitted()
        {
            // Nothing to do.
        }

        public void OnPlayerError(ExoPlaybackException p0)
        {
            this.Next(true);
        }

        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            if (playbackState == ExoPlayer.StateEnded)
            {
                this.Next(true);
            }
            switch (playbackState)
            {
                case ExoPlayer.StateBuffering:
                    break;
                case ExoPlayer.StateEnded:
                    break;
                case ExoPlayer.StateIdle:
                    break;
                case ExoPlayer.StatePreparing:
                    break;
                case ExoPlayer.StateReady:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// For a good user experience we should account for when audio focus has changed.
        /// There is only 1 audio output there may be several media services trying to use it so
        /// we should act correctly based on this.  "duck" to be quiet and when we gain go full.
        /// All applications are encouraged to follow this, but are not enforced.
        /// </summary>
        /// <param name="focusChange"></param>
        public void OnAudioFocusChange(AudioFocus focusChange)
        {
            try
            {
                switch (focusChange)
                {
                    case AudioFocus.Gain:
                        if (Player == null)
                        {
                            this.IntializePlayer();
                        }

                        if (!Player.PlayWhenReady && !this.starting && this.prevPlay)
                        {
                            Player.PlayWhenReady = true;
                            this.paused = false;
                        }

                        //Player.SetVolume(1.0f, 1.0f);//Turn it up!
                        break;
                    case AudioFocus.Loss:
                        //We have lost focus stop!
                        this.prevPlay = Player?.PlayWhenReady ?? false;
                        this.Pause();
                        break;
                    case AudioFocus.LossTransient:
                        //We have lost focus for a short time, but likely to resume so pause
                        this.prevPlay = Player?.PlayWhenReady ?? false;
                        this.Pause();
                        break;
                    case AudioFocus.LossTransientCanDuck:
                        //We have lost focus but should till play at a muted 10% volume
                        if (Player.PlayWhenReady)
                        {
                        }
                        //Player.SetVolume(.1f, .1f);//turn it down!
                        break;
                }
            }
            catch 
            {
                // Ignored
            }
        }

        public static event EventHandler QueueChanged;
        public static event EventHandler<string> SongNameChanged;
        public static event EventHandler<long> posChanged;
        public static event EventHandler<int> percentChanged;
        public static event EventHandler<int> prepared;

        private void Queue_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Queue.Count == 0)
            {
                CurrentTrack = null;
                CurrentTrackId = null;
            }
        }

        public static string GetCurrentSong()
        {
            if (string.IsNullOrEmpty(CurrentTrack) || (Queue == null) || (Queue.Count == 0))
            {
                return "0";
            }
            Song currentSong = Queue.FirstOrDefault(p => p.Url == CurrentTrack);
            if (currentSong != null && CurrentTrack == null)
            {
                return currentSong.Id;
            }

            currentSong = Queue.FirstOrDefault(p => p.Id == CurrentTrackId);
            if (currentSong != null)
            {
                return currentSong.Id;
            }

            return "0";
        }

        public override void OnCreate()
        {
            base.OnCreate();
            //Find our audio and notificaton managers
            this.audioManager = (AudioManager)this.GetSystemService(AudioService);
            this.wifiManager = (WifiManager)this.GetSystemService(WifiService);

            this.remoteComponentName = new ComponentName(
                this.PackageName,
                new RemoteControlBroadcastReceiver().ComponentName);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public void ResetQueue()
        {
            Queue.Clear();
        }

        public void Add(Song song)
        {
            Queue.Add(song);
            if (!Player.PlayWhenReady)
            {
                CurrentTrack = song.Url;
                CurrentTrackId = song.Id;
                this.Play();
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                if (intent != null)
                {
                    switch (intent.Action)
                    {
                        case ActionPlay:
                            this.Play();
                            break;
                        case ActionStop:
                            this.Stop();
                            break;
                        case ActionPause:
                            this.Pause();
                            break;
                        case ActionNext:
                            this.Next(false);
                            break;
                        case ActionPrev:
                            this.Prev(false);
                            break;
                        case ActionPlayWithoutClear:
                            this.PlayWithoutClear();
                            break;
                        case ActionClose:
                            this.StopSelf();
                            break;
                    }

                    //Set sticky as we are a long running operation
                    return StartCommandResult.Sticky;
                }
                return StartCommandResult.NotSticky;
            }
            catch (Exception)
            {
                return StartCommandResult.NotSticky;
            }
        }

        private void Next(bool autoNext)
        {
            if (Player == null)
            {
                return;
            }

            var currentSong = Queue.FirstOrDefault(p => p.Url == CurrentTrack);
            if (currentSong == null || CurrentTrack == null)
            {
                currentSong = Queue.FirstOrDefault(p => p.Id == CurrentTrackId);
            }

            if (currentSong != null)
            {
                int currentPosition = Queue.IndexOf(currentSong);
                if (currentPosition > -1)
                {
                    if (Queue.Count > currentPosition + 1)
                    {
                        if (IsShuffle)
                        {
                            Random rnd = new Random();
                            int r = rnd.Next(Queue.Count);
                            CurrentTrack = Queue[r].Url;
                            CurrentTrackId = Queue[r].Id;
                            this.Play();
                        }
                        else
                        {
                            CurrentTrack = Queue[currentPosition + 1].Url;
                            CurrentTrackId = Queue[currentPosition + 1].Id;
                            this.Play();
                        }
                    }
                    else if (IsRepeat)
                    {
                        CurrentTrack = Queue[0].Url;
                        CurrentTrackId = Queue[0].Id;
                    }
                    else
                    {
                        if (autoNext)
                        {
                            this.Stop();
                            this.StopSelf();
                        }
                        else
                        {
                            Mvx.Resolve<IDialog>().ShowAlert("No more songs in queue", 5000);
                        }
                    }
                }
                else
                {
                    this.Stop();
                }
            }
            else
            {
                this.Stop();
            }
        }

        private void Prev(bool autoPrev)
        {
            if (Player == null)
            {
                return;
            }
            var currentSong = Queue.FirstOrDefault(p => p.Url == CurrentTrack);
            if (currentSong == null || CurrentTrack == null)
            {
                currentSong = Queue.FirstOrDefault(p => p.Id == CurrentTrackId);
            }
            if (currentSong != null)
            {
                int currentPosition = Queue.IndexOf(currentSong);
                if ((currentPosition > -1) && (currentPosition - 1 > -1))
                {
                    CurrentTrack = Queue[currentPosition - 1].Url;
                    CurrentTrackId = Queue[currentPosition - 1].Id;
                    this.Play();
                }
                else
                {
                    if (autoPrev)
                    {
                        this.Stop();
                    }
                    else
                    {
                        Mvx.Resolve<IDialog>().ShowAlert("No previous songs in queue", 5000);
                    }
                }
            }
            else
            {
                this.Stop();
            }
        }

        private void IntializePlayer()
        {
            Player = ExoPlayerFactory.NewInstance(1);

            //Tell our player to sream music
            //ExoPlayer.SetAudioStreamType(Stream.Music);

            //Wake mode will be partial to keep the CPU still running under lock screen
            //ExoPlayer.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);

            //When we have prepared the song start playback
            /*ExoPlayer.Prepared += (sender, args) => {
                if (remoteControlClient != null)
                {
                    remoteControlClient.SetPlaybackState(RemoteControlPlayState.Playing);
                }
                if (prepared != null)
                {
                    prepared(this, 0);
                }
                UpdateMetadata();
                starting = false;
                Player.Start();
            };*/
            //TODO EXOPLAYER

            Task.Run(
                async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                if ((posChanged != null) && (Player != null))
                                {
                                    if (Player.CurrentPosition > 0 && Player.Duration > 0)
                                    {
                                        posChanged(this, Player.CurrentPosition);
                                    }
                                }
                                if ((percentChanged != null) && (Player != null))
                                {
                                    if (Player.CurrentPosition > 0 && Player.Duration > 0)
                                    {
                                        int percent = (int)(Player.CurrentPosition * 100 / Player.Duration);
                                        percentChanged(this, percent);
                                    }
                                }
                                await Task.Delay(100);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    });

            Player.AddListener(this);
            //When we have reached the end of the song stop ourselves, however you could signal next track here.
            /*
            Player.Error += async (sender, args) =>
            {
                Player.Reset();
                await Task.Delay(500);
                if (Player.IsPlaying)
                {
                    return;
                }
                if (errorCounter <3)
                {
                    errorCounter++;
                    this.Play();
                }
                else
                {
                    errorCounter = 0;
                    if (remoteControlClient != null)
                        remoteControlClient.SetPlaybackState(RemoteControlPlayState.Error);
                    //playback error
                    Console.WriteLine("Error in playback resetting: " + args.What);
                    Stop();//this will clean up and reset properly.
                    Next();
                }
            };
            */
        }

        private void PlayWithoutClear()
        {
            this.Play();
        }

        private async void Play()
        {
            try
            {
                if ((CurrentTrack == null) && (CurrentTrackId == null))
                {
                    if (Queue.Count > 0)
                    {
                        Song firstOrDefault = Queue.FirstOrDefault();
                        if (firstOrDefault != null)
                        {
                            CurrentTrack = firstOrDefault.Url;
                            CurrentTrackId = firstOrDefault.Id;
                        }
                        else
                        {
                            CurrentTrack = null;
                            CurrentTrackId = null;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                Song currentSong = Queue.FirstOrDefault(p => p.Url == CurrentTrack);
                if (currentSong == null || CurrentTrack == null)
                {
                    currentSong = Queue.FirstOrDefault(p => p.Id == CurrentTrackId);
                }

                Player?.Stop();

                Player = null;

                if (this.paused && Player != null)
                {
                    this.paused = false;
                    //We are simply paused so just start again
                    Player.PlayWhenReady = true;
                    if (currentSong != null)
                    {
                        if (!string.IsNullOrEmpty(currentSong.Artist) && !string.IsNullOrEmpty(currentSong.Title))
                        {
                            this.StartForeground(currentSong.Artist + " - " + currentSong.Title);
                        }
                        else if (!string.IsNullOrEmpty(currentSong.Artist))
                        {
                            this.StartForeground(currentSong.Artist);
                        }
                        else if (!string.IsNullOrEmpty(currentSong.Title))
                        {
                            this.StartForeground(currentSong.Title);
                        }
                    }

                    this.RegisterRemoteClient();
                    this.remoteControlClient.SetPlaybackState(RemoteControlPlayState.Playing);
                    this.UpdateMetadata();
                    return;
                }

                if (Player == null)
                {
                    this.IntializePlayer();
                }

                if (Player.PlayWhenReady)
                {
                    this.Stop();
                }
                else
                {
                    //Player.Reset();
                    this.paused = false;
                    this.StopForeground(true);
                    this.ReleaseWifiLock();
                }

                this.starting = true;
                var netEase = Mvx.Resolve<INetEase>();
                var newSong = new SongNetease
                                  {
                                      Artist = currentSong.Artist,
                                      Title = currentSong.Title,
                                      Url = CurrentTrack,
                                      Id = currentSong.Id
                                  };


                if (string.IsNullOrEmpty(newSong.Url))
                {
                    var tempsong = await netEase.GetSong(newSong.Id);
                    if (tempsong != null)
                    {
                        tempsong.Id = newSong.Id;
                        tempsong.Title = newSong.Title;
                        tempsong.Artist = newSong.Artist;
                        newSong = tempsong;
                    }
                }


                var currenTrackToPlay = (await netEase.FixUrl(newSong, true)).Url;
                Dictionary<string, string> headers = new Dictionary<string, string>();
                if (currenTrackToPlay.StartsWith("http://221.228.64.228/"))
                {
                    headers.Add("Host", "m1.music.126.net");
                }
                headers.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");

                var trackUrl = Uri.Parse(currenTrackToPlay);
                FrameworkSampleSource sampleSource = new FrameworkSampleSource(Main, trackUrl, headers);

                TrackRenderer aRenderer = new MediaCodecAudioTrackRenderer(sampleSource, MediaCodecSelector.Default);

                if (QueueChanged != null)
                {
                    QueueChanged(this, new EventArgs());
                }

                var focusResult = this.audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
                if (focusResult != AudioFocusRequest.Granted)
                {
                    //could not get audio focus
                    Console.WriteLine("Could not get audio focus");
                }

                Player.Prepare(aRenderer);
                Player.PlayWhenReady = true;

                this.AquireWifiLock();

                if (currentSong != null)
                {
                    if (!string.IsNullOrEmpty(currentSong.Artist) && !string.IsNullOrEmpty(currentSong.Title))
                    {
                        this.StartForeground(currentSong.Artist + " - " + currentSong.Title);
                    }
                    else if (!string.IsNullOrEmpty(currentSong.Artist))
                    {
                        this.StartForeground(currentSong.Artist);
                    }
                    else if (!string.IsNullOrEmpty(currentSong.Title))
                    {
                        this.StartForeground(currentSong.Title);
                    }
                }

                this.RegisterRemoteClient();
                this.remoteControlClient.SetPlaybackState(RemoteControlPlayState.Buffering);
                this.UpdateMetadata();
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// When we start on the foreground we will present a notification to the user
        /// When they press the notification it will take them to the main page so they can control the music
        /// </summary>
        private void StartForeground(string detail)
        {
            SongNameChanged?.Invoke(this, detail);
            SongName = detail;

            var intent = new Intent(this.ApplicationContext, typeof(MainView));
            intent.PutExtra("notificationPlayList", "player");
            var pendingIntent = PendingIntent.GetActivity(
                this.ApplicationContext,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent);

            // use System.currentTimeMillis() to have a unique ID for the pending intent
            PendingIntent pIntent = PendingIntent.GetActivity(this, NotificationId, intent, 0);

            var intentNext = new Intent(ActionNext);
            PendingIntent pIntentNext = PendingIntent.GetService(this, NotificationId + 1, intentNext, 0);

            var intentPlayPause = new Intent(ActionPause);
            PendingIntent pIntentPlayPause = PendingIntent.GetService(this, NotificationId + 2, intentPlayPause, 0);

            var intentClose = new Intent(ActionClose);
            PendingIntent pIntentClose = PendingIntent.GetService(this, NotificationId + 2, intentClose, 0);

            string[] details = detail.Split(new string[] { " - " }, 2, StringSplitOptions.None);
            string dataToIntent = detail;
            string dataToIntent2 = string.Empty;
            if (details.Length == 2)
            {
                dataToIntent = details[0];
                dataToIntent2 = details[1];
            }
            
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this).SetContentIntent(pendingIntent)
                // Start 2nd activity when the intent is clicked.
                .SetSmallIcon(Resource.Drawable.Icon) // Display this icon
                .AddAction(Resource.Drawable.play3, string.Empty, pIntentPlayPause)
                .AddAction(Resource.Drawable.nextb, string.Empty, pIntentNext)
                .AddAction(0, "Close", pIntentClose)
                //.SetAutoCancel(true)                    // Dismiss from the notif. area when clicked
                .SetContentTitle(dataToIntent) // Set its title
                .SetContentText(dataToIntent2); // The message to display.

            Notification notification = builder.Build();

            //var notification = new Notification
            //{
            //    TickerText = new Java.Lang.String("Song started!"),
            //    Icon = Resource.Drawable.Icon
            //};
            notification.Flags |= NotificationFlags.OngoingEvent;
            //notification.SetLatestEventInfo(ApplicationContext, "Fildo Streaming", detail, pendingIntent);

            this.StartForeground(NotificationId, notification);
        }

        private void Pause()
        {
            try
            {
                if (Player == null)
                {
                    return;
                }

                if (Player.PlayWhenReady)
                {
                    Player.PlayWhenReady = false;
                    this.remoteControlClient.SetPlaybackState(RemoteControlPlayState.Paused);
                    this.paused = true;
                }
                else
                {
                    Player.PlayWhenReady = true;
                    this.remoteControlClient.SetPlaybackState(RemoteControlPlayState.Playing);
                    Song currentSong = Queue.FirstOrDefault(p => p.Url == CurrentTrack);
                    if (currentSong == null || CurrentTrack == null)
                    {
                        currentSong = Queue.FirstOrDefault(p => p.Id == CurrentTrackId);
                    }

                    if (currentSong != null)
                    {
                        if (!string.IsNullOrEmpty(currentSong.Artist) && !string.IsNullOrEmpty(currentSong.Title))
                        {
                            this.StartForeground(currentSong.Artist + " - " + currentSong.Title);
                        }
                        else if (!string.IsNullOrEmpty(currentSong.Artist))
                        {
                            this.StartForeground(currentSong.Artist);
                        }
                        else if (!string.IsNullOrEmpty(currentSong.Title))
                        {
                            this.StartForeground(currentSong.Title);
                        }
                    }
                    this.paused = true;
                }
            }
            catch
            {
                // ignored
            }
        }

        private void Stop()
        {
            if (Player == null)
            {
                return;
            }

            if (Player.PlayWhenReady)
            {
                Player.Stop();
                if (this.remoteControlClient != null)
                {
                    this.remoteControlClient.SetPlaybackState(RemoteControlPlayState.Stopped);
                }
            }

            //Player.Reset();
            this.paused = false;
            this.StopForeground(true);
            this.ReleaseWifiLock();
            this.UnregisterRemoteClient();
        }

        /// <summary>
        /// Lock the wifi so we can still stream under lock screen
        /// </summary>
        private void AquireWifiLock()
        {
            if (this.wifiLock == null)
            {
                this.wifiLock = this.wifiManager.CreateWifiLock(WifiMode.Full, "fildo_wifi_lock");
            }
            this.wifiLock.Acquire();
        }

        /// <summary>
        /// This will release the wifi lock if it is no longer needed
        /// </summary>
        private void ReleaseWifiLock()
        {
            if (this.wifiLock == null)
            {
                return;
            }

            this.wifiLock.Release();
            this.wifiLock = null;
        }

        /// <summary>
        /// Properly cleanup of your player by releasing resources
        /// </summary>
        public override void OnDestroy()
        {
            var activityManager = (ActivityManager)this.ApplicationContext.GetSystemService(ActivityService);
            var lastTasks = activityManager.GetRunningTasks(1);
            if (lastTasks?.FirstOrDefault() != null 
                && lastTasks.FirstOrDefault().TopActivity != null 
                && lastTasks.FirstOrDefault().TopActivity.PackageName != null 
                && lastTasks.FirstOrDefault().TopActivity.PackageName != "net.fildo.app")
            {
                Process.KillProcess(Process.MyPid());
            }
            base.OnDestroy();
            if (Player != null)
            {
                Player.Release();
                Player = null;
            }
        }

        private void RegisterRemoteClient()
        {
            this.remoteComponentName = new ComponentName(
                this.PackageName,
                new RemoteControlBroadcastReceiver().ComponentName);
            if (this.remoteControlClient == null)
            {
                this.audioManager.RegisterMediaButtonEventReceiver(this.remoteComponentName);
                //Create a new pending intent that we want triggered by remote control client
                var mediaButtonIntent = new Intent(Intent.ActionMediaButton);
                mediaButtonIntent.SetComponent(this.remoteComponentName);
                // Create new pending intent for the intent
                var mediaPendingIntent = PendingIntent.GetBroadcast(this, 0, mediaButtonIntent, 0);
                // Create and register the remote control client
                this.remoteControlClient = new RemoteControlClient(mediaPendingIntent);
                this.audioManager.RegisterRemoteControlClient(this.remoteControlClient);
            }
            //add transport control flags we can to handle
            this.remoteControlClient.SetTransportControlFlags(
                RemoteControlFlags.Play | RemoteControlFlags.Pause | RemoteControlFlags.PlayPause
                | RemoteControlFlags.Stop | RemoteControlFlags.Previous | RemoteControlFlags.Next);
        }

        private void UnregisterRemoteClient()
        {
            try
            {
                this.audioManager.UnregisterMediaButtonEventReceiver(this.remoteComponentName);
                this.audioManager.UnregisterRemoteControlClient(this.remoteControlClient);
                if (this.remoteControlClient != null)
                {
                    this.remoteControlClient.Dispose();
                    this.remoteControlClient = null;
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateMetadata()
        {
            if (this.remoteControlClient == null)
            {
                return;
            }
            var metadataEditor = this.remoteControlClient.EditMetadata(true);
            var currentSong = Queue.FirstOrDefault(p => p.Url == CurrentTrack);
            if (currentSong == null || CurrentTrack == null)
            {
                currentSong = Queue.FirstOrDefault(p => p.Id == CurrentTrackId);
            }
            if (currentSong != null)
            {
                metadataEditor.PutString(MetadataKey.Artist, currentSong.Artist);
                metadataEditor.PutString(MetadataKey.Title, currentSong.Title);
                //var coverArt = BitmapFactory.DecodeResource(Resources, Resource.Drawable.album_art);
                //metadataEditor.PutBitmap(BitmapKey.Artwork, coverArt);
            }
            else
            {
                metadataEditor.PutString(MetadataKey.Artist, string.Empty);
                metadataEditor.PutString(MetadataKey.Title, string.Empty);
            }
            metadataEditor.Apply();
        }
    }
}