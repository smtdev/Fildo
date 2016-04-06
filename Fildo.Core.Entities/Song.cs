namespace Fildo.Core.Entities
{
    using MvvmCross.Core.ViewModels;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Song : MvxNotifyPropertyChanged
    {
        private string title;
        private string artist;
        private string id;
        private string url;
        private bool isPlaying;
        private string imagePlay = "res:play3";
        private int progress;
        private int duration;

        public string ImagePlay
        {
            get { return this.imagePlay; }
            set
            {
                if (value.Equals(this.imagePlay)) return;
                this.imagePlay = value;
                this.RaisePropertyChanged(() => this.ImagePlay);
            }
        }

        public bool IsPlaying
        {
            get { return this.isPlaying; }
            set
            {
                if (value.Equals(this.isPlaying)) return;
                this.isPlaying = value;
                this.RaisePropertyChanged(() => this.IsPlaying);
            }
        }

        public int Duration
        {
            get { return this.duration; }
            set
            {
                if (value.Equals(this.duration)) return;
                this.duration = value;
                this.RaisePropertyChanged(() => this.Duration);
            }
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                if (value == this.title) return;
                this.title = value;
                this.RaisePropertyChanged(() => this.Title);
            }
        }

        public string Artist
        {
            get { return this.artist; }
            set
            {
                if (value == this.artist) return;
                this.artist = value;
                this.RaisePropertyChanged(() => this.Artist);
            }
        }

        public string Id
        {
            get { return this.id; }
            set
            {
                if (value == this.id) return;
                this.id = value;
                this.RaisePropertyChanged(() => this.Id);
            }
        }

        public string Url
        {
            get { return this.url; }
            set
            {
                if (value == this.url) return;
                this.url = value;
                this.RaisePropertyChanged(() => this.Url);
            }
        }

        public int Progress
        {
            get { return this.progress; }
            set
            {
                if (value == this.progress) return;
                this.progress = value;
                this.RaisePropertyChanged(() => this.Progress);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
