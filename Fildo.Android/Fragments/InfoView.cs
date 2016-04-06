namespace Fildo.Droid.Fragments
{
    using System.Globalization;
    using System.Linq;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Views;
    using Core.ViewModels;

    using Fildo.Droid.Services;

    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Binding.Droid.BindingContext;
    using MvvmCross.Droid.FullFragging.Fragments;
    using Views;

    [Activity(Label = "Login for Playlists", NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class InfoView : MvxFragment
    {
        private CultureInfo cultureInfo;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.LyricLayout, null);
            
            this.cultureInfo = ((MainView)this.Activity).CultureInfo;
           
            if (this.ViewModel != null)
            {
                view.FindViewById<Android.Widget.TextView>(Resource.Id.lyricContainer).Text = ((BaseViewModel) this.ViewModel).GetString("Info", this.cultureInfo);
            }
            

            return view;
        }
    }
}