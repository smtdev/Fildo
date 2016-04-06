namespace Fildo.Core.Entities
{
    using MvvmCross.Core.ViewModels;

    public class Download : MvxNotifyPropertyChanged
    {
        public Download(Song song) : this(song, null, false,false,string.Empty,string.Empty)
        {
        }

        public Download(Song song, int? index, bool? isAlbum, string albumName, string playlistName)
        {
            if (isAlbum.HasValue && isAlbum.Value)
            {
                this.Song = song;
                this.IsAlbum = isAlbum.Value;
                this.Index = index;
                this.IsPlaylist = false;
                this.AlbumName = albumName;
                this.PlaylistName = playlistName;
            }
            else if (isAlbum.HasValue && !isAlbum.Value)
            {
                this.Song = song;
                this.IsAlbum = false;
                this.Index = index;
                this.IsPlaylist = true;
                this.AlbumName = albumName;
                this.PlaylistName = playlistName;
            }
            else if (!isAlbum.HasValue)
            {
                this.Song = song;
                this.IsAlbum = false;
                this.Index = index;
                this.IsPlaylist = false;
                this.AlbumName = string.Empty;
                this.PlaylistName = string.Empty;
            }
        }
        
        public Download(Song song, int? index, bool isAlbum, bool isPlaylist, string albumName, string playlistName)
        {
            this.Song = song;
            this.IsAlbum = isAlbum;
            this.Index = index;
            this.isPlaylist = isPlaylist;
            this.AlbumName = albumName;
            this.PlaylistName = playlistName;
        }

        private int? index;

        public int? Index
        {
            get { return this.index; }
            set {
                this.index = value;
                this.RaisePropertyChanged(() => this.Index); }
        }


        private Song song;

        public Song Song
        {
            get { return this.song; }
            set {
                this.song = value;
                this.RaisePropertyChanged(() => this.Song); }
        }

        private bool isAlbum;

        public bool IsAlbum
        {
            get { return this.isAlbum; }
            set {
                this.isAlbum = value;
                this.RaisePropertyChanged(() => this.IsAlbum); }
        }

        private bool isPlaylist;

        public bool IsPlaylist
        {
            get { return this.isPlaylist; }
            set {
                this.isPlaylist = value;
                this.RaisePropertyChanged(() => this.IsPlaylist); }
        }

        private string albumName;

        public string AlbumName
        {
            get { return this.albumName; }
            set {
                this.albumName = value;
                this.RaisePropertyChanged(() => this.AlbumName); }
        }

        private string playlistName;

        public string PlaylistName
        {
            get { return this.playlistName; }
            set {
                this.playlistName = value;
                this.RaisePropertyChanged(() => this.PlaylistName); }
        }


        private int percent;

        /// <summary>
        /// Establece u obtiene el valor para Percent
        /// </summary>
        /// <value>
        /// El valor de Percent.
        /// </value>
        public int Percent
        {
            get
            {
                return this.percent;
            }

            set
            {
                this.percent = value;
                this.RaisePropertyChanged(() => this.Percent);
            }
        }



    }
}
