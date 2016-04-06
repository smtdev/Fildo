namespace Fildo.Core.ViewModels
{
    using Entities;
    using IPlatform;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System;
    using Others;
    using Resources;
    using MvvmCross.Core.ViewModels;
    using System.Linq;

    public class MainViewModel : BaseViewModel
    {
        private List<Album> topAlbums;
        private ICommand itemSelectedCommand;
        private List<AutocompleteSearch> topArtists;
        private MvxCommand<AutocompleteSearch> artistSelectedCommand;
        private List<NetEasePlaylist> topPlaylists;
        private MvxCommand<NetEasePlaylist> playlistSelectedCommand;
        private List<AutocompleteSearch> recommendations;
        private readonly IPersist persist;

        public MainViewModel(INetEase netEase, INetwork network, IDialog dialog, IPersist persist)
            : base(netEase, network, dialog)
        {
            this.persist = persist;
            string username = this.persist.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                this.HeaderMenuText = "Welcome \n" + username;
                var loginMenuItem = this.MenuItems.FirstOrDefault(p => p.Image == "res:login");
                if (loginMenuItem != null)
                {
                    loginMenuItem.Image = "res:listmenu";
                    loginMenuItem.Title = Texts.MenuMyPL;
                    loginMenuItem.ViewModel = typeof(LoginViewModel);
                }
            }
            else
            {
                this.HeaderMenuText = "Fildo \nSearch Music and download it!";
            }

            if (!this.NoInternet)
            {
                this.Init();
            }
        }
     
        public List<Album> TopAlbums
        {
            get { return this.topAlbums; }
            set
            {
                this.topAlbums = value;
                this.RaisePropertyChanged(() => this.TopAlbums);
            }
        }

        public List<AutocompleteSearch> TopArtists
        {
            get { return this.topArtists; }
            set
            {
                this.topArtists = value;
                this.RaisePropertyChanged(() => this.TopArtists);
            }
        }

        public List<AutocompleteSearch> Recommendations
        {
            get { return this.recommendations; }
            set
            {
                this.recommendations = value;
                this.RaisePropertyChanged(() => this.Recommendations);
            }
        }

        public List<NetEasePlaylist> TopPlaylists
        {
            get { return this.topPlaylists; }
            set
            {
                this.topPlaylists = value;
                this.RaisePropertyChanged(() => this.TopPlaylists);
            }
        }

        public ICommand ArtistSelectedCommand
        {
            get
            {
                this.artistSelectedCommand = this.artistSelectedCommand ?? new MvxCommand<AutocompleteSearch>(this.SelectArtistHandler);
                return this.artistSelectedCommand;
            }
        }

        public ICommand PlaylistSelectedCommand
        {
            get
            {
                this.playlistSelectedCommand = this.playlistSelectedCommand ?? new MvxCommand<NetEasePlaylist>(this.SelectPlaylistHandler);
                return this.playlistSelectedCommand;
            }
        }


        public ICommand ItemSelectedCommand
        {
            get
            {
                this.itemSelectedCommand = this.itemSelectedCommand ?? new MvxCommand<Album>(this.SelectItemHandler);
                return this.itemSelectedCommand;
            }
        }

        private void SelectItemHandler(Album item)
        {
            this.ShowViewModel<ListSongViewModel>(item);
        }

        private void SelectArtistHandler(AutocompleteSearch item)
        {
            if (string.IsNullOrEmpty(item.ResultType))
            {
                item.ResultType = "Artist";
            }

            this.ShowViewModel<ListSongViewModel>(item);
        }

        private void SelectPlaylistHandler(NetEasePlaylist item)
        {
            this.ShowViewModel<ListSongViewModel>(item);
        }

        private async void Init()
        {
            this.IsBusy = true;
            try
            {
                this.Recommendations = await this.netEase.GetRecommendations();
                this.TopAlbums = await this.netEase.GetTopAlbums();
                this.TopArtists = await this.netEase.GetTopArtists();
                this.TopPlaylists = this.netEase.GetNetEasePlaylist();
            }
            catch(Exception)
            {
                this.CheckErrors();
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void CheckErrors()
        {
            if ((this.TopAlbums == null) || (this.TopAlbums.Count == 0))
            {
                this.dialog.ShowAlert(Texts.TopAlbumError, 4000);
            }
            if ((this.TopArtists == null) || (this.TopArtists.Count == 0))
            {
                this.dialog.ShowAlert(Texts.TopArtistError, 4000);
            }
            if ((this.TopPlaylists == null) || (this.TopPlaylists.Count == 0))
            {
                this.dialog.ShowAlert(Texts.TopPlaylistError, 4000);
            }
        }
    }
}
