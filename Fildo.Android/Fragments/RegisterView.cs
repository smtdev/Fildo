namespace Fildo.Droid.Fragments
{
    using System.Globalization;
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Views;
    using Core.ViewModels;

    using Fildo.Droid.Services;

    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Binding.Droid.BindingContext;
    using MvvmCross.Droid.FullFragging.Fragments;
    using Views;

    public class RegisterView : MvxFragment
    {
        private Bindables.BindableProgress progress;

        private CultureInfo cultureInfo;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.RegisterLayout, null);

            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.cultureInfo = ((MainView)this.Activity).CultureInfo;

            if (!string.IsNullOrEmpty(((BaseViewModel)((MainView)this.Activity).ViewModel).PlayingArtist))
            {
                ((MainView)this.Activity).FindViewById<Android.Widget.LinearLayout>(Resource.Id.miniPlayer).Visibility = ViewStates.Visible;
            }
            this.progress = new Bindables.BindableProgress(view.Context, this.ViewModel);

            var set = this.CreateBindingSet<RegisterView, RegisterViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(this.progress).For(p => p.NoInternet).To(vm => vm.NoInternet);
            set.Apply();
            GAService.GetGASInstance().Track_App_Page("Register");
            return view;
        }
    }
}