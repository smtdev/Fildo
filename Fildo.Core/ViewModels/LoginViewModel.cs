namespace Fildo.Core.ViewModels
{
    using Entities;
    using IPlatform;
    using Others;
    using Resources;
    using MvvmCross.Core.ViewModels;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Input;
    using System.Linq;

    public class LoginViewModel : BaseViewModel
    {
        private ICommand loginCommand;
        private ICommand registerCommand;
        private string username;
        private string password;
        private bool logged;
        private string hash;
        private string idUser;

        public LoginViewModel(INetEase netEase, INetwork network, IDialog dialog)
            : base(netEase, network, dialog)
        {
        }
                
        public string Username
        {
            get { return this.username; }
            set
            {
                if (value.LastIndexOf("\n") <= -1)
                {
                    this.username = value;
                }
                this.RaisePropertyChanged(() => this.Username);
            }
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                if (value.LastIndexOf("\n") <= -1)
                {
                    this.password = value;
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.username))
                    {
                        this.DoLogin();
                    }
                }

                this.RaisePropertyChanged(() => this.Password);
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

        public ICommand LoginCommand
        {
            get
            {
                this.loginCommand = this.loginCommand ?? new MvxCommand(this.DoLogin);
                return this.loginCommand;
            }
        }

        public ICommand RegisterCommand
        {
            get
            {
                this.registerCommand = this.registerCommand ?? new MvxCommand(this.Register);
                return this.registerCommand;
            }
        }

        public bool Logged
        {
            get { return this.logged; }
            set
            {
                this.logged = value;
                this.RaisePropertyChanged(() => this.Logged);
            }
        }

        private void Register()
        {
            this.ShowViewModel<RegisterViewModel>();
        }

        public async void DoLogin()
        {
            this.IsBusy = true;
            var playlists = await this.netEase.GetPlaylists(this.Username, this.Password);
            if (playlists != null)
            {
                this.Hash = this.netEase.GetHash();
                this.IdUser = this.netEase.GetIdUser();
                this.Logged = true;
                Container.Playlists = playlists;
                Container.ArePublic = true;
                this.ShowViewModel<ListViewModel>(new { playlist = playlists });
                this.dialog.ShowAlert(
                    string.Format(CultureInfo.InvariantCulture, Texts.LoginOk, this.Username),
                    5000);
            }
            else
            {
                this.dialog.ShowAlert(Texts.LoginError, 5000);
            }
            
            this.IsBusy = false;
        }
    }
}
