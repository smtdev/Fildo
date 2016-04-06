namespace Fildo.Core.ViewModels
{
    using Entities;
    using IPlatform;
    using Others;
    using Resources;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Platform;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using MvvmCross.Plugins.Share;

    public abstract class BaseViewModel : MvxViewModel
    {
        public event EventHandler ShowAds;
        
        private List<MenuItem> menuItems;
        private string toSearch;
        private const int major = 1;
        private const int medium = 1;
        private const int minor = 2;
        private bool isBusy;
        private bool newVersion;
        private bool noInternet;
        private ICommand menuItemSelectedCommand;

        protected readonly INetEase netEase;
        protected readonly INetwork network;
        protected readonly IDialog dialog;
        private ICommand searchCommand;
        private string playingSong;
        private string playingArtist;
        private MvxCommand playPauseCommand;
        private MvxCommand nextCommand;
        private MvxCommand playerQueueCommand;
        private bool showMenu;
        private string headerMenuText;
        private string picUrl;
        private int playingArtistPercent;

        public List<MenuItem> MenuItems
        {
            get { return this.menuItems; }
            set
            {
                this.menuItems = value;
                this.RaisePropertyChanged(() => this.MenuItems);
            }
        }

        public string PicUrl
        {
            get
            {
                return this.picUrl;
            }

            set
            {
                this.picUrl = value;
                this.RaisePropertyChanged(() => this.PicUrl);
            }
        }

        public string ToSearch
        {
            get { return this.toSearch; }
            set
            {
                if (value.LastIndexOf("\n") <= -1)
                {
                    this.toSearch = value;
                }
                else
                {
                    this.ShowMenu = false;
                    this.Search();
                }

                this.RaisePropertyChanged(() => this.ToSearch);
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                this.searchCommand = this.searchCommand ?? new MvxCommand(this.Search);
                return this.searchCommand;
            }
        }

        public ICommand MenuItemSelectedCommand
        {
            get
            {
                this.menuItemSelectedCommand = this.menuItemSelectedCommand ?? new MvxCommand<MenuItem>(this.MenuItemSelectedHandler);
                return this.menuItemSelectedCommand;
            }
        }

        public ICommand PlayPauseCommand
        {
            get
            {
                this.playPauseCommand = this.playPauseCommand ?? new MvxCommand(this.PlayPauseCommandHandler);
                return this.playPauseCommand;
            }
        }

        public ICommand NextCommand
        {
            get
            {
                this.nextCommand = this.nextCommand ?? new MvxCommand(this.NextCommandHandler);
                return this.nextCommand;
            }
        }

        public ICommand PlayerQueueCommand
        {
            get
            {
                this.playerQueueCommand = this.playerQueueCommand ?? new MvxCommand(this.playerQueueCommandHandler);
                return this.playerQueueCommand;
            }
        }

        private void playerQueueCommandHandler()
        {
            this.ShowViewModel<PlayerViewModel>();
        }

        private void NextCommandHandler()
        {
            Mvx.Resolve<IPlayer>().Next();
        }

        private void PlayPauseCommandHandler()
        {
            try
            {
                Mvx.Resolve<IPlayer>().PlayPause();
            }
            catch
            {
                // Ignored
            }
        }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set {
                this.isBusy = value;
                this.RaisePropertyChanged(() => this.IsBusy); }
        }

        public bool NewVersion
        {
            get { return this.newVersion; }
            set {
                this.newVersion = value;
                this.RaisePropertyChanged(() => this.NewVersion); }
        }

        public bool ShowMenu
        {
            get { return this.showMenu; }
            set {
                this.showMenu = value;
                this.RaisePropertyChanged(() => this.ShowMenu); }
        }

        public string HeaderMenuText
        {
            get
            {
                return this.headerMenuText;
            }

            set
            {
                this.headerMenuText = value;
                this.RaisePropertyChanged(() => this.HeaderMenuText);
            }
        }
        
        public bool NoInternet
        {
            get { return this.noInternet; }
            set
            {
                this.noInternet = value;
                this.RaisePropertyChanged(() => this.NoInternet);
            }
        }

        public BaseViewModel(INetEase netEase, INetwork network, IDialog dialog)
        {
            this.netEase = netEase;
            this.network = network;
            this.dialog = dialog;
            this.MenuItems = new List<MenuItem>();
        }

        private async void CheckVersion()
        {
            int versionWeb = await this.netEase.GetVersion(1);
            int version = (major * 10000) + (medium * 100) + minor;
            if (version < versionWeb)
            {
                this.NewVersion = true;
            }
        }

        private async void MenuItemSelectedHandler(MenuItem menuItem)
        {
            if (menuItem.ViewModel == typeof(ListViewModel))
            {
                if (this.ShowAds != null)
                {
                    this.ShowAds(this, new EventArgs());
                }
                this.IsBusy = true;
                var playlists = await this.netEase.GetPublicPlaylists();
                if (playlists.Count > 0)
                {
                    Container.Playlists = playlists;
                    Container.ArePublic = false;
                    this.ShowViewModel<ListViewModel>(new { playlist = playlists }); ;
                }
                this.IsBusy = false;
            }
            else if (menuItem.ViewModel == null)
            {
                if (menuItem.Title == Texts.MenuShare)
                {
                    Mvx.Resolve<IMvxShareTask>().ShareLink("Fildo", Texts.ShareText, "http://fildo.net/android");
                }
            }
            else
            {
                this.ShowViewModel(menuItem.ViewModel);
            }

            this.ShowMenu = false;
        }

        public virtual void Search()
        {
            this.ShowViewModel<SearchResultViewModel>(new { toSearch = this.ToSearch });
        }

        public virtual void CloseVM()
        {
            this.Close(this);
        }


        public string GetString(string id, CultureInfo ci)
        {
            ResourceManager rm = Texts.ResourceManager;
            return rm.GetString(id, ci);
        }

        public string PlayingArtist
        {
            get { return this.playingArtist; }
            set
            {
                this.playingArtist = value;
                this.RaisePropertyChanged(() => this.PlayingArtist);
            }
        }

        public string PlayingSong
        {
            get { return this.playingSong; }
            set
            {
                this.playingSong = value;
                this.RaisePropertyChanged(() => this.PlayingSong);
            }
        }

        public int PlayingArtistPercent
        {
            get { return this.playingArtistPercent; }
            set
            {
                this.playingArtistPercent = value;
                this.RaisePropertyChanged(() => this.PlayingArtistPercent);
            }
        }

        public async void ShowPlayer()
        {
            await Task.Delay(1000);
            this.ShowViewModel<PlayerViewModel>();
        }

        public void SetCulture(CultureInfo ci)
        {
            var persist = Mvx.Resolve<IPersist>();
            string username = persist.GetString("Username");
            Texts.Culture = ci;
            this.MenuItems = new List<MenuItem>();
            this.MenuItems.Add(new MenuItem() { Image = "res:icon", Title = "Home", ViewModel = typeof(MainViewModel) });
            this.MenuItems.Add(new MenuItem() { Image = "res:listmenu", Title = Texts.MenuLast100PL, ViewModel = typeof(ListViewModel) });
            if (string.IsNullOrEmpty(username))
            {
                this.MenuItems.Add(new MenuItem() { Image = "res:login", Title = Texts.MenuLoginPL, ViewModel = typeof(LoginViewModel) });
            }
            else
            {
                this.MenuItems.Add(new MenuItem() { Image = "res:playlistmenu", Title = Texts.MenuMyPL, ViewModel = typeof(LoginViewModel) });
            }
            this.MenuItems.Add(new MenuItem() { Image = "res:netease", Title = Texts.MenuImportPLNetease, ViewModel = typeof(ImportNeteaseViewModel) });
            this.MenuItems.Add(new MenuItem() { Image = "res:configmenu", Title = Texts.MenuConfiguration, ViewModel = typeof(ConfigurationViewModel) });
            this.MenuItems.Add(new MenuItem() { Image = "res:downloadmenu", Title = Texts.MenuDownloadQueue, ViewModel = typeof(DownloadViewModel) });
            this.MenuItems.Add(new MenuItem() { Image = "res:playlistmenu", Title = Texts.MenuPlayerQueue, ViewModel = typeof(PlayerViewModel) });
            this.MenuItems.Add(new MenuItem() { Image = "res:share", Title = Texts.MenuShare, ViewModel = null });
            this.MenuItems.Add(new MenuItem() { Image = "res:infomenu", Title = "Info", ViewModel = typeof(InfoViewModel) });
            this.CheckVersion();
            if (!this.network.HasInternet())
            {
                this.NoInternet = true;
            }
        }
    }
}
