namespace Fildo.Core.ViewModels
{
    using Entities;
    using IPlatform;
    using Others;
    using MvvmCross.Core.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;

    public class ImportNeteaseViewModel : BaseViewModel
    {
        private ICommand importCommand;
        private string urlPlaylist;
        private string hash;
        private string idUser;

        public ImportNeteaseViewModel(INetEase netEase, INetwork network, IDialog dialog)
            : base(netEase, network, dialog)
        {
        }
                
        public string UrlPlaylist
        {
            get { return this.urlPlaylist; }
            set
            {
                this.urlPlaylist = value;
                this.RaisePropertyChanged(() => this.UrlPlaylist);
            }
        }

        public string Hash
        {
            get { return this.hash; }
            set
            {
                this.hash = value;
                this.RaisePropertyChanged(() => this.Hash);
            }
        }

        public string IdUser
        {
            get { return this.idUser; }
            set
            {
                this.idUser = value;
                this.RaisePropertyChanged(() => this.IdUser);
            }
        }

        public ICommand ImportCommand
        {
            get
            {
                this.importCommand = this.importCommand ?? new MvxCommand(this.Import);
                return this.importCommand;
            }
        }

        private async void Import()
        {
            this.IsBusy = true;
            try
            {
                int n;
                bool isNumeric = int.TryParse(this.UrlPlaylist, out n);
                if (isNumeric)
                {
                    await this.netEase.ImportNetease(this.UrlPlaylist, this.IdUser, this.Hash);
                }
                else {
                    var temp = this.UrlPlaylist.Split(new string[] { "playlist/" }, StringSplitOptions.None)[1];
                    temp = temp.Split('/')[0];
                    if ((!string.IsNullOrEmpty(this.Hash)) && (!string.IsNullOrEmpty(this.UrlPlaylist)))
                    {
                        await this.netEase.ImportNetease(temp, this.IdUser, this.Hash);
                    }
                }
                this.ShowViewModel<LoginViewModel>();
            }
            catch (Exception)
            {
                
            }

            this.IsBusy = false;
        }
    }
}
