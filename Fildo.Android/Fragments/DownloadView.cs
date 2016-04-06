namespace Fildo.Droid.Fragments
{
    using Android.App;
    using Android.Content.PM;
    using Android.OS;
    using Android.Views;
    using Android.Widget;
    using Bindables;
    using Core.ViewModels;

    using Fildo.Droid.Services;

    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Binding.Droid.BindingContext;
    using MvvmCross.Droid.FullFragging.Fragments;
    using Views;

    [Activity(Label = "Download Queue", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DownloadView : MvxFragment
    {
        private BindableProgress progress;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.DownloadQueue, null);
            if (!string.IsNullOrEmpty(((BaseViewModel) ((MainView) this.Activity).ViewModel).PlayingArtist))
            {
                ((MainView) this.Activity).FindViewById<LinearLayout>(Resource.Id.miniPlayer).Visibility =
                    ViewStates.Visible;
            }
            this.progress = new BindableProgress(view.Context, this.ViewModel);

            var set = this.CreateBindingSet<DownloadView, DownloadViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Apply();
            GAService.GetGASInstance().Track_App_Page("Download Queue");
            return view;
        }
    }
}