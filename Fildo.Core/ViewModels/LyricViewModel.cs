namespace Fildo.Core.ViewModels
{
    using Entities;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System;
    using IPlatform;
    using Others;
    using Workers;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Resources;
    using MvvmCross.Core.ViewModels;

    public class LyricViewModel : BaseViewModel
    {
        private ICommand itemSelectedCommand;
        private readonly ObservableCollection<SongNetease> songs;
        private readonly IPlayer player;

        public LyricViewModel(INetEase netEase, INetwork network, IPlayer player, IDialog dialog)
            : base(netEase, network, dialog)
        {
            /*this.downloadQueue = downloadQueue;
            this.downloads = this.downloadQueue.GetAll();
            this.downloadQueue.QueueChanged += DownloadQueue_QueueChanged;*/
            this.player = player;
            this.player.QueueChanged += this.Player_QueueChanged;
            this.RaisePropertyChanged(() => this.Songs);

        }

        private void Player_QueueChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(() => this.Songs);
        }

        public ObservableCollection<Song> Songs
        {
            get {
                return this.player.PlayerQueue;
            }
        }

        public ICommand ItemSelectedCommand
        {
            get
            {
                this.itemSelectedCommand = this.itemSelectedCommand ?? new MvxCommand<SongNetease>(this.SelectItemHandler);
                return this.itemSelectedCommand;
            }
        }

        private void SelectItemHandler(SongNetease item)
        {
            //ShowViewModel<ListSongViewModel>(new { plid = item.Id, plname = item.Name });
            this.player.PlayWithoutClear(item);
        }

        public async Task<string> GetLyric(string id)
        {
            var temp = await this.netEase.GetLyric(id);
            if (temp == null)
            {
                this.dialog.ShowAlert(Texts.LyricError, 5000);
                return string.Empty;
            }
            else if (temp == string.Empty)
            {
                this.dialog.ShowAlert(Texts.LyricNotFound, 5000);
                return string.Empty;
            }
            else
            {
                return temp;
            }
        }
    }
}
