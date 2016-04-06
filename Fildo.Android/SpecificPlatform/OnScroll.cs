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
using Fildo.Core.Entities;
using Fildo.Core.IPlatform;

namespace Fildo.Droid.SpecificPlatform
{
    public class OnScroll : AbsListView.IOnScrollListener
    {
        public IntPtr Handle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void OnScrollStateChanged(AbsListView view, [GeneratedEnum] ScrollState scrollState)
        {
            throw new NotImplementedException();
        }

        void AbsListView.IOnScrollListener.OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
        {
            throw new NotImplementedException();
        }
    }
}