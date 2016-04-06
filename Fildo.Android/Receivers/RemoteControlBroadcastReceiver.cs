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
using Fildo.Droid.Services;

namespace Fildo.Droid.Receivers
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionMediaButton })]
    public class RemoteControlBroadcastReceiver : BroadcastReceiver
    {

        /// <summary>
        /// gets the class name for the component
        /// </summary>
        /// <value>The name of the component.</value>
        public string ComponentName { get { return this.Class.Name; } }

        /// <Docs>The Context in which the receiver is running.</Docs>
        /// <summary>
        /// When we receive the action media button intent
        /// parse the key event and tell our service what to do.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="intent">Intent.</param>
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != Intent.ActionMediaButton)
                return;

            //The event will fire twice, up and down.
            // we only want to handle the down event though.
            var key = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);
            if (key.Action != KeyEventActions.Down)
                return;

            var action = BackgroundStreamingService.ActionPlay;

            switch (key.KeyCode)
            {
                case Keycode.Headsethook:
                case Keycode.MediaPlayPause:
                    action = null;
                    if (BackgroundStreamingService.Player.PlayWhenReady)
                    {
                        BackgroundStreamingService.Player.PlayWhenReady = false;
                    }
                    else
                    {
                        BackgroundStreamingService.Player.PlayWhenReady = true;
                    }
                    
                    break;
                case Keycode.MediaPlay:
                    action = BackgroundStreamingService.ActionPlay;
                    break;
                case Keycode.MediaPause:
                    action = null;
                    BackgroundStreamingService.Player.PlayWhenReady = false;
                    break;
                case Keycode.MediaStop:
                    action = BackgroundStreamingService.ActionStop;
                    break;
                case Keycode.MediaNext:
                    action = BackgroundStreamingService.ActionNext;
                    break;
                case Keycode.MediaPrevious:
                    action = BackgroundStreamingService.ActionPrev;
                    break;
                default:
                    return;
            }
            if (action != null)
            {
                var remoteIntent = new Intent(action);
                context.StartService(remoteIntent);
            }
        }
    }
}