namespace Fildo.Core.ViewModels
{
    using Entities;
    using System.Windows.Input;
    using System;
    using IPlatform;
    using Workers;
    using System.Collections.ObjectModel;
    using MvvmCross.Core.ViewModels;
    public class DownloadViewModel : BaseViewModel
    {
        private ICommand itemSelectedCommand;
        private readonly ObservableCollection<Download> downloads;
        private readonly IDownloadQueue downloadQueue;
        private MvxCommand clearAllCommand;

        public DownloadViewModel(INetEase netEase, INetwork network, IDownloadQueue downloadQueue, IDialog dialog)
            : base(netEase, network, dialog)
        {
            this.downloadQueue = downloadQueue;
            this.downloads = this.downloadQueue.GetAll();
            
            this.downloadQueue.QueueChanged += this.DownloadQueue_QueueChanged;
        }

        private void DownloadQueue_QueueChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(() => this.Downloads);
        }

        public ObservableCollection<Download> Downloads
        {
            get
            {
                return this.downloads;
            }
        }

        public ICommand ItemSelectedCommand
        {
            get
            {
                this.itemSelectedCommand = this.itemSelectedCommand ?? new MvxCommand<Download>(this.SelectItemHandler);
                return this.itemSelectedCommand;
            }
        }

        public ICommand ClearAllCommand
        {
            get
            {
                this.clearAllCommand = this.clearAllCommand ?? new MvxCommand(this.ClearAllHandler);
                return this.clearAllCommand;
            }
        }

        private void ClearAllHandler()
        {
            this.downloadQueue.ClearAll();
        }

        private void SelectItemHandler(Download item)
        {
            //ShowViewModel<ListSongViewModel>(new { plid = item.Id, plname = item.Name });
        }
    }
}
