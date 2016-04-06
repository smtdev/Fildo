namespace Fildo.Core.ViewModels
{
    using Entities;
    using IPlatform;
    using Others;
    using MvvmCross.Core.ViewModels;
    using Resources;
    using System.Collections.Generic;
    using System.Windows.Input;

    public class ConfigurationViewModel : BaseViewModel
    {
        private ICommand loginCommand;
        private bool createFolderArtistAlbum;
        private bool saved;
        private List<string> cultures;
        private List<string> proxies;

        public ConfigurationViewModel(INetEase netEase, INetwork network, IDialog dialog)
            : base(netEase, network, dialog)
        {
            this.Cultures = new List<string>();
            this.Cultures.Add(string.Empty);
            this.Cultures.Add("Español");
            this.Cultures.Add("English");
            this.Cultures.Add("Català");
            this.Cultures.Add("Français");
            this.Cultures.Add("Deutsch");
            this.Cultures.Add("Italiano");
            this.Cultures.Add("Português");

            this.Proxies = new List<string>();
            this.Proxies.Add(Texts.NoProxy);

            this.Proxies.Add("http://117.177.250.149:82");
            this.Proxies.Add("http://117.177.250.149:83");
            this.Proxies.Add("http://117.177.250.149:84");
            this.Proxies.Add("http://117.177.250.149:86");

            this.Proxies.Add("http://117.177.250.148:85");
            this.Proxies.Add("http://117.177.250.148:86");

            this.Proxies.Add("http://117.177.250.147:85");
            this.Proxies.Add("http://117.177.250.147:84");
            this.Proxies.Add("http://117.177.250.147:83");
            this.Proxies.Add("http://117.177.250.147:82");

            this.Proxies.Add("http://117.177.250.146:86");
            this.Proxies.Add("http://117.177.250.146:85");
            this.Proxies.Add("http://117.177.250.147:84");
            this.Proxies.Add("http://117.177.250.147:83");
            this.Proxies.Add("http://117.177.250.147:82");
        }
        
        public List<string> Cultures
        {
            get { return this.cultures; }
            set
            {
                this.cultures = value;
                this.RaisePropertyChanged(() => this.Cultures);
            }
        }

        public List<string> Proxies
        {
            get { return this.proxies; }
            set
            {
                this.proxies = value;
                this.RaisePropertyChanged(() => this.Proxies);
            }
        }

        public bool CreateFolderArtistAlbum
        {
            get { return this.createFolderArtistAlbum; }
            set
            {
                this.createFolderArtistAlbum = value;
                this.RaisePropertyChanged(() => this.CreateFolderArtistAlbum);
            }
        }

        public ICommand SaveCommand{
            get
            {
                this.loginCommand = this.loginCommand ?? new MvxCommand(this.Save);
                return this.loginCommand;
            }
        }

        public bool Saved
        {
            get { return this.saved; }
            set
            {
                this.saved = value;
                this.RaisePropertyChanged(() => this.Saved);
            }
        }
        private void Save()
        {
            this.Saved = true;
            this.Close(this);
        }
    }
}
