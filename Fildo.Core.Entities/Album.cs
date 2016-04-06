using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fildo.Core.Entities
{
    public class Album : INotifyPropertyChanged
    {
        private string albumId;
        private string imageUrl;
        private string name;
        private string artist;

        public string AlbumId
        {
            get { return this.albumId; }
            set
            {
                if (value == this.albumId) return;
                this.albumId = value;
                this.OnPropertyChanged();
            }
        }

        public string Artist
        {
            get { return this.artist; }
            set
            {
                if (value == this.artist) return;
                this.artist = value;
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

        public string ImageUrl
        {
            get { return this.imageUrl; }
            set
            {
                if (value == this.imageUrl) return;
                this.imageUrl = value;
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
}
