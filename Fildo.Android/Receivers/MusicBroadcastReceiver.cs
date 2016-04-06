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
using Fildo.Droid.Services;

namespace Fildo.Droid.Receivers
{
    /// <summary>
    /// This is a simple intent receiver that is used to stop playback
    /// when audio become noisy, such as the user unplugged headphones
    /// </summary>
    [BroadcastReceiver]
    [Android.App.IntentFilter(new[] { AudioManager.ActionAudioBecomingNoisy })]
    public class MusicBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != AudioManager.ActionAudioBecomingNoisy)
                return;

            //signal the service to stop!
            var stopIntent = new Intent(BackgroundStreamingService.ActionStop);
            context.StartService(stopIntent);
        }
    }
}