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
    public class LoginView : MvxFragment
    {
        private Bindables.BindableProgress progress;

        private bool logged;

        public bool Logged
        {
            get { return this.logged; }
            set
            {
                this.logged = value;
                if (value)
                {
                    var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
                    var prefEditor = prefs.Edit();
                    prefEditor.PutString("Username", this.username.Text);
                    prefEditor.PutString("Password", this.password.Text);
                    prefEditor.PutString("UserHash", ((LoginViewModel) this.ViewModel).Hash);
                    prefEditor.PutString("IdUser", ((LoginViewModel) this.ViewModel).IdUser);
                    prefEditor.Commit();

                    ((BaseViewModel)((MainView)this.Activity).ViewModel).HeaderMenuText = this.username.Text;
                    var loginMenuItem = ((BaseViewModel)((MainView)this.Activity).ViewModel).MenuItems.FirstOrDefault(p => p.Image == "res:login");
                    if (loginMenuItem != null)
                    {
                        loginMenuItem.Image = "res:playlistmenu";
                        loginMenuItem.Title = ((BaseViewModel) this.ViewModel).GetString("MenuMyPL", this.cultureInfo);
                        loginMenuItem.ViewModel = typeof(LoginViewModel);
                    }
                }
            }
        }

        private Android.Widget.EditText username;
        private Android.Widget.EditText password;
        private CultureInfo cultureInfo;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View view = this.BindingInflate(Resource.Layout.LoginLayout, null);

            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.cultureInfo = ((MainView)this.Activity).CultureInfo;
            if (!string.IsNullOrEmpty(((BaseViewModel)((MainView)this.Activity).ViewModel).PlayingArtist))
            {
                ((MainView)this.Activity).FindViewById<Android.Widget.LinearLayout>(Resource.Id.miniPlayer).Visibility = ViewStates.Visible;
            }
            this.progress = new Bindables.BindableProgress(view.Context, this.ViewModel);

            var set = this.CreateBindingSet<LoginView, LoginViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(this.progress).For(p => p.NoInternet).To(vm => vm.NoInternet);
            set.Bind(this).For(p => p.Logged).To(vm => vm.Logged);
            set.Apply();

            this.username = view.FindViewById<Android.Widget.EditText>(Resource.Id.UsernameEditText);
            this.password = view.FindViewById<Android.Widget.EditText>(Resource.Id.PasswordEditText);

            this.username.Text = prefs.GetString("Username", string.Empty);
            this.password.Text = prefs.GetString("Password", string.Empty);

            if (this.ViewModel != null)
            {
                view.FindViewById<Android.Widget.TextView>(Resource.Id.loginText).Text = ((BaseViewModel) this.ViewModel).GetString("LoginText", this.cultureInfo);
            }

            if (!string.IsNullOrEmpty(this.username.Text) && !string.IsNullOrEmpty(this.password.Text))
            {
                ((LoginViewModel) this.ViewModel).DoLogin();
                GAService.GetGASInstance().Track_App_Page("My Playlists");
                return this.BindingInflate(Resource.Layout.EmptyLayout, null);
            }

            GAService.GetGASInstance().Track_App_Page("Login");

            return view;
        }
    }
}