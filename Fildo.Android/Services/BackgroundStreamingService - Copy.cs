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
using Android.Media;
using Android.Net.Wifi;
using Android.Net;
using Fildo.Droid.Views;
using Fildo.Core.Entities;
using System.Collections.ObjectModel;
using Fildo.Droid.Receivers;
using Android.Support.V4.Media.Session;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Support.V4.Media;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

namespace Fildo.Droid.Services
{
    [Service]
    [IntentFilter(new[] { ActionPlay, ActionPause, ActionStop, ActionNext, ActionPrev, ActionPlayWithoutClear, ActionClose })]
    public class BackgroundStreamingService : Service, AudioManager.IOnAudioFocusChangeListener
    {
        //Actions
        public const string ActionPlay = "fildo.fildoaction.PLAY";
        public const string ActionPause = "fildo.fildo.action.PAUSE";
        public const string ActionStop = "fildo.fildo.action.STOP";
        public const string ActionNext = "fildo.fildo.action.NEXT";
        public const string ActionPrev = "fildo.fildo.action.PREV";

        public const string ActionClose = "fildo.fildo.action.CLOSE";
        public const string ActionPlayWithoutClear = "fildo.fildo.action.PLAYWITHOUT";
        public static event EventHandler QueueChanged;

        private ComponentName remoteComponentName;
        private RemoteControlClient remoteControlClient;

        public static string currentTrack;
        public static MediaPlayer Player;

        private AudioManager audioManager;
        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;
        private bool paused;
        public static bool IsShuffle;
        public static bool IsRepeat;
        private const int NotificationId = 600223194;
        private bool starting = false;

        public static event EventHandler<string> SongNameChanged;
        public static event EventHandler<int> posChanged;

        public static event EventHandler<int> percentChanged;
        public static event EventHandler<int> prepared;
        public static string SongName;

        private static ObservableCollection<Song> queue = new ObservableCollection<Song>();
        private int errorCounter;
        private bool prevPlay;

        public BackgroundStreamingService()
        {
            Queue.CollectionChanged += Queue_CollectionChanged;
        }

        private void Queue_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Queue.Count == 0)
            {
                currentTrack = null;
            }
        }

        public static string GetCurrentSong()
        {
            if (string.IsNullOrEmpty(currentTrack) || (queue == null) || (queue.Count == 0))
            {
                return "0";
            }
            var currentSong = queue.Where(p => p.Url == currentTrack).FirstOrDefault();
            return currentSong.Id;
        }

        public static ObservableCollection<Song> Queue
        {
            get { return queue; }
            set { queue = value; }
        }

        public override void OnCreate()
        {
            base.OnCreate();
            //Find our audio and notificaton managers
            audioManager = (AudioManager)GetSystemService(AudioService);
            wifiManager = (WifiManager)GetSystemService(WifiService);

            remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);
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
            if (!Player.IsPlaying)
            {
                currentTrack = song.Url;
                Play();
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
                            Play();
                            break;
                        case ActionStop:
                            Stop();
                            break;
                        case ActionPause:
                            Pause();
                            break;
                        case ActionNext:
                            Next();
                            break;
                        case ActionPrev:
                            Prev();
                            break;
                        case ActionPlayWithoutClear:
                            PlayWithoutClear();
                            break;
                        case ActionClose:
                            this.StopSelf();
                            break;
                    }

                    //Set sticky as we are a long running operation
                    return StartCommandResult.Sticky;
                }
                else
                {
                    return StartCommandResult.NotSticky;
                }
            }
            catch (Exception)
            {
                return StartCommandResult.NotSticky;
            }
        }

        private void Next()
        {
            if (Player == null)
                return;
            
            var currentSong = queue.Where(p => p.Url == currentTrack).FirstOrDefault();
            if (currentSong != null)
            {
                int currentPosition = queue.IndexOf(currentSong);
                if (currentPosition > -1)
                {
                    if (queue.Count > currentPosition + 1)
                    {
                        if (IsShuffle)
                        {
                            Random rnd = new Random();
                            int r = rnd.Next(queue.Count);
                            currentTrack = queue[r].Url;
                            this.Play();
                        }
                        else
                        {
                            currentTrack = queue[currentPosition + 1].Url;
                            this.Play();
                        }
                    }
                    else if (IsRepeat)
                    {
                        currentTrack = queue[0].Url;
                    }
                    else
                    {
                        this.Stop();
                        this.StopSelf();
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

        private void Prev()
        {
            if (Player == null)
                return;
            var currentSong = queue.Where(p => p.Url == currentTrack).FirstOrDefault();
            if (currentSong != null)
            {
                int currentPosition = queue.IndexOf(currentSong);
                if ((currentPosition > -1) && (currentPosition - 1 > -1))
                {
                    currentTrack = queue[currentPosition - 1].Url;
                    this.Play();
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

        private void IntializePlayer()
        {
            Player = new MediaPlayer();

            //Tell our player to sream music
            Player.SetAudioStreamType(Stream.Music);

            //Wake mode will be partial to keep the CPU still running under lock screen
            Player.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);

            //When we have prepared the song start playback
            Player.Prepared += (sender, args) => {
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
            };

            Task.Run(async () =>
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
                                int percent = (Player.CurrentPosition * 100) / Player.Duration;
                                percentChanged(this, percent);
                            }
                        }
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        
                    }

                }
            });

            //When we have reached the end of the song stop ourselves, however you could signal next track here.
            Player.Completion += ((sender, args) =>
            {
                Next();
            });

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
        }

        private async void PlayWithoutClear()
        {
            Play();
        }

        private async void Play()
        {
            try
            {
                Toast.MakeText(ApplicationContext, "Preparing Streaming", ToastLength.Long);
                await Task.Delay(1500);
                if (currentTrack == null)
                {
                    if (Queue.Count > 0)
                    {
                        currentTrack = Queue.First().Url;
                    }
                    else
                    {
                        return;
                    }
                }

                Song currentSong = queue.Where(p => p.Url == currentTrack).FirstOrDefault();

                if (Player != null)
                {
                    Player.Stop();
                }
                Player = null;

                if (paused && Player != null)
                {
                    paused = false;
                    //We are simply paused so just start again
                    Player.Start();
                    if (currentSong != null)
                    {
                        if (!string.IsNullOrEmpty(currentSong.Artist) && !string.IsNullOrEmpty(currentSong.Title))
                        {
                            StartForeground(currentSong.Artist + " - " + currentSong.Title);
                        }
                        else if (!string.IsNullOrEmpty(currentSong.Artist))
                        {
                            StartForeground(currentSong.Artist);
                        }
                        else if (!string.IsNullOrEmpty(currentSong.Title))
                        {
                            StartForeground(currentSong.Title);
                        }
                    }

                    RegisterRemoteClient();
                    remoteControlClient.SetPlaybackState(RemoteControlPlayState.Playing);
                    UpdateMetadata();
                    return;
                }

                

                if (Player == null)
                {
                    IntializePlayer();
                }

                if (Player.IsPlaying)
                {
                    this.Stop();
                }
                else
                {
                    Player.Reset();
                    paused = false;
                    StopForeground(true);
                    ReleaseWifiLock();
                }


                starting = true;
                var netEase = MvvmCross.Platform.Mvx.Resolve<Core.INetEase>();
                var newSong = new SongNetease() { Artist = currentSong.Artist, Title = currentSong.Title, Url = currentTrack };

                var currenTrackToPlay = (await netEase.FixUrl(newSong, true)).Url;
                try
                {
                    if (Player == null)
                    {
                        IntializePlayer();
                    }
                    if (currenTrackToPlay.StartsWith("http://221.228.64.228/"))
                    {
                        if (Player == null)
                        {

                        }
                        Dictionary<string, string> headers = new Dictionary<string, string>();
                        headers.Add("Host", "m1.music.126.net");
                        await Player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(currenTrackToPlay), headers);
                    }
                    else
                    {
                        if (Player == null)
                        {

                        }
                        await Player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(currenTrackToPlay));
                    }

                }
                catch (Exception ex)
                {
                    
                }
                
                if (QueueChanged != null)
                {
                    QueueChanged(this, new EventArgs());
                }

                var focusResult = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
                if (focusResult != AudioFocusRequest.Granted)
                {
                    //could not get audio focus
                    Console.WriteLine("Could not get audio focus");
                }
                try
                {
                    Player.PrepareAsync();
                }
                catch (Exception)
                {
                    try
                    {
                        Player.Reset();
                        paused = false;
                        StopForeground(true);
                        ReleaseWifiLock();
                        if (currenTrackToPlay.StartsWith("http://221.228.64.228/"))
                        {
                            Dictionary<string, string> headers = new Dictionary<string, string>();
                            headers.Add("Host", "m1.music.126.net");
                            await Player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(currenTrackToPlay), headers);
                        }
                        else
                        {
                            await Player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(currenTrackToPlay));
                        }
                        Player.PrepareAsync();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            Player.Reset();
                            paused = false;
                            StopForeground(true);
                            ReleaseWifiLock();
                            if (currenTrackToPlay.StartsWith("http://221.228.64.228/"))
                            {
                                Dictionary<string, string> headers = new Dictionary<string, string>();
                                headers.Add("Host", "m1.music.126.net");
                                await Player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(currenTrackToPlay), headers);
                            }
                            else
                            {
                                await Player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(currenTrackToPlay));
                            }
                            Player.PrepareAsync();
                        }
                        catch (Exception)
                        {
                            this.Next();
                        }
                    }
                }

                AquireWifiLock();

                if (currentSong != null)
                {
                    if (!string.IsNullOrEmpty(currentSong.Artist) && !string.IsNullOrEmpty(currentSong.Title))
                    {
                        StartForeground(currentSong.Artist + " - " + currentSong.Title);
                    }
                    else if (!string.IsNullOrEmpty(currentSong.Artist))
                    {
                        StartForeground(currentSong.Artist);
                    }
                    else if (!string.IsNullOrEmpty(currentSong.Title))
                    {
                        StartForeground(currentSong.Title);
                    }
                }

                RegisterRemoteClient();
                remoteControlClient.SetPlaybackState(RemoteControlPlayState.Buffering);
                UpdateMetadata();

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// When we start on the foreground we will present a notification to the user
        /// When they press the notification it will take them to the main page so they can control the music
        /// </summary>
        private void StartForeground(string detail)
        {
            if (SongNameChanged != null)
            {
                SongNameChanged(this, detail);
            }
            SongName = detail;

            var intent = new Intent(ApplicationContext, typeof(MainView));
            intent.PutExtra("notificationPlayList", "player");
            var pendingIntent = PendingIntent.GetActivity(ApplicationContext, 0,
                            intent,
                            PendingIntentFlags.UpdateCurrent);
            
            // use System.currentTimeMillis() to have a unique ID for the pending intent
            PendingIntent pIntent = PendingIntent.GetActivity(this, NotificationId, intent, 0);

            var intentNext = new Intent(BackgroundStreamingService.ActionNext);
            PendingIntent pIntentNext = PendingIntent.GetService(this, NotificationId+1, intentNext, 0);

            var intentPlayPause = new Intent(BackgroundStreamingService.ActionPause);
            PendingIntent pIntentPlayPause = PendingIntent.GetService(this, NotificationId + 2, intentPlayPause, 0);

            var intentClose = new Intent(BackgroundStreamingService.ActionClose);
            PendingIntent pIntentClose = PendingIntent.GetService(this, NotificationId + 2, intentClose, 0);


            NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
                .SetContentIntent(pendingIntent)  // Start 2nd activity when the intent is clicked.
                .SetSmallIcon(Resource.Drawable.Icon)  // Display this icon
                .AddAction(Resource.Drawable.play3, string.Empty, pIntentPlayPause)
                .AddAction(Resource.Drawable.nextb, string.Empty, pIntentNext)
                .AddAction(0, "Close", pIntentClose)
                //.SetAutoCancel(true)                    // Dismiss from the notif. area when clicked
                .SetContentTitle("Fildo")      // Set its title
                .SetContentText(detail); // The message to display.

            Notification notification = builder.Build();

            //var notification = new Notification
            //{
            //    TickerText = new Java.Lang.String("Song started!"),
            //    Icon = Resource.Drawable.Icon
            //};
            notification.Flags |= NotificationFlags.OngoingEvent;
            //notification.SetLatestEventInfo(ApplicationContext, "Fildo Streaming", detail, pendingIntent);

            StartForeground(NotificationId, notification);
        }

        private void Pause()
        {
            try
            {
                if (Player == null)
                    return;

                if (Player.IsPlaying)
                {
                    Player.Pause();
                    remoteControlClient.SetPlaybackState(RemoteControlPlayState.Paused);
                    paused = true;
                }
                else
                {
                    Player.Start();
                    remoteControlClient.SetPlaybackState(RemoteControlPlayState.Playing);
                    Song currentSong = queue.Where(p => p.Url == currentTrack).FirstOrDefault();

                    if (currentSong != null)
                    {
                        if (!string.IsNullOrEmpty(currentSong.Artist) && !string.IsNullOrEmpty(currentSong.Title))
                        {
                            StartForeground(currentSong.Artist + " - " + currentSong.Title);
                        }
                        else if (!string.IsNullOrEmpty(currentSong.Artist))
                        {
                            StartForeground(currentSong.Artist);
                        }
                        else if (!string.IsNullOrEmpty(currentSong.Title))
                        {
                            StartForeground(currentSong.Title);
                        }
                    }
                    paused = true;
                }
                
            }
            catch (Exception)
            {
            }
        }

        private void Stop()
        {
            if (Player == null)
                return;

            if (Player.IsPlaying)
            {
                Player.Stop();
                if (remoteControlClient != null)
                    remoteControlClient.SetPlaybackState(RemoteControlPlayState.Stopped);
            }

            Player.Reset();
            paused = false;
            StopForeground(true);
            ReleaseWifiLock();
            UnregisterRemoteClient();
        }

        /// <summary>
        /// Lock the wifi so we can still stream under lock screen
        /// </summary>
        private void AquireWifiLock()
        {
            if (wifiLock == null)
            {
                wifiLock = wifiManager.CreateWifiLock(WifiMode.Full, "fildo_wifi_lock");
            }
            wifiLock.Acquire();
        }

        /// <summary>
        /// This will release the wifi lock if it is no longer needed
        /// </summary>
        private void ReleaseWifiLock()
        {
            if (wifiLock == null)
                return;

            wifiLock.Release();
            wifiLock = null;
        }

        /// <summary>
        /// Properly cleanup of your player by releasing resources
        /// </summary>
        public override void OnDestroy()
        {
            var activityManager = (ActivityManager)this.ApplicationContext.GetSystemService(Context.ActivityService);
            var lastTasks = activityManager.GetRunningTasks(1);
            if (lastTasks != null && 
                lastTasks.FirstOrDefault() != null && 
                lastTasks.FirstOrDefault().TopActivity != null && 
                lastTasks.FirstOrDefault().TopActivity.PackageName != null && 
                lastTasks.FirstOrDefault().TopActivity.PackageName != "Fildo.Fildo")
            {
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            }
            base.OnDestroy();
            if (Player != null)
            {
                Player.Release();
                Player = null;
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
            switch (focusChange)
            {
                case AudioFocus.Gain:
                    if (Player == null)
                        IntializePlayer();

                    if (!Player.IsPlaying && !starting && this.prevPlay)
                    {
                        Player.Start();
                        paused = false;
                    }

                    Player.SetVolume(1.0f, 1.0f);//Turn it up!
                    break;
                case AudioFocus.Loss:
                    //We have lost focus stop!
                    this.prevPlay = Player.IsPlaying;
                    Pause();
                    break;
                case AudioFocus.LossTransient:
                    //We have lost focus for a short time, but likely to resume so pause
                    this.prevPlay = Player.IsPlaying;
                    Pause();
                    break;
                case AudioFocus.LossTransientCanDuck:
                    //We have lost focus but should till play at a muted 10% volume
                    if (Player.IsPlaying)
                        Player.SetVolume(.1f, .1f);//turn it down!
                    break;

            }
        }

        private void RegisterRemoteClient()
        {
            remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);
            if (remoteControlClient == null)
            {
                audioManager.RegisterMediaButtonEventReceiver(remoteComponentName);
                //Create a new pending intent that we want triggered by remote control client
                var mediaButtonIntent = new Intent(Intent.ActionMediaButton);
                mediaButtonIntent.SetComponent(remoteComponentName);
                // Create new pending intent for the intent
                var mediaPendingIntent = PendingIntent.GetBroadcast(this, 0, mediaButtonIntent, 0);
                // Create and register the remote control client
                remoteControlClient = new RemoteControlClient(mediaPendingIntent);
                audioManager.RegisterRemoteControlClient(remoteControlClient);
            }
            //add transport control flags we can to handle
            remoteControlClient.SetTransportControlFlags(RemoteControlFlags.Play |
                                     RemoteControlFlags.Pause |
                                     RemoteControlFlags.PlayPause |
                                     RemoteControlFlags.Stop |
                                     RemoteControlFlags.Previous |
                                     RemoteControlFlags.Next);
        }

        private void UnregisterRemoteClient()
        {
            try
            {
                audioManager.UnregisterMediaButtonEventReceiver(remoteComponentName);
                audioManager.UnregisterRemoteControlClient(remoteControlClient);
                if (remoteControlClient != null)
                {
                    remoteControlClient.Dispose();
                    remoteControlClient = null;
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void UpdateMetadata()
        {
            if (remoteControlClient == null)
                return;
            var metadataEditor = remoteControlClient.EditMetadata(true);
            var currentSong = queue.Where(p => p.Url == currentTrack).FirstOrDefault();
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













    /*[Service]
    [IntentFilter(new[] { ActionPlay, ActionPause, ActionStop, ActionTogglePlayback, ActionNext, ActionPrevious })]
    public class BackgroundStreamingService : Service, AudioManager.IOnAudioFocusChangeListener,
    MediaPlayer.IOnBufferingUpdateListener,
    MediaPlayer.IOnCompletionListener,
    MediaPlayer.IOnErrorListener,
    MediaPlayer.IOnPreparedListener,
    MediaPlayer.IOnSeekCompleteListener
    {
        //Actions
        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionPause = "com.xamarin.action.PAUSE";
        public const string ActionStop = "com.xamarin.action.STOP";
        public const string ActionTogglePlayback = "com.xamarin.action.TOGGLEPLAYBACK";
        public const string ActionNext = "com.xamarin.action.NEXT";
        public const string ActionPrevious = "com.xamarin.action.PREVIOUS";


        
        public MediaPlayer mediaPlayer;
        private AudioManager audioManager;

        private MediaSessionCompat mediaSessionCompat;
        public MediaControllerCompat mediaControllerCompat;
        private string currentTrack;

        public static ObservableCollection<Song> Queue
        {
            get { return queue; }
            set { queue = value; }
        }

        public int MediaPlayerState
        {
            get
            {
                return (mediaControllerCompat.PlaybackState != null ?
                    mediaControllerCompat.PlaybackState.State :
                    PlaybackStateCompat.StateNone);
            }
        }


        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;
        private ComponentName remoteComponentName;

        private const int NotificationId = 1;

        public event EventHandlers.StatusChangedEventHandler StatusChanged;

        public event EventHandlers.CoverReloadedEventHandler CoverReloaded;

        public event EventHandlers.PlayingEventHandler Playing;

        public event EventHandlers.BufferingEventHandler Buffering;

        private Handler PlayingHandler;
        private Java.Lang.Runnable PlayingHandlerRunnable;
        private static ObservableCollection<Song> queue = new ObservableCollection<Song>();
        public BackgroundStreamingService()
        {

            Queue.CollectionChanged += Queue_CollectionChanged;
            // Create an instance for a runnable-handler
            PlayingHandler = new Handler();

            // Create a runnable, restarting itself if the status still is "playing"
            PlayingHandlerRunnable = new Java.Lang.Runnable(() => {
                OnPlaying(EventArgs.Empty);

                if (MediaPlayerState == PlaybackStateCompat.StatePlaying)
                {
                    PlayingHandler.PostDelayed(PlayingHandlerRunnable, 250);
                }
            });

            // On Status changed to PLAYING, start raising the Playing event
            StatusChanged += (object sender, EventArgs e) => {
                if (MediaPlayerState == PlaybackStateCompat.StatePlaying)
                {
                    PlayingHandler.PostDelayed(PlayingHandlerRunnable, 0);
                }
            };
        }

        private void Queue_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Queue.Count == 0)
            {
                currentTrack = null;
            }
        }

        protected virtual void OnStatusChanged(EventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }

        protected virtual void OnCoverReloaded(EventArgs e)
        {
            if (CoverReloaded != null)
            {
                CoverReloaded(this, e);
                StartNotification();
                UpdateMediaMetadataCompat();
            }
        }

        protected virtual void OnPlaying(EventArgs e)
        {
            if (Playing != null)
                Playing(this, e);
        }

        protected virtual void OnBuffering(EventArgs e)
        {
            if (Buffering != null)
                Buffering(this, e);
        }

        /// <summary>
        /// On create simply detect some of our managers
        /// </summary>
        public override void OnCreate()
        {
            base.OnCreate();
            //Find our audio and notificaton managers
            audioManager = (AudioManager)GetSystemService(AudioService);
            wifiManager = (WifiManager)GetSystemService(WifiService);

            remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);
        }

        /// <summary>
        /// Will register for the remote control client commands in audio manager
        /// </summary>
        private void InitMediaSession()
        {
            try
            {
                if (mediaSessionCompat == null)
                {
                    Intent nIntent = new Intent(ApplicationContext, typeof(MainView));
                    PendingIntent pIntent = PendingIntent.GetActivity(ApplicationContext, 0, nIntent, 0);

                    remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);

                    mediaSessionCompat = new MediaSessionCompat(ApplicationContext, "XamarinStreamingAudio", remoteComponentName, pIntent);
                    mediaControllerCompat = new MediaControllerCompat(ApplicationContext, mediaSessionCompat.SessionToken);
                }

                mediaSessionCompat.Active = true;
                mediaSessionCompat.SetCallback(new MediaSessionCallback((MediaPlayerServiceBinder)binder));

                mediaSessionCompat.SetFlags(MediaSessionCompat.FlagHandlesMediaButtons | MediaSessionCompat.FlagHandlesTransportControls);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Intializes the player.
        /// </summary>
        private void InitializePlayer()
        {
            mediaPlayer = new MediaPlayer();

            //Tell our player to sream music
            mediaPlayer.SetAudioStreamType(Stream.Music);

            //Wake mode will be partial to keep the CPU still running under lock screen
            mediaPlayer.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);

            mediaPlayer.SetOnBufferingUpdateListener(this);
            mediaPlayer.SetOnCompletionListener(this);
            mediaPlayer.SetOnErrorListener(this);
            mediaPlayer.SetOnPreparedListener(this);
        }


        public void OnBufferingUpdate(MediaPlayer mp, int percent)
        {
            int duration = 0;
            if (MediaPlayerState == PlaybackStateCompat.StatePlaying || MediaPlayerState == PlaybackStateCompat.StatePaused)
                duration = mp.Duration;

            int newBufferedTime = duration * percent / 100;
            if (newBufferedTime != Buffered)
            {
                Buffered = newBufferedTime;
            }
        }

        public async void OnCompletion(MediaPlayer mp)
        {
            await PlayNext();
        }

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {

            UpdatePlaybackState(PlaybackStateCompat.StateError);
            Stop();
            return true;
        }

        public void OnSeekComplete(MediaPlayer mp)
        {
            //TODO: Implement buffering on seeking
        }

        public void OnPrepared(MediaPlayer mp)
        {
            //Mediaplayer is prepared start track playback
            mp.Start();
            UpdatePlaybackState(PlaybackStateCompat.StatePlaying);
        }

        public int Position
        {
            get
            {
                if (mediaPlayer == null
                    || (MediaPlayerState != PlaybackStateCompat.StatePlaying
                        && MediaPlayerState != PlaybackStateCompat.StatePaused))
                    return -1;
                else
                    return mediaPlayer.CurrentPosition;
            }
        }

        public int Duration
        {
            get
            {
                if (mediaPlayer == null
                    || (MediaPlayerState != PlaybackStateCompat.StatePlaying
                        && MediaPlayerState != PlaybackStateCompat.StatePaused))
                    return 0;
                else
                    return mediaPlayer.Duration;
            }
        }

        private int buffered = 0;

        public int Buffered
        {
            get
            {
                if (mediaPlayer == null)
                    return 0;
                else
                    return buffered;
            }
            private set
            {
                buffered = value;
                OnBuffering(EventArgs.Empty);
            }
        }

        private Bitmap cover;

        public object Cover
        {
            get
            {
                if (cover == null)
                    cover = BitmapFactory.DecodeResource(Resources, Resource.Drawable.Icon);
                return cover;
            }
            private set
            {
                cover = value as Bitmap;
                OnCoverReloaded(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Intializes the player.
        /// </summary>
        public async Task Play()
        {
            if (currentTrack == null)
            {
                if (Queue.Count > 0)
                {
                    currentTrack = Queue.First().Url;
                }
                else
                {
                    return;
                }
            }
            if (mediaPlayer != null && MediaPlayerState == PlaybackStateCompat.StatePaused)
            {
                //We are simply paused so just start again
                mediaPlayer.Start();
                UpdatePlaybackState(PlaybackStateCompat.StatePlaying);
                StartNotification();

                //Update the metadata now that we are playing
                UpdateMediaMetadataCompat();
                return;
            }

            if (mediaPlayer == null)
                InitializePlayer();

            if (mediaSessionCompat == null)
                InitMediaSession();

            if (mediaPlayer.IsPlaying)
            {
                UpdatePlaybackState(PlaybackStateCompat.StatePlaying);
                return;
            }

            try
            {
                MediaMetadataRetriever metaRetriever = new MediaMetadataRetriever();

                await mediaPlayer.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(currentTrack));

                await metaRetriever.SetDataSourceAsync(currentTrack, new Dictionary<string, string>());

                var focusResult = audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
                if (focusResult != AudioFocusRequest.Granted)
                {
                    //could not get audio focus
                    Console.WriteLine("Could not get audio focus");
                }

                UpdatePlaybackState(PlaybackStateCompat.StateBuffering);
                mediaPlayer.PrepareAsync();

                AquireWifiLock();
                UpdateMediaMetadataCompat(metaRetriever);
                StartNotification();

                byte[] imageByteArray = metaRetriever.GetEmbeddedPicture();
                if (imageByteArray == null)
                    Cover = await BitmapFactory.DecodeResourceAsync(Resources, Resource.Drawable.Icon);
                else
                    Cover = await BitmapFactory.DecodeByteArrayAsync(imageByteArray, 0, imageByteArray.Length);
            }
            catch (Exception ex)
            {
                UpdatePlaybackState(PlaybackStateCompat.StateStopped);

                mediaPlayer.Reset();
                mediaPlayer.Release();
                mediaPlayer = null;

                //unable to start playback log error
                Console.WriteLine(ex);
            }
        }

        public async Task Seek(int position)
        {
            await Task.Run(() => {
                if (mediaPlayer != null)
                {
                    mediaPlayer.SeekTo(position);
                }
            });
        }

        public async Task PlayNext()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Reset();
                mediaPlayer.Release();
                mediaPlayer = null;
            }

            UpdatePlaybackState(PlaybackStateCompat.StateSkippingToNext);

            var currentSong = queue.Where(p => p.Url == this.currentTrack).FirstOrDefault();
            if (currentSong != null)
            {
                int currentPosition = queue.IndexOf(currentSong);
                if ((currentPosition > -1) && (queue.Count > currentPosition + 1))
                {
                    this.currentTrack = queue[currentPosition + 1].Url;
                    await Play();
                }
                else
                {
                    await Stop();
                }
            }
            else
            {
                await Stop();
            }
        }

        public async Task PlayPrevious()
        {
            // Start current track from beginning if it's the first track or the track has played more than 3sec and you hit "playPrevious".
            if (Position > 3000)
            {
                await Seek(0);
            }
            else
            {
                if (mediaPlayer != null)
                {
                    mediaPlayer.Reset();
                    mediaPlayer.Release();
                    mediaPlayer = null;
                }

                UpdatePlaybackState(PlaybackStateCompat.StateSkippingToPrevious);

                await Play();
            }
        }

        public async Task PlayPause()
        {
            if (mediaPlayer == null || (mediaPlayer != null && MediaPlayerState == PlaybackStateCompat.StatePaused))
            {
                await Play();
            }
            else
            {
                await Pause();
            }
        }

        public async Task Pause()
        {
            await Task.Run(() => {
                if (mediaPlayer == null)
                    return;

                if (mediaPlayer.IsPlaying)
                    mediaPlayer.Pause();

                UpdatePlaybackState(PlaybackStateCompat.StatePaused);
            });
        }

        public async Task Stop()
        {
            await Task.Run(() => {
                if (mediaPlayer == null)
                    return;

                if (mediaPlayer.IsPlaying)
                {
                    mediaPlayer.Stop();
                }

                UpdatePlaybackState(PlaybackStateCompat.StateStopped);
                mediaPlayer.Reset();
                StopNotification();
                StopForeground(true);
                ReleaseWifiLock();
                UnregisterMediaSessionCompat();
            });
        }

        private void UpdatePlaybackState(int state)
        {
            if (mediaSessionCompat == null || mediaPlayer == null)
                return;

            try
            {
                PlaybackStateCompat.Builder stateBuilder = new PlaybackStateCompat.Builder()
                    .SetActions(
                        PlaybackStateCompat.ActionPause |
                        PlaybackStateCompat.ActionPlay |
                        PlaybackStateCompat.ActionPlayPause |
                        PlaybackStateCompat.ActionSkipToNext |
                        PlaybackStateCompat.ActionSkipToPrevious |
                        PlaybackStateCompat.ActionStop
                    )
                    .SetState(state, Position, 1.0f, SystemClock.ElapsedRealtime());

                mediaSessionCompat.SetPlaybackState(stateBuilder.Build());

                //Used for backwards compatibility
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    if (mediaSessionCompat.RemoteControlClient != null && mediaSessionCompat.RemoteControlClient.Equals(typeof(RemoteControlClient)))
                    {
                        RemoteControlClient remoteControlClient = (RemoteControlClient)mediaSessionCompat.RemoteControlClient;

                        RemoteControlFlags flags = RemoteControlFlags.Play
                            | RemoteControlFlags.Pause
                            | RemoteControlFlags.PlayPause
                            | RemoteControlFlags.Previous
                            | RemoteControlFlags.Next
                            | RemoteControlFlags.Stop;

                        remoteControlClient.SetTransportControlFlags(flags);
                    }
                }

                OnStatusChanged(EventArgs.Empty);

                if (state == PlaybackStateCompat.StatePlaying || state == PlaybackStateCompat.StatePaused)
                {
                    StartNotification();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// When we start on the foreground we will present a notification to the user
        /// When they press the notification it will take them to the main page so they can control the music
        /// </summary>
        private void StartNotification()
        {
            if (mediaSessionCompat == null)
                return;

            var pendingIntent = PendingIntent.GetActivity(ApplicationContext, 0, new Intent(ApplicationContext, typeof(MainView)), PendingIntentFlags.UpdateCurrent);
            MediaMetadataCompat currentTrack = mediaControllerCompat.Metadata;

            Android.Support.V7.App.NotificationCompat.MediaStyle style = new Android.Support.V7.App.NotificationCompat.MediaStyle();
            style.SetMediaSession(mediaSessionCompat.SessionToken);

            Intent intent = new Intent(ApplicationContext, typeof(BackgroundStreamingService));
            intent.SetAction(ActionStop);
            PendingIntent pendingCancelIntent = PendingIntent.GetService(ApplicationContext, 1, intent, PendingIntentFlags.CancelCurrent);

            style.SetShowCancelButton(true);
            style.SetCancelButtonIntent(pendingCancelIntent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(ApplicationContext)
                .SetStyle(style)
                .SetContentTitle(currentTrack.GetString(MediaMetadata.MetadataKeyTitle))
                .SetContentText(currentTrack.GetString(MediaMetadata.MetadataKeyArtist))
                .SetContentInfo(currentTrack.GetString(MediaMetadata.MetadataKeyAlbum))
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetLargeIcon(Cover as Bitmap)
                .SetContentIntent(pendingIntent)
                .SetShowWhen(false)
                .SetOngoing(MediaPlayerState == PlaybackStateCompat.StatePlaying)
                .SetVisibility(NotificationCompat.VisibilityPublic);

            builder.AddAction(GenerateActionCompat(Android.Resource.Drawable.IcMediaPrevious, "Previous", ActionPrevious));
            AddPlayPauseActionCompat(builder);
            builder.AddAction(GenerateActionCompat(Android.Resource.Drawable.IcMediaNext, "Next", ActionNext));
            style.SetShowActionsInCompactView(0, 1, 2);

            NotificationManagerCompat.From(ApplicationContext).Notify(NotificationId, builder.Build());
        }

        private NotificationCompat.Action GenerateActionCompat(int icon, String title, String intentAction)
        {
            Intent intent = new Intent(ApplicationContext, typeof(BackgroundStreamingService));
            intent.SetAction(intentAction);

            PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
            if (intentAction.Equals(ActionStop))
                flags = PendingIntentFlags.CancelCurrent;

            PendingIntent pendingIntent = PendingIntent.GetService(ApplicationContext, 1, intent, flags);

            return new NotificationCompat.Action.Builder(icon, title, pendingIntent).Build();
        }

        private void AddPlayPauseActionCompat(NotificationCompat.Builder builder)
        {
            if (MediaPlayerState == PlaybackStateCompat.StatePlaying)
                builder.AddAction(GenerateActionCompat(Android.Resource.Drawable.IcMediaPause, "Pause", ActionPause));
            else
                builder.AddAction(GenerateActionCompat(Android.Resource.Drawable.IcMediaPlay, "Play", ActionPlay));
        }

        public void StopNotification()
        {
            NotificationManagerCompat nm = NotificationManagerCompat.From(ApplicationContext);
            nm.CancelAll();
        }

        /// <summary>
        /// Updates the metadata on the lock screen
        /// </summary>
        private void UpdateMediaMetadataCompat(MediaMetadataRetriever metaRetriever = null)
        {
            if (mediaSessionCompat == null)
                return;

            MediaMetadataCompat.Builder builder = new MediaMetadataCompat.Builder();

            if (metaRetriever != null)
            {
                builder
                .PutString(MediaMetadata.MetadataKeyAlbum, metaRetriever.ExtractMetadata(MetadataKey.Album))
                .PutString(MediaMetadata.MetadataKeyArtist, metaRetriever.ExtractMetadata(MetadataKey.Artist))
                .PutString(MediaMetadata.MetadataKeyTitle, metaRetriever.ExtractMetadata(MetadataKey.Title));
            }
            else {
                builder
                    .PutString(MediaMetadata.MetadataKeyAlbum, mediaSessionCompat.Controller.Metadata.GetString(MediaMetadata.MetadataKeyAlbum))
                    .PutString(MediaMetadata.MetadataKeyArtist, mediaSessionCompat.Controller.Metadata.GetString(MediaMetadata.MetadataKeyArtist))
                    .PutString(MediaMetadata.MetadataKeyTitle, mediaSessionCompat.Controller.Metadata.GetString(MediaMetadata.MetadataKeyTitle));
            }
            builder.PutBitmap(MediaMetadata.MetadataKeyAlbumArt, Cover as Bitmap);

            mediaSessionCompat.SetMetadata(builder.Build());
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            HandleIntent(intent);
            //return base.OnStartCommand(intent, flags, startId);
            
            return StartCommandResult.Sticky; 
        }

        

        private void HandleIntent(Intent intent)
        {
            if (intent == null || intent.Action == null)
                return;

            String action = intent.Action;

            if (action.Equals(ActionPlay))
            {
                mediaControllerCompat.GetTransportControls().Play();
            }
            else if (action.Equals(ActionPause))
            {
                mediaControllerCompat.GetTransportControls().Pause();
            }
            else if (action.Equals(ActionPrevious))
            {
                mediaControllerCompat.GetTransportControls().SkipToPrevious();
            }
            else if (action.Equals(ActionNext))
            {
                mediaControllerCompat.GetTransportControls().SkipToNext();
            }
            else if (action.Equals(ActionStop))
            {
                mediaControllerCompat.GetTransportControls().Stop();
            }
        }

        /// <summary>
        /// Lock the wifi so we can still stream under lock screen
        /// </summary>
        private void AquireWifiLock()
        {
            if (wifiLock == null)
            {
                wifiLock = wifiManager.CreateWifiLock(WifiMode.Full, "xamarin_wifi_lock");
            }
            wifiLock.Acquire();
        }

        /// <summary>
        /// This will release the wifi lock if it is no longer needed
        /// </summary>
        private void ReleaseWifiLock()
        {
            if (wifiLock == null)
                return;

            wifiLock.Release();
            wifiLock = null;
        }

        private void UnregisterMediaSessionCompat()
        {
            try
            {
                if (mediaSessionCompat != null)
                {
                    mediaSessionCompat.Dispose();
                    mediaSessionCompat = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        IBinder binder;

        public override IBinder OnBind(Intent intent)
        {
            binder = new MediaPlayerServiceBinder(this);
            return binder;
        }

        public override bool OnUnbind(Intent intent)
        {
            StopNotification();
            return base.OnUnbind(intent);
        }

        /// <summary>
        /// Properly cleanup of your player by releasing resources
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (mediaPlayer != null)
            {
                mediaPlayer.Release();
                mediaPlayer = null;

                StopNotification();
                StopForeground(true);
                ReleaseWifiLock();
                UnregisterMediaSessionCompat();
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
            switch (focusChange)
            {
                case AudioFocus.Gain:
                    if (mediaPlayer == null)
                        InitializePlayer();

                    if (!mediaPlayer.IsPlaying)
                    {
                        mediaPlayer.Start();
                    }

                    mediaPlayer.SetVolume(1.0f, 1.0f);//Turn it up!
                    break;
                case AudioFocus.Loss:
                    //We have lost focus stop!
                    Stop();
                    break;
                case AudioFocus.LossTransient:
                    //We have lost focus for a short time, but likely to resume so pause
                    Pause();
                    break;
                case AudioFocus.LossTransientCanDuck:
                    //We have lost focus but should till play at a muted 10% volume
                    if (mediaPlayer.IsPlaying)
                        mediaPlayer.SetVolume(.1f, .1f);//turn it down!
                    break;

            }
        }

        public class MediaSessionCallback : MediaSessionCompat.Callback
        {

            private MediaPlayerServiceBinder mediaPlayerService;
            public MediaSessionCallback(MediaPlayerServiceBinder service)
            {
                mediaPlayerService = service;
            }

            public override void OnPause()
            {
                mediaPlayerService.GetMediaPlayerService().Pause();
                base.OnPause();
            }

            public override void OnPlay()
            {
                mediaPlayerService.GetMediaPlayerService().Play();
                base.OnPlay();
            }

            public override void OnSkipToNext()
            {
                mediaPlayerService.GetMediaPlayerService().PlayNext();
                base.OnSkipToNext();
            }

            public override void OnSkipToPrevious()
            {
                mediaPlayerService.GetMediaPlayerService().PlayPrevious();
                base.OnSkipToPrevious();
            }

            public override void OnStop()
            {
                mediaPlayerService.GetMediaPlayerService().Stop();
                base.OnStop();
            }
        }
    }

    public class MediaPlayerServiceBinder : Binder
    {
        private BackgroundStreamingService service;

        public MediaPlayerServiceBinder(BackgroundStreamingService service)
        {
            this.service = service;
        }

        public BackgroundStreamingService GetMediaPlayerService()
        {
            return service;
        }
    }*/
}