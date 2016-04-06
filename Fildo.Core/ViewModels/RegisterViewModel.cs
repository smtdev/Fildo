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


    public class RegisterViewModel : BaseViewModel
    {
        private ICommand registerCommand;
        private string username;
        private string password;
        private string mail;

        public RegisterViewModel(INetEase netEase, INetwork network, IDialog dialog)
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

                this.RaisePropertyChanged(() => this.Password);
            }
        }

        public string Email
        {
            get { return this.mail; }
            set
            {
                if (value.LastIndexOf("\n") <= -1)
                {
                    this.mail = value;
                }

                this.RaisePropertyChanged(() => this.Email);
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
 
        public async void Register()
        {
            if (string.IsNullOrEmpty(this.username) || string.IsNullOrEmpty(this.password) || string.IsNullOrEmpty(this.Email))
            {
                this.dialog.ShowAlert("You have to fill all fields.", 5000);
            }
            else {
                this.IsBusy = true;
                var result = await this.netEase.Register(this.Username, this.Password, this.Email);
                if (result != null)
                {
                    if (result == "1")
                    {
                        this.dialog.ShowAlert(Texts.RegisterOK, 5000);
                        this.ShowViewModel<LoginViewModel>();
                    }
                    else if (result == "0")
                    {
                        this.dialog.ShowAlert(Texts.RegisterError, 5000);
                        this.Password = string.Empty;
                    }
                    else if (result == "-1")
                    {
                        this.dialog.ShowAlert(Texts.RegisterDupe, 5000);
                        this.Password = string.Empty;
                    }
                }
                else
                {
                    this.dialog.ShowAlert(Texts.LoginError, 5000);
                }

                this.IsBusy = false;
            }
        }
    }
}
