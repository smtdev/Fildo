namespace Fildo.Core.Others
{
    using MvvmCross.Core.ViewModels;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class WrappingList<T> : IList<WrappingList<T>.Wrapped>
    {
        public class Wrapped
        {
            public IMvxCommand PlayCommand { get; set; }
            public IMvxCommand DownloadCommand { get; set; }
            
            public T TheItem { get; set; }
        }

        private readonly List<T> _realList;
        private readonly Action<T> realAction1;
        private readonly Action<T> realAction2;
        
        public WrappingList(List<T> realList, Action<T> realAction, Action<T> realAction2)
        {
            this._realList = realList;
            this.realAction1 = realAction;
            this.realAction2 = realAction2;
        }

        private Wrapped Wrap(T item)
        {
            return new Wrapped()
            {
                PlayCommand = new MvxCommand(() => this.playCommandHandler(item)),
                DownloadCommand = new MvxCommand(() => this.downloadCommandHandler(item)),
                TheItem = item
            };
        }

        private void playCommandHandler(T item)
        {

        }

        private void downloadCommandHandler(T item)
        {
            
        }

        #region Implementation of Key required methods

        public int Count { get { return this._realList.Count; } }

        public Wrapped this[int index]
        {
            get { return this.Wrap(this._realList[index]); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region NonImplementation of other methods

        public IEnumerator<Wrapped> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(Wrapped item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Wrapped item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Wrapped[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Wrapped item)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly { get; private set; }

        #endregion

        #region Implementation of IList<DateFilter>

        public int IndexOf(Wrapped item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Wrapped item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
