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
using Fildo.Core.IPlatform;
using Android.Support.Design.Widget;

namespace Fildo.Droid.SpecificPlatform
{
    public class DialogAndroid : IDialog
    {
        public void ShowAlert(string message, int duration)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }
    }
}