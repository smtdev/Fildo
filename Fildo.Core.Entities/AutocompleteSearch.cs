using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fildo.Core.Entities
{
    public class AutocompleteSearch : INotifyPropertyChanged
    {
        public AutocompleteSearch()
        {
            this.AutoCompleteType = AutoCompleteType.NetEase;
        }

        private string resultType;
        private string picUrl;
        private string name;
        private string artistName;

        public AutoCompleteType AutoCompleteType { get; set; }

        public string Duration { get; set; }
        public string ArtistName
        {
            get { return this.artistName; }
            set
            {
                if (value == this.artistName) return;
                this.artistName = value;
                this.OnPropertyChanged();
            }
        }

        public string ResultType
        {
            get { return this.resultType; }
            set
            {
                if (value == this.resultType) return;
                this.resultType = value;
                this.OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (value == this.name) return;
                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public string Id { get; set; }
        public string PicUrl
        {
            get { return this.picUrl; }
            set
            {
                if (Equals(value, this.picUrl)) return;
                this.picUrl = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum AutoCompleteType
    {
        NetEase,
        Xiami
    }
}
