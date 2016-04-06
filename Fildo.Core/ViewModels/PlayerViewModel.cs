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
    using MvvmCross.Core.ViewModels;
    using Resources;
    using Acr.UserDialogs;
    using System.Linq;

    public class PlayerViewModel : BaseViewModel
    {
        private ICommand itemSelectedCommand;
        private readonly ObservableCollection<SongNetease> songs;
        private readonly IPlayer player;
        private string userIdToSavePL;
        private string hashToSavePL;
        private readonly IUserDialogs userDialog;

        public PlayerViewModel(IUserDialogs userDialog, INetEase netEase, INetwork network, IPlayer player, IDialog dialog)
            : base(netEase, network, dialog)
        {
            /*this.downloadQueue = downloadQueue;
            this.downloads = this.downloadQueue.GetAll();
            this.downloadQueue.QueueChanged += DownloadQueue_QueueChanged;*/
            this.userDialog = userDialog;
            this.player = player;
            this.player.QueueChanged += this.Player_QueueChanged;
            this.RaisePropertyChanged(() => this.Songs);

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
            pc.OnResult = this.ResultSavePL;
            pc.Placeholder = string.Empty;
            pc.Title = Texts.TitleSavePL;
            this.userDialog.Prompt(pc);
        }

        private void ResultSavePL(PromptResult pr)
        {
            if (pr.Ok)
            {
                this.netEase.SavePL(this.userIdToSavePL, this.hashToSavePL, pr.Text, this.Songs.ToList());
            }
        }

        private void Player_QueueChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(() => this.Songs);
        }

        public ObservableCollection<Song> Songs
        {
            get
            {
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
