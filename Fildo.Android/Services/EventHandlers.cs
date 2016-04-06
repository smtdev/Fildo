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

namespace Fildo.Droid.Services
{
    public class EventHandlers
    {
        public delegate void StatusChangedEventHandler(object sender, EventArgs e);

        public delegate void BufferingEventHandler(object sender, EventArgs e);

        public delegate void CoverReloadedEventHandler(object sender, EventArgs e);

        public delegate void PlayingEventHandler(object sender, EventArgs e);
    }
}