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
using Android.Gms.Ads;

namespace Fildo.Droid
{
    public class AdEventListener : AdListener
    {
        // Declare the delegate (if using non-generic pattern).
        public delegate void AdLoadedEvent();
        public delegate void AdClosedEvent();
        public delegate void AdOpenedEvent();



        // Declare the event.
        public event AdLoadedEvent AdLoaded;
        public event AdClosedEvent AdClosed;
        public event AdOpenedEvent AdOpened;

        public override void OnAdLoaded()
        {
            if (this.AdLoaded != null) this.AdLoaded();
            base.OnAdLoaded();
        }

        public override void OnAdClosed()
        {
            if (this.AdClosed != null) this.AdClosed();
            base.OnAdClosed();
        }
        public override void OnAdOpened()
        {
            if (this.AdOpened != null) this.AdOpened();
            base.OnAdOpened();
        }
    }
}