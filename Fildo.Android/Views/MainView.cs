namespace Fildo.Droid.Views
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    using Acr.UserDialogs;

    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Runtime;
    using Android.Support.V4.Widget;
    using Android.Views;
    using Android.Widget;

    using Fildo.Core.ViewModels;
    using Fildo.Droid.Bindables;
    using Fildo.Droid.Fragments;
    using Fildo.Droid.Services;

    using MvvmCross.Binding.BindingContext;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Droid.Platform;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using MvvmCross.Droid.Views;
    using MvvmCross.Platform;

    using Toolbar = Android.Support.V7.Widget.Toolbar;

    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainView : MvxAppCompatActivity
    {
        private static BaseViewModel baseViewModel;
        DrawerLayout menuDrawerLayout;
        private MvxActionBarDrawerToggle menuDrawerToggle;
        private BindableProgress progress;

        private bool showMenu;
        private Toolbar toolbar;

        public CultureInfo CultureInfo { get; private set; }

        public bool IsCaptchaShow { get; set; }

        public bool ShowMenu
        {
            get
            {
                return this.showMenu;
            }

            set
            {
                this.showMenu = value;

                if (value)
                {
                    this.menuDrawerLayout.OpenDrawer((int)GravityFlags.Left);
                }
                else
                {
                    this.menuDrawerLayout.CloseDrawer((int)GravityFlags.Left);
                }
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            AndroidEnvironment.UnhandledExceptionRaiser += this.AndroidEnvironment_UnhandledExceptionRaiser;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += this.HandleExceptions;

            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;

            UserDialogs.Init(this);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            // ensure the initialization is done
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(this.ApplicationContext);
            setup.EnsureInitialized();

            if (this.Intent.Extras.ContainsKey("notificationPlayList"))
            {
                if (this.ViewModel != null)
                {
                    ((BaseViewModel)this.ViewModel).ShowPlayer();
                }
                else
                {
                    var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
                    this.ViewModel = loaderService.LoadViewModel(new MvxViewModelRequest(typeof(MainViewModel), null, null, null), null) as MainViewModel;
                    ((BaseViewModel)this.ViewModel).ShowPlayer();
                }
            }

            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.Main);

            var presenter = (DroidPresenter)Mvx.Resolve<IMvxAndroidViewPresenter>();
            var initialFragment = new MainContentView { ViewModel = this.ViewModel };

            presenter.RegisterFragmentManager(this.FragmentManager, initialFragment);

            baseViewModel = (BaseViewModel)this.ViewModel;

            var prefs = Application.Context.GetSharedPreferences("Fildo", FileCreationMode.Private);
            this.CultureInfo = new CultureInfo(prefs.GetString("CultureForced", Thread.CurrentThread.CurrentUICulture.Name));

            ((BaseViewModel)this.ViewModel).SetCulture(this.CultureInfo);

            this.toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.menuDrawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.mainContainer);
            this.menuDrawerToggle = new MvxActionBarDrawerToggle(
                this,
                // host Activity
                this.menuDrawerLayout,
                // DrawerLayout object

                Resource.String.Login,
                // "open drawer" description
                Resource.String.Login // "close drawer" description
                );
            this.menuDrawerLayout.SetDrawerListener(this.menuDrawerToggle);

            this.SetSupportActionBar(this.toolbar);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            this.SupportActionBar.SetHomeButtonEnabled(true);

            this.progress = new BindableProgress(this, this.ViewModel);

            var set = this.CreateBindingSet<MainView, MainViewModel>();
            set.Bind(this.progress).For(p => p.Visible).To(vm => vm.IsBusy);
            set.Bind(this.progress).For(p => p.NoInternet).To(vm => vm.NoInternet);
            set.Bind(this.progress).For(p => p.NewVersion).To(vm => vm.NewVersion);
            set.Bind(this).For(p => p.ShowMenu).To(vm => vm.ShowMenu);
            set.Apply();

            if (prefs.GetBoolean("FirstTime", true))
            {
                this.menuDrawerLayout.OpenDrawer((int)GravityFlags.Start);
                var prefEditor = prefs.Edit();
                prefEditor.PutBoolean("FirstTime", false);
                prefEditor.Commit();
            }
            BackgroundStreamingService.Main = this;

            BackgroundStreamingService.SongNameChanged -= this.BackgroundStreamingService_SongNameChanged;
            BackgroundStreamingService.percentChanged -= this.BackgroundStreamingService_posChanged;
            BackgroundStreamingService.SongNameChanged += this.BackgroundStreamingService_SongNameChanged;
            BackgroundStreamingService.percentChanged += this.BackgroundStreamingService_posChanged;

            GAService.GetGASInstance().Initialize(this);
            GAService.GetGASInstance().Track_App_Page("Fildo Main Activity");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
        }

        private void BackgroundStreamingService_posChanged(object sender, int e)
        {
            if (this.ViewModel != null)
            {
                ((BaseViewModel)this.ViewModel).PlayingArtistPercent = e;
            }
        }

        private void HandleExceptions(object sender, UnhandledExceptionEventArgs e)
        {
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            // Changed for public publish
            e.Handled = true;
        }

        public override void OnBackPressed()
        {
            if (this.FragmentManager.BackStackEntryCount > 0)
            {
                this.FragmentManager.PopBackStack();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            return base.OnKeyDown(keyCode, e);
        }

        private void BackgroundStreamingService_SongNameChanged(object sender, string e)
        {
            ((BaseViewModel)this.ViewModel).PlayingArtist = e;
        }

        private void Intlistener_AdClosed()
        {
            this.Finish();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.menuDrawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            this.menuDrawerToggle.SyncState();
        }

        protected override void OnResume()
        {
            base.OnResume();

            var playpause = this.FindViewById<ToggleButton>(Resource.Id.btnPlayPause);
            var miniPlayerProgress = this.FindViewById<ProgressBar>(Resource.Id.miniPlayerProgress);
            if (BackgroundStreamingService.Player != null)
            {
                if (this.ViewModel != null)
                {
                    ((BaseViewModel)this.ViewModel).PlayingArtist = BackgroundStreamingService.SongName;
                    var set = this.CreateBindingSet<MainView, MainViewModel>();
                    set.Bind(miniPlayerProgress).For(p => p.Progress).To(vm => vm.PlayingArtistPercent);
                    set.Apply();
                    if (BackgroundStreamingService.Player.PlayWhenReady)
                    {
                        playpause.Checked = true;
                    }
                    else
                    {
                        playpause.Checked = false;
                    }
                }
            }
        }
    }
}