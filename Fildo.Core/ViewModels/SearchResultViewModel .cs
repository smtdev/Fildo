namespace Fildo.Core.ViewModels
{
    using Entities;
    using IPlatform;
    using Resources;
    using MvvmCross.Core.ViewModels;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using System;
    using Wrappers;
    using System.Diagnostics;
    using Workers;
    public class SearchResultViewModel : BaseViewModel
    {
        private ObservableCollection<AutocompleteSearch> results;
        private ObservableCollection<AutocompleteSearch> resultsXiami;
        private ICommand itemSelectedCommand;
        private ObservableCollection<ItemWrap> resultsVK;
        private MvxCommand<ItemWrap> vkResultSelectCommand;
        private readonly IPlayer player;
        private ObservableCollection<ItemWrap> resultsQQ;

        public SearchResultViewModel(
            INetEase netEase, 
            INetwork network, 
            IDialog dialog, 
            IPlayer player, 
            IDownloadQueue downloaderQueue)
            : base(netEase, network, dialog)
        {
            this.player = player;
            this.downloaderQueue = downloaderQueue;
        }
        
        public ObservableCollection<AutocompleteSearch> Results
        {
            get { return this.results; }
            set
            {
                this.results = value;
                this.RaisePropertyChanged(() => this.Results);
            }
        }

        public ObservableCollection<AutocompleteSearch> ResultsXiami
        {
            get { return this.resultsXiami; }
            set
            {
                this.resultsXiami = value;
                this.RaisePropertyChanged(() => this.ResultsXiami);
            }
        }

        public ObservableCollection<ItemWrap> ResultsVK
        {
            get { return this.resultsVK; }
            set
            {
                this.resultsVK = value;
                this.RaisePropertyChanged(() => this.ResultsVK);
            }
        }

        public ObservableCollection<ItemWrap> ResultsQQ
        {
            get { return this.resultsQQ; }
            set
            {
                this.resultsQQ = value;
                this.RaisePropertyChanged(() => this.ResultsQQ);
            }
        }

        internal void Play(Song songVK)
        {
            this.player.Play(songVK);
        }

        public ICommand ItemSelectedCommand
        {
            get
            {
                this.itemSelectedCommand = this.itemSelectedCommand ?? new MvxCommand<AutocompleteSearch>(this.SelectItemHandler);
                return this.itemSelectedCommand;
            }
        }

        internal void Download(Song songVK)
        {
            this.downloaderQueue.Add(songVK);
        }

        public ICommand VkResultSelectCommand
        {
            get
            {
                this.vkResultSelectCommand = this.vkResultSelectCommand ?? new MvxCommand<ItemWrap>(this.VkResultSelectHandler);
                return this.vkResultSelectCommand;
            }
        }

        private void SelectItemHandler(AutocompleteSearch item)
        {
            this.ShowViewModel<ListSongViewModel>(item);
        }

        private void VkResultSelectHandler(ItemWrap item)
        {
            this.player.Play(item.Item);
        }

        public override void Search()
        {
            this.ShowViewModel<SearchResultViewModel>(new { toSearch = this.ToSearch });
        }


        private bool showCaptcha;

        /// <summary>
        /// Establece u obtiene el valor para ShowCaptcha
        /// </summary>
        /// <value>
        /// El valor de ShowCaptcha.
        /// </value>
        public bool ShowCaptcha
        {
            get
            {
                return this.showCaptcha;
            }

            set
            {
                this.showCaptcha = value;
                this.RaisePropertyChanged(() => this.ShowCaptcha);
            }
        }


        private string captchaUrl;
        private readonly IDownloadQueue downloaderQueue;

        /// <summary>
        /// Establece u obtiene el valor para CaptchaUrl
        /// </summary>
        public string CaptchaUrl
        {
            get
            {
                return this.captchaUrl;
            }

            set
            {
                this.captchaUrl = value;
                this.RaisePropertyChanged(() => this.CaptchaUrl);
            }
        }

        public VkResult vkResultTemp { get; private set; }

        public async void ResolveCaptcha(string captchaText)
        {
            this.vkResultTemp = await this.netEase.SearchVK(this.vkResultTemp.PreviousSearch, this.vkResultTemp.CaptchaSid, captchaText);
            this.SetVKResults();
        }
        
        public async void Init(string toSearch)
        {
            try
            {
                this.ToSearch = toSearch;
                this.IsBusy = true;
                var data = await this.netEase.Autocomplete(toSearch);
                this.Results = new ObservableCollection<AutocompleteSearch>();
                if (data == null)
                {
                    this.dialog.ShowAlert(Texts.ErrorSearching, 5000);
                }
                else if (data.Count == 0)
                {
                    this.dialog.ShowAlert(Texts.SearchNotFound, 5000);
                }
                else
                {
                    this.Results = new ObservableCollection<AutocompleteSearch>(data);
                }
                /**************** XIAMI **********************/
                data = await this.netEase.SearchXiami(toSearch);
                this.ResultsXiami = new ObservableCollection<AutocompleteSearch>();
                if (data == null)
                {
                    this.dialog.ShowAlert(Texts.ErrorSearching, 5000);
                }
                else if (data.Count == 0)
                {
                    this.dialog.ShowAlert(Texts.SearchNotFound, 5000);
                }
                else
                {
                    this.ResultsXiami = new ObservableCollection<AutocompleteSearch>(data);
                }

                /******************** VK **********************/
                this.vkResultTemp = await this.netEase.SearchVK(toSearch);
                this.SetVKResults();

                var tempSongQQ = new ObservableCollection<SongQQ>(await this.netEase.SearchQQ(toSearch));
                if (tempSongQQ == null)
                {
                    this.dialog.ShowAlert("QQMusic: " + Texts.ErrorSearching, 5000);
                }
                else if (tempSongQQ.Count == 0)
                {
                    this.dialog.ShowAlert("QQMusic: " + Texts.SearchNotFound, 5000);
                }
                else if (tempSongQQ[0].Artist == "CaptchaError")
                {
                    this.dialog.ShowAlert("QQMusic: Wow... too much searches, try again in few minutes.", 5000);
                }
                else
                {
                    this.ResultsQQ = new ObservableCollection<ItemWrap>();
                    if (tempSongQQ == null)
                    {
                        this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                    }
                    else
                    {
                        foreach (var item in tempSongQQ)
                        {
                            this.ResultsQQ.Add(new ItemWrap(item, this));
                        }
                    }
                }

                this.IsBusy = false;
                this.RaisePropertyChanged(() => this.Results);
                this.RaisePropertyChanged(() => this.ResultsXiami);
                this.RaisePropertyChanged(() => this.ResultsVK);
            }
            catch (Exception)
            {
                this.dialog.ShowAlert("Something was wrong with search. Try again.", 5000);
            }
        }

        private void SetVKResults()
        {
            if (this.vkResultTemp == null)
            {
                this.dialog.ShowAlert("VK: " + Texts.ErrorSearching, 5000);
            }
            else if (this.vkResultTemp.Error)
            {
                if (!string.IsNullOrEmpty(this.vkResultTemp.CaptchaUrl))
                {
                    this.CaptchaUrl = this.vkResultTemp.CaptchaUrl;
                    this.ShowCaptcha = true;
                }
                else
                {
                    this.dialog.ShowAlert("VK: " + Texts.ErrorSearching, 5000);
                }
            }
            else if (this.vkResultTemp.Songs.Count == 0)
            {
                this.dialog.ShowAlert("VK: " + Texts.SearchNotFound, 5000);
            }
            else
            {
                this.ResultsVK = new ObservableCollection<ItemWrap>();
                if (this.vkResultTemp == null)
                {
                    this.dialog.ShowAlert(Texts.ErrorGettingResults, 5000);
                }
                else
                {
                    foreach (var item in this.vkResultTemp.Songs)
                    {
                        this.ResultsVK.Add(new ItemWrap(item, this));
                    }
                }
            }
        }
    }
}
