namespace Fildo.Droid.Fragments
{
    using System.Globalization;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Support.V4.Widget;
    using Android.Views;
    using Android.Widget;
    using Bindables;
    using Core.ViewModels;

    using Fildo.Droid.Services;

    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Binding.Droid.BindingContext;
    using MvvmCross.Droid.FullFragging.Fragments;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using Views;
    using Toolbar = Android.Support.V7.Widget.Toolbar;

    [Activity(Label = "Import from Netease", NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class ImportNeteaseView : MvxFragment
    {
        private CultureInfo cultureInfo;
        private BindableProgress progress;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.ImportNetease, null);
            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.cultureInfo = ((MainView) this.Activity).CultureInfo;
            if (!string.IsNullOrEmpty(((BaseViewModel) ((MainView) this.Activity).ViewModel).PlayingArtist))
            {
                ((MainView) this.Activity).FindViewById<LinearLayout>(Resource.Id.miniPlayer).Visibility =
                    ViewStates.Visible;
            }
            string hash = prefs.GetString("UserHash", string.Empty);
            string idUser = prefs.GetString("IdUser", string.Empty);
            ((ImportNeteaseViewModel) this.ViewModel).Hash = hash;
            ((ImportNeteaseViewModel) this.ViewModel).IdUser = idUser;
            if (!string.IsNullOrEmpty(hash))
            {
                LinearLayout hashash = view.FindViewById<LinearLayout>(Resource.Id.hashash);
                LinearLayout nohash = view.FindViewById<LinearLayout>(Resource.Id.nohash);
                hashash.Visibility = ViewStates.Visible;
                nohash.Visibility = ViewStates.Gone;
            }
            //toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);

            //textView1
            if (this.ViewModel != null)
            {
                view.FindViewById<TextView>(Resource.Id.textView1).Text =
                    ((BaseViewModel) this.ViewModel).GetString("LoginBeforeImport", this.cultureInfo);
                view.FindViewById<TextView>(Resource.Id.importNetease).Text =
                    ((BaseViewModel) this.ViewModel).GetString("ImportNetease", this.cultureInfo);
            }

            /*view.SetSupportActionBar(toolbar);
            view.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            view.SupportActionBar.SetHomeButtonEnabled(true);
            */
            this.progress = new BindableProgress(view.Context, this.ViewModel);

            var set = this.CreateBindingSet<ImportNeteaseView, ImportNeteaseViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(this.progress).For(p => p.NoInternet).To(vm => vm.NoInternet);
            set.Apply();
            GAService.GetGASInstance().Track_App_Page("Import Netease");
            return view;
        }
    }
}