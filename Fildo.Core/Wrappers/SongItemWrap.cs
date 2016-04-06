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

    public class ItemWrap
    {
        Song song;
        IMvxViewModel parent;

        public ItemWrap(Song item, IMvxViewModel parent)
        {
            this.song = item;
            this.parent = parent;
        }


        public IMvxCommand PlaySongCommand
        {
            get
            {
                if (this.parent.GetType() == typeof(ListSongViewModel))
                {
                    return new MvxCommand(() => ((ListSongViewModel)this.parent).Play(this.song));
                }
            
                if (this.parent.GetType() == typeof(SearchResultViewModel))
                {
                    return new MvxCommand(() => ((SearchResultViewModel)this.parent).Play(this.song));
                }

                return null;
            }
        }

        public IMvxCommand DownloadSongCommand
        {
            get
            {
                if (this.parent.GetType() == typeof(ListSongViewModel))
                {
                    return new MvxCommand(() => ((ListSongViewModel)this.parent).ContextMenu(this.song));
                }
                if (this.parent.GetType() == typeof(SearchResultViewModel))
                {
                    return new MvxCommand(() => ((SearchResultViewModel)this.parent).Download(this.song));
                }

                return null;
            }
        }

        public Song Item
        {
            get
            {
                return this.song;
            }
        }
    }
}
