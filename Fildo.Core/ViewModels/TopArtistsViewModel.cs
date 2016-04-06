namespace Fildo.Core.ViewModels
{
    using Entities;
    using IPlatform;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System;
    using MvvmCross.Core.ViewModels;

    public class TopArtistsViewModel : BaseViewModel
    {
        private List<AutocompleteSearch> topArtists;

        public TopArtistsViewModel(INetEase netEase, INetwork network, IDialog dialog)
            : base(netEase, network, dialog)
        {
            if (!this.NoInternet)
            {
                this.Init();
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
        
        private ICommand itemSelectedCommand;
        public ICommand ItemSelectedCommand
        {
            get
            {
                this.itemSelectedCommand = this.itemSelectedCommand ?? new MvxCommand<AutocompleteSearch>(this.SelectItemHandler);
                return this.itemSelectedCommand;
            }
        }

        private void SelectItemHandler(AutocompleteSearch item)
        {
            this.ShowViewModel<ListSongViewModel>(item);
        }
        
        private async void Init()
        {
            this.IsBusy = true;    
            this.TopArtists = await this.netEase.GetTopArtists();
            this.IsBusy = false;
        }
    }
}
