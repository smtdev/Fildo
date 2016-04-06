namespace Fildo.Core.ViewModels
{
    using Entities;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Acr.UserDialogs;

    using Fildo.Core.Resources;
    using Fildo.Core.Wrappers;

    using IPlatform;
    using Others;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Platform;

    public class ListViewModel : BaseViewModel
    {
        private ICommand itemSelectedCommand;
        private ObservableCollection<PlaylistItemWrap> playlist;
        
        public ListViewModel(INetEase netEase, INetwork network, IDialog dialog)
            : base(netEase, network, dialog)
        {
        }

        public ObservableCollection<PlaylistItemWrap> Playlists
        {
            get { return this.playlist; }
            set
            {
                this.playlist = value;
                this.RaisePropertyChanged(() => this.Playlists);
            }
        }

        public ICommand ItemSelectedCommand
        {
            get
            {
                this.itemSelectedCommand = this.itemSelectedCommand ?? new MvxCommand<PlaylistItemWrap>(this.SelectItemHandler);
                return this.itemSelectedCommand;
            }
        }

        public void SelectItemHandler(PlaylistItemWrap item)
        {
            this.ShowViewModel<ListSongViewModel>(new { plid = item.Item.Id, plname = item.Item.Name });
        }

        public void Init(List<Playlist> playlists)
        {
            this.Playlists = new ObservableCollection<PlaylistItemWrap>();
            foreach (Playlist pl in Container.Playlists)
            {
                this.Playlists.Add(new PlaylistItemWrap(pl, this, Container.ArePublic));
            }
            this.RaisePropertyChanged("Playlists");
        }

        private string plidToDelete;
        public void DeletePlaylist(Playlist pl)
        {
            this.plidToDelete = pl.Id;
            var userDialogs = Mvx.Resolve<IUserDialogs>();
            ConfirmConfig cc = new ConfirmConfig();
            cc.CancelText = "Cancel";
            cc.OkText = "Ok";
            cc.Message = Texts.SureDelete;
            cc.OnConfirm = OnConfirm;
            userDialogs.Confirm(cc);
        }

        private void OnConfirm(bool b)
        {
            if (b)
            {
                var persist = Mvx.Resolve<IPersist>();
                var userHash = persist.GetString("UserHash");
                this.netEase.DeletePlaylist(this.plidToDelete + ";" + userHash);
                var itemToDelete = this.Playlists.FirstOrDefault(p => p.Item.Id == this.plidToDelete);
                this.Playlists.Remove(itemToDelete);
                this.RaisePropertyChanged("Playlists");
            }
        }
    }
}
