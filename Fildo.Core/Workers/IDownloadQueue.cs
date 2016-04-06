using Fildo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.Workers
{
    public interface IDownloadQueue
    {
        void Add(Song song);
        void Add(Song song, bool isAlbum, string name, int? index);
        void Remove(Song song);
        void RemoveFirst();
        void DownloadNext();

        void ClearAll();

        ObservableCollection<Download> GetAll();

        event EventHandler<System.Collections.Specialized.NotifyCollectionChangedEventArgs> QueueChanged;
    }
}
