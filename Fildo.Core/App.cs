
using Acr.UserDialogs;
using Fildo.Core.IPlatform;
using Fildo.Core.Resources;
using Fildo.Core.Workers;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using System.Globalization;

namespace Fildo.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            /*CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();*/

            Mvx.RegisterSingleton<IUserDialogs>(() => UserDialogs.Instance);
            Mvx.RegisterSingleton<INetEase>(() => new NetEase());
            //Mvx.RegisterSingleton<IDownloadQueue>(Mvx.IocConstruct<DownloadQueue>());
            Mvx.LazyConstructAndRegisterSingleton<IDownloadQueue, DownloadQueue>();
            this.RegisterAppStart<ViewModels.MainViewModel>();
            
        }
    }
}