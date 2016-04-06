namespace Fildo.Droid
{
    using Android.App;
    using Android.Content.PM;
    using Droid;
    using MvvmCross.Droid.Views;

    [Activity(
		Label = "Fildo"
		, MainLauncher = true
		, Icon = "@drawable/icon"
		, Theme = "@style/Theme.Splash"
		, NoHistory = true
		, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {
        }
    }
}