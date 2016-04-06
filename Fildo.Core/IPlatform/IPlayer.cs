using Fildo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.IPlatform
{
    public interface IPlayer
    {
        ObservableCollection<Song> PlayerQueue { get; }
        void PlayPause();
        void Play(Song song);

        void Next();
        void PlayWithoutClear(Song song);

        void PlayAll(List<Song> songs, bool clear);

        void Stop(Song song);

        event EventHandler<int> SongFinished;

        event EventHandler<int> ProgressChanged;

        event EventHandler QueueChanged;
    }
}
