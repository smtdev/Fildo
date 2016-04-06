namespace Fildo.Core.Wrappers
{
    using Entities;
    using ViewModels;
    using MvvmCross.Core.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class PlaylistItemWrap
    {
        Playlist pl;
        IMvxViewModel parent;
        private bool arePublic;

        public PlaylistItemWrap(Playlist item, IMvxViewModel parent, bool arePublic)
        {
            this.pl = item;
            this.parent = parent;
            this.arePublic = arePublic;
        }


        public IMvxCommand LoadPlaylistCommand
        {
            get
            {
                 return new MvxCommand(
                     () =>
                     {
                         ((ListViewModel)this.parent).SelectItemHandler(this);
                         
                     });
            }
        }

        public IMvxCommand DeletePlaylistCommand
        {
            get
            {
                return new MvxCommand(() => ((ListViewModel)this.parent).DeletePlaylist(this.pl));
            }
        }

        public Playlist Item
        {
            get
            {
                return this.pl;
            }
        }

        public bool ArePublic
        {
            get
            {
                return this.arePublic;
            }
        }
    }
}
