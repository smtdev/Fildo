namespace Fildo.Core.ViewModels
{
    using Entities;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System;
    using IPlatform;
    using System.Collections.ObjectModel;
    using Wrappers;
    using System.Linq;
    using Workers;
    using Resources;
    using System.Globalization;
    using System.Threading.Tasks;
    using MvvmCross.Core.ViewModels;
    using Acr.UserDialogs;
    using MvvmCross.Platform;
    public class ListSongViewModel : BaseViewModel
    {
        private readonly IDownloadQueue downloadQueue;
        private readonly IPlayer player;
        private bool isArtist;
        private int progress;
        private bool isAlbum;
        private bool isPlaylist;
        private string playlistName;
        private string albumName;
        private ICommand itemSelectedCommand;
        private ICommand playCommand;
        private ObservableCollection<ItemWrap> songs;
        private List<Album> albums;
        private bool downloadAll;

        private string picUrl;
        private bool isDownloading;
        private MvxCommand downloadAllCommand;
        private string titleView;
        private List<AutocompleteSearch> similarArtists;
        private MvxCommand<AutocompleteSearch> similarArtistSelectedCommand;
        private List<SongListInitItem> historyItems;
        private SongListInitItem currentItem;
        private string plid;
        private string userIdToSavePL;
        private string hashToSavePL;
        private readonly IUserDialogs userDialog;

        public ListSongViewModel(IUserDialogs userDialog, INetEase netEase, IDownloadQueue downloadQueue, IPlayer player, INetwork network, IDialog dialog)
            : base(netEase, network, dialog)
        {
            this.userDialog = userDialog;
            this.downloadAll = false;
            this.downloadQueue = downloadQueue;
            this.player = player;

            this.player.ProgressChanged -= this.Player_ProgressChanged;
            this.player.ProgressChanged += this.Player_ProgressChanged;
            this.player.SongFinished -= this.Player_SongFinished;
            this.player.SongFinished += this.Player_SongFinished;
            this.historyItems = new List<SongListInitItem>();
        }

        public ObservableCollection<ItemWrap> Songs
        {
            get { return this.songs; }
            set
            {
                this.songs = value;
                this.RaisePropertyChanged(() => this.Songs);
            }
        }

        public List<Album> Albums
        {
            get { return this.albums; }
            set
            {
                this.albums = value;
                this.RaisePropertyChanged(() => this.Albums);
            }
        }

        public List<AutocompleteSearch> SimilarArtists
        {
            get { return this.similarArtists; }
            set
            {
                this.similarArtists = value;
                this.RaisePropertyChanged(() => this.SimilarArtists);
            }
        }

        public bool IsArtist
        {
            get { return this.isArtist; }
            set
            {
                this.isArtist = value;
                this.RaisePropertyChanged(() => this.IsArtist);
            }
        }

        public bool IsPlaylist()
        {
            if (string.IsNullOrEmpty(this.playlistName))
            {
                return false;
            }

            return true;
        }
        public string TitleView
        {
            get
            {
                if (this.IsArtist)
                {
                    if ((this.Songs == null) || (this.Songs.Count == 0))
                    {
                        return Texts.SongsAndAlbums;
                    }
                    return this.Songs.First().Item.Artist;
                }
                else
                {
                    if (this.isAlbum)
                    {
                        return this.albumName;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.playlistName))
                        {
                            return this.playlistName;
                        }
                        else
                        {
                            return Texts.SongsAndAlbums;
                        }
                    }
                }
            }
        }


        public int Progress
        {
            get { return this.progress; }
            set
            {
                this.progress = value;
                this.RaisePropertyChanged(() => this.Progress);
            }
        }

        public bool IsDownloading
        {
            get { return this.isDownloading; }
            set
            {
                this.isDownloading = value;
                this.RaisePropertyChanged(() => this.IsDownloading);
            }
        }

        public ICommand PlayCommand
        {
            get
            {
                this.playCommand = this.playCommand ?? new MvxCommand<ItemWrap>(this.PlayHandler);
                return this.playCommand;
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
        
        public ICommand SimilarArtistSelectedCommand
        {
            get
            {
                this.similarArtistSelectedCommand = this.similarArtistSelectedCommand ?? new MvxCommand<AutocompleteSearch>(this.OpenSimilar);
                return this.similarArtistSelectedCommand;
            }
        }

        public ICommand DownloadAllCommand
        {
            get
            {
                this.downloadAllCommand = this.downloadAllCommand ?? new MvxCommand(this.DownloadAll);
                return this.downloadAllCommand;
            }
        }
        
        public string PicUrlTest
        {
            get { return this.picUrl; }
            set
            {
                this.picUrl = value;
                this.RaisePropertyChanged(() => this.PicUrlTest);
            }
        }

        private void PlayHandler(ItemWrap song)
        {
            this.Play(song.Item);
        }

        private void SelectItemHandler(Album item)
        {
            this.ShowViewModel<ListSongViewModel>(item);
        }

        public async void Init(AutocompleteSearch autocompleteSearch)
        {
            if (string.IsNullOrEmpty(autocompleteSearch.Id) || string.IsNullOrEmpty(autocompleteSearch.ResultType))
            {
                return;
            }

            this.IsBusy = true;
            this.currentItem = new SongListInitItem() { ItemType = SongListEnum.AutoComplete, Item = autocompleteSearch };
            if (!string.IsNullOrEmpty(autocompleteSearch.PicUrl))
            {
                this.PicUrl = autocompleteSearch.PicUrl.Replace("Small", string.Empty);
            }
            if (autocompleteSearch.AutoCompleteType == AutoCompleteType.Xiami)
            {
                List<SongNetease> tempSong = new List<SongNetease>();
                tempSong = await this.netEase.GetXiamiSongs(autocompleteSearch);
                
                this.Songs = new ObservableCollection<ItemWrap>();

                if (tempSong == null)
                {
                    this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                }
                else
                {
                    foreach (var item in tempSong)
                    {
                        this.Songs.Add(new ItemWrap(item, this));
                    }
                }
            }
            else
            {
                //this.Songs = await this.netEase.Autocomplete(toSearch);
                List<SongNetease> tempSong = new List<SongNetease>();
                if (autocompleteSearch.ResultType == "Album")
                {
                    tempSong = await this.netEase.GetSongsForAlbum(autocompleteSearch.Id, autocompleteSearch.ArtistName);
                }
                else if (autocompleteSearch.ResultType == "Song")
                {
                    List<SongNetease> songs = new List<SongNetease>();
                    SongNetease song = await this.netEase.GetSong(autocompleteSearch.Id);
                    if (song != null)
                    {
                        tempSong.Add(song);
                    }
                    else
                    {
                        tempSong = null;
                    }

                }
                else if (autocompleteSearch.ResultType == "Artist")
                {
                    this.IsArtist = true;
                    tempSong = await this.netEase.SearchArtist(autocompleteSearch.Id, autocompleteSearch.Name);
                    this.Albums = await this.netEase.SearchAlbums(autocompleteSearch.Id, autocompleteSearch.Name);
                    this.SimilarArtists = await this.netEase.GetSimilar(autocompleteSearch.Name);
                    if ((this.Albums == null) || (this.SimilarArtists == null))
                    {
                        this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                    }
                }
                this.Songs = new ObservableCollection<ItemWrap>();

                if (tempSong == null)
                {
                    this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                }
                else
                {
                    foreach (var item in tempSong)
                    {
                        this.Songs.Add(new ItemWrap(item, this));
                    }
                } 
            }
            this.RaisePropertyChanged(() => this.Songs);
            this.IsBusy = false;
            this.RaisePropertyChanged(() => this.TitleView);
        }

        public void SavePL(string idUser, string userHash)
        {
            //this.playlistName
            this.userIdToSavePL = idUser;
            this.hashToSavePL = userHash;

            PromptConfig pc = new PromptConfig();
            pc.CancelText = Texts.ButtonCancel;
            pc.IsCancellable = true;
            pc.Message = Texts.TextSavePL;
            pc.OkText = Texts.ButtonSave;
            pc.InputType = InputType.Name;
            pc.Text = this.playlistName;
            pc.OnResult = this.ResultSavePL;
            pc.Placeholder = this.playlistName;
            pc.Title = Texts.TitleSavePL;
            this.userDialog.Prompt(pc);
        }

        private void ResultSavePL(PromptResult pr)
        {
            if (pr.Ok)
            {
                this.netEase.SavePL(this.userIdToSavePL, this.hashToSavePL, pr.Text, this.plid);
            }
        }

        public async void Init(Album album)
        {
            if (album.AlbumId != null)
            {
                this.currentItem = new SongListInitItem() { ItemType = SongListEnum.Album, Item = album};
                this.isAlbum = true;
                this.albumName = album.Name;
                this.PicUrl = album.ImageUrl;
                this.IsBusy = true;
                
                List<SongNetease> tempSong = new List<SongNetease>();
                tempSong = await this.netEase.GetSongsForAlbum(album.AlbumId, album.Artist);
                this.Songs = new ObservableCollection<ItemWrap>();
                if (tempSong == null)
                {
                    this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                }
                else
                {
                    foreach (var item in tempSong)
                    {
                        this.Songs.Add(new ItemWrap(item, this));
                    }
                }
                this.RaisePropertyChanged(() => this.Songs);
                this.IsBusy = false;
                this.RaisePropertyChanged(() => this.TitleView);
            }
        }

        public async void Init(NetEasePlaylist netEasePlaylist)
        {
            if ((netEasePlaylist != null) && !string.IsNullOrEmpty(netEasePlaylist.Id) && netEasePlaylist.IsRealPl)
            {
                this.currentItem = new SongListInitItem() { ItemType = SongListEnum.NeteasePl, Item = netEasePlaylist };
                this.isPlaylist = true;
                this.playlistName = netEasePlaylist.Name;
                this.IsBusy = true;
                this.PicUrl = netEasePlaylist.PicUrl;
                var tempSongs = await this.netEase.GetNetEasePlaylistSongs(netEasePlaylist.Id);
                this.Songs = new ObservableCollection<ItemWrap>();
                if (tempSongs != null)
                {
                    foreach (var item in tempSongs)
                    {
                        this.Songs.Add(new ItemWrap(item, this));
                    }
                }
                else
                {
                    this.dialog.ShowAlert(Texts.ErrorGettingNeteasePL, 5000);
                }
                this.IsBusy = false;
                this.RaisePropertyChanged(() => this.TitleView);
            }
        }

        public async void Init(string plid, string plname)
        {
            if (!string.IsNullOrEmpty(plid))
            {
                this.plid = plid;
                this.currentItem = new SongListInitItem() { ItemType = SongListEnum.CustomPl, Item = plid + ";;;" + plname };
                this.isPlaylist = true;
                this.playlistName = plname;
                this.IsBusy = true;
                this.PicUrl = "http://fildo.net/splash.png";
                var tempSongs = await this.netEase.GetPlaylistSongs(plid);
                this.Songs = new ObservableCollection<ItemWrap>();
                if (tempSongs != null)
                {
                    foreach (var item in tempSongs)
                    {
                        this.Songs.Add(new ItemWrap(item, this));
                    }
                }
                else
                {
                    this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                }
                this.IsBusy = false;
                this.RaisePropertyChanged(() => this.TitleView);
            }
        }

        public void ContextMenu(Song song)
        {
            this.songToUseInAction = song;
            ActionSheetConfig asc = new ActionSheetConfig();
            asc.Options.Add(new ActionSheetOption(Texts.CleanAndPlay, this.CleanAndPlay));
            asc.Options.Add(new ActionSheetOption(Texts.Download, this.DownloadSong));
            asc.Options.Add(new ActionSheetOption(Texts.EnqueueLast, this.EnqueueSong));
            asc.Cancel = new ActionSheetOption(Texts.ButtonCancel);
            asc.Title = Texts.SelectAction;
            this.userDialog.ActionSheet(asc);
        }

        public async void Play(Song song)
        {
            this.Progress = 0;
            if (!song.IsPlaying)
            {
                foreach (var item in this.songs)
                {
                    item.Item.ImagePlay = "res:play3";
                    item.Item.IsPlaying = false;
                    item.Item.Progress = 0;
                }

                song.ImagePlay = "res:pause3";
                song.IsPlaying = true;

                if (string.IsNullOrEmpty(song.Url))
                {
                    this.IsBusy = true;
                    var tempsong = await this.netEase.GetSong(song.Id);
                    if (tempsong != null)
                    {
                        tempsong.Id = song.Id;
                        tempsong.Title = song.Title;
                        tempsong.Artist = song.Artist;
                        this.player.Play(tempsong);
                    }
                    else
                    {
                        this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                    }
                    this.IsBusy = false;
                }
                else
                {
                    this.player.Play(song);
                }

            }
            else
            {
                this.player.Stop(song);
                foreach (var item in this.songs)
                {
                    item.Item.ImagePlay = "res:play3";
                    item.Item.IsPlaying = false;
                    item.Item.Progress = 0;
                }
            }
        }

        public async void PlayAll()
        {
            this.IsBusy = true;
            //List<Song> tempSongs = new List<Song>();
            //if (this.songs.Count > 0)
            //{
            //    this.Play(this.songs.First().Item);
            //}
            ObservableCollection<ItemWrap> songs2 = new ObservableCollection<ItemWrap>(this.songs);
            //songs2.RemoveAt(0);
            //await Task.Delay(1000);

            this.player.PlayAll(songs2.Select(item => item.Item).ToList(), true);

            this.IsBusy = false;

            //await Task.Run(() =>
            //{
            //foreach (var item in songs2)
            //{
            //    try
            //    {
            //        var song = item.Item;
            //        if (string.IsNullOrEmpty(song.Url))
            //        {
            //            var tempsong = await this.netEase.GetSong(song.Id);
            //            if (tempsong != null)
            //            {
            //                tempsong.Id = song.Id;
            //                tempsong.Title = song.Title;
            //                tempsong.Artist = song.Artist;
            //                tempSongs.Add(tempsong);
            //            }
            //            else
            //            {
            //                this.dialog.ShowAlert(
            //                    string.Format(CultureInfo.InvariantCulture, Texts.ErrorWithSong, song.Title),
            //                    5000);
            //            }
            //        }
            //        else
            //        {
            //            tempSongs.Add(song);
            //        }

            //        this.player.PlayAll(tempSongs, false);
            //        tempSongs.Clear();
            //    }
            //    catch 
            //    {
            //        // ignored
            //    }
            //}
            //});
        }

        private Song songToUseInAction;
      
        private void EnqueueSong()
        {
            this.player.PlayerQueue.Add(this.songToUseInAction);
        }


        private async void DownloadSong()
        {
            this.downloadAll = false;
            if (string.IsNullOrEmpty(this.songToUseInAction.Url))
            {
                this.IsBusy = true;
                var tempsong = await this.netEase.GetSong(this.songToUseInAction.Id);
                if (tempsong != null)
                {
                    tempsong.Title = this.songToUseInAction.Title;
                    tempsong.Artist = this.songToUseInAction.Artist;
                    this.Progress = 0;
                    this.downloadQueue.Add(tempsong);
                }
                else
                {
                    this.dialog.ShowAlert(
                        string.Format(CultureInfo.InvariantCulture, Texts.ErrorWithSong, this.songToUseInAction.Title),
                        5000);
                }
                this.IsBusy = false;
            }
            else
            {
                this.downloadQueue.Add(this.songToUseInAction);
            }
        }

        private async void CleanAndPlay()
        {
            this.Progress = 0;
            if (!this.songToUseInAction.IsPlaying)
            {
                foreach (var item in this.songs)
                {
                    item.Item.ImagePlay = "res:play3";
                    item.Item.IsPlaying = false;
                    item.Item.Progress = 0;
                }

                this.songToUseInAction.ImagePlay = "res:pause3";
                this.songToUseInAction.IsPlaying = true;

                if (string.IsNullOrEmpty(this.songToUseInAction.Url))
                {
                    this.IsBusy = true;
                    var tempsong = await this.netEase.GetSong(this.songToUseInAction.Id);
                    if (tempsong != null)
                    {
                        tempsong.Id = this.songToUseInAction.Id;
                        tempsong.Title = this.songToUseInAction.Title;
                        tempsong.Artist = this.songToUseInAction.Artist;
                        this.player.Play(tempsong);
                    }
                    else
                    {
                        this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                    }
                    this.IsBusy = false;
                }
                else
                {
                    this.player.Play(this.songToUseInAction);
                }

            }
            else
            {
                this.player.Stop(this.songToUseInAction);
                foreach (var item in this.songs)
                {
                    item.Item.ImagePlay = "res:play3";
                    item.Item.IsPlaying = false;
                    item.Item.Progress = 0;
                }
            }
        }

        public async void DownloadAll()
        {
            var userDialogs = Mvx.Resolve<IUserDialogs>();
            ConfirmConfig cc = new ConfirmConfig();
            cc.Message = string.Format(Texts.DownloadAllConfirm, this.songs.Count);
            cc.CancelText = Texts.ButtonCancel;
            cc.OkText = "Ok";
            cc.Title = "Download All";
            cc.OnConfirm = this.OnConfirm;
            userDialogs.Confirm(cc);
        }

        private async void OnConfirm(bool b)
        {
            if (!b)
            {
                return;
            }

            this.IsBusy = true;
            for (int i = 0; i < this.songs.Count; i++)
            {
                try
                {
                    var song = this.Songs[i];
                    if (string.IsNullOrEmpty(song.Item.Url))
                    {
                        var tempsong = await this.netEase.GetSong(song.Item.Id);
                        tempsong.Artist = song.Item.Artist;
                        tempsong.Title = song.Item.Title;
                        if (this.isAlbum)
                        {
                            this.downloadQueue.Add(tempsong, true, this.albumName, i + 1);
                        }
                        else if (this.isArtist)
                        {
                            this.downloadQueue.Add((SongNetease)song.Item, false, string.Empty, null);
                        }
                        else
                        {
                            this.downloadQueue.Add(tempsong, false, this.playlistName, i + 1);
                        }
                    }
                    else
                    {
                        if (this.isAlbum)
                        {
                            this.downloadQueue.Add((SongNetease)song.Item, true, this.albumName, i + 1);
                        }
                        else if (this.isArtist)
                        {
                            this.downloadQueue.Add((SongNetease)song.Item, false, string.Empty, null);
                        }
                        else
                        {
                            this.downloadQueue.Add((SongNetease)song.Item, false, this.playlistName, i + 1);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            this.IsBusy = false;
        }

        public void UnregisterEvents()
        {
            //this.downloader.ProgressChanged -= Downloader_ProgressChanged;
            //this.downloader.Downloaded -= Downloader_Downloaded;
            this.player.ProgressChanged -= this.Player_ProgressChanged;
            this.player.SongFinished -= this.Player_SongFinished;
        }

        private void Player_ProgressChanged(object sender, int e)
        {
            var data = this.Songs.Where(p => p.Item.IsPlaying).FirstOrDefault();
            if (data != null)
            {
                data.Item.Progress = e;
            }
        }

        private void Player_SongFinished(object sender, int e)
        {
            var song = this.Songs.Where(p => p.Item.IsPlaying).FirstOrDefault();
            if (song != null)
            {
                int index = this.Songs.IndexOf(song);
                if (this.Songs.Count >= index + 1)
                {
                    this.Play(this.Songs[index + 1].Item);
                }
                else
                {
                    this.Play(this.Songs.First(p => p.Item.Title != null).Item);
                }
            }
        }

        private void OpenSimilar(AutocompleteSearch similar)
        {
            //this.historyItems.Add(this.currentItem);
            similar.ResultType = "Artist";
            this.ShowViewModel<ListSongViewModel>(similar);
            //this.Init(similar);
        }

    }
}
