using Acr.UserDialogs;
using Android.Content;
using Android.Net;
using Fildo.Core;
using Fildo.Core.IPlatform;
using Fildo.Droid.Services;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Views;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using System.Globalization;

namespace Fildo.Droid
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new App();
        }
		
        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            var presenter = Mvx.IocConstruct<DroidPresenter>();

            Mvx.RegisterSingleton<IMvxAndroidViewPresenter>(presenter);

            return presenter;
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
            
            Mvx.RegisterSingleton<INetwork>(new SpecificPlatform.Network());
            
            Mvx.RegisterSingleton<IDialog>(new SpecificPlatform.DialogAndroid());
            Mvx.RegisterSingleton<IPersist>(new SpecificPlatform.PersistData());
            Mvx.RegisterSingleton<IPlayer>(new SpecificPlatform.Player(Mvx.Resolve<INetEase>()));
            Mvx.RegisterSingleton<IDownloader>(new SpecificPlatform.Downloader(Mvx.Resolve<INetEase>()));
            
        }

        protected override void InitializeIoC()
        {
            base.InitializeIoC();

            Mvx.ConstructAndRegisterSingleton<IFragmentTypeLookup, FragmentTypeLookup>();
        }
    }
}