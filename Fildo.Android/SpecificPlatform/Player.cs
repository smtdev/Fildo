namespace Fildo.Droid.SpecificPlatform
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Android.App;
    using Android.Content;
    using Android.Media;
    using Android.Widget;

    using Com.Google.Android.Exoplayer;

    using Fildo.Core;
    using Fildo.Core.Entities;
    using Fildo.Core.IPlatform;
    using Fildo.Droid.Services;

    using AudioTrack = Com.Google.Android.Exoplayer.Audio.AudioTrack;

    public class Player : IPlayer, MediaCodecAudioTrackRenderer.IEventListener
    {
        private readonly INetEase netEase;

        private MediaCodecAudioTrackRenderer aRenderer;

        private IExoPlayer exoPlayer;

        private IExoPlayer mediaPlayer;

        //private MediaPlayerServiceBinder binder;
        //MediaPlayerServiceConnection mediaPlayerServiceConnection;
        private Intent mediaPlayerServiceIntent;

        private Song playedSong;

        private FrameworkSampleSource sampleSource;

        public Player(INetEase netEase)
        {
            BackgroundStreamingService.QueueChanged += this.BackgroundStreamingService_QueueChanged;
            this.netEase = netEase;
            //mediaPlayerServiceIntent = new Intent(Android.App.Application.Context, typeof(BackgroundStreamingService));
            //mediaPlayerServiceConnection = new MediaPlayerServiceConnection(this);
            //Android.App.Application.Context.BindService(mediaPlayerServiceIntent, mediaPlayerServiceConnection, Bind.AutoCreate);
        }

        public IntPtr Handle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void OnAudioTrackInitializationError(AudioTrack.InitializationException p0)
        {
            throw new NotImplementedException();
        }

        public void OnAudioTrackUnderrun(int p0, long p1, long p2)
        {
            throw new NotImplementedException();
        }

        public void OnAudioTrackWriteError(AudioTrack.WriteException p0)
        {
            throw new NotImplementedException();
        }

        public void OnCryptoError(MediaCodec.CryptoException p0)
        {
            throw new NotImplementedException();
        }

        public void OnDecoderInitializationError(MediaCodecTrackRenderer.DecoderInitializationException p0)
        {
            throw new NotImplementedException();
        }

        public void OnDecoderInitialized(string p0, long p1, long p2)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<int> ProgressChanged;

        public event EventHandler<int> SongFinished;

        public event EventHandler QueueChanged;

        public ObservableCollection<Song> PlayerQueue
        {
            get
            {
                return BackgroundStreamingService.Queue;
            }
        }

        public async void PlayWithoutClear(Song song)
        {
            song = await this.netEase.FixUrl(song);
            BackgroundStreamingService.CurrentTrack = song.Url;
            var intent = new Intent(BackgroundStreamingService.ActionPlayWithoutClear);
            Application.Context.StartService(intent);
        }

        public async void Play(Song song)
        {
            song = await this.netEase.FixUrl(song);

            BackgroundStreamingService.Queue.Clear();
            BackgroundStreamingService.Queue.Add(song);
            var intent = new Intent(BackgroundStreamingService.ActionPlay);
            var componentName = Application.Context.StartService(intent);

            if (componentName.PackageName != "net.fildo.app")
            {
                Toast.MakeText(
                    Application.Context,
                    "Error: Cannot start background audio service. Check your permissions.",
                    ToastLength.Long);
            }

            //if (mediaPlayer == null)
            //{
            //    mediaPlayer = Com.Google.Android.Exoplayer.ExoPlayerFactory.NewInstance(1);
            //    //mediaPlayer.AddListener(this);
            //}

            //Android.Net.Uri soundString = Android.Net.Uri.Parse("http://p3.music.126.net/Vlz_T2iKjY0lB_JzFF6bgw==/1895558046287361.mp3");
            //if (sampleSource != null)
            //{
            //    sampleSource = null;
            //}
            //if (aRenderer != null)
            //{
            //    aRenderer = null;
            //}
            //try
            //{
            //    sampleSource = new FrameworkSampleSource(BackgroundStreamingService.Main, soundString, null);

            //    aRenderer = new MediaCodecAudioTrackRenderer(sampleSource, MediaCodecSelector.Default);
            //    mediaPlayer.Prepare(aRenderer);
            //    mediaPlayer.PlayWhenReady = true;
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
        }

        public void PlayAll(List<Song> songs, bool clear)
        {
            if (clear)
            {
                BackgroundStreamingService.Queue.Clear();
            }
            foreach (Song song1 in songs)
            {
                Application.SynchronizationContext.Post(
                    _ =>
                    {
                        BackgroundStreamingService.Queue.Add(song1);
                    }, 
                    null);
            }
            
            if (clear)
            {
                var intent = new Intent(BackgroundStreamingService.ActionPlay);
                Application.Context.StartService(intent);
            }
        }

        //private void MediaPlayer_Completion(object sender, EventArgs e)
        //{
        //    if (this.SongFinished != null)
        //    {
        //        this.SongFinished(this, 0);
        //    }
        //}

        public void Stop(Song song)
        {
            var intent = new Intent(BackgroundStreamingService.ActionStop);
            Application.Context.StartService(intent);
        }

        public void PlayPause()
        {
            try
            {
                var intent = new Intent(BackgroundStreamingService.ActionPause);
                Application.Context.StartService(intent);
            }
            catch
            {
                // ignored
            }
        }

        public void Next()
        {
            var intent = new Intent(BackgroundStreamingService.ActionNext);
            Application.Context.StartService(intent);
        }

        private void BackgroundStreamingService_QueueChanged(object sender, EventArgs e)
        {
            this.QueueChanged?.Invoke(this, e);
        }

        /*class MediaPlayerServiceConnection : Java.Lang.Object, IServiceConnection
        {
            Player player;
            public MediaPlayerServiceConnection(IPlayer player)
            {
                this.player = (Player)player;
            }
            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                var mediaPlayerServiceBinder = service as MediaPlayerServiceBinder;
                if (mediaPlayerServiceBinder != null)
                {
                    var binder = (MediaPlayerServiceBinder)service;
                    player.binder = binder;
                    

                    //binder.GetMediaPlayerService().Playing += (object sender, EventArgs e) => { if (instance.Playing != null) instance.Playing(sender, e); };
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
             
            }
        }*/
    }
}