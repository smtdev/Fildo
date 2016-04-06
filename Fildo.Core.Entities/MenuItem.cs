namespace Fildo.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;


    public class MenuItem : INotifyPropertyChanged
    {
        private string title;
        private string image;
        private Type viewModel;
        
        public string Title
        {
            get { return this.title; }
            set
            {
                if (value == this.title) return;
                this.title = value;
                this.OnPropertyChanged();
            }
        }

        public string Image
        {
            get { return this.image; }
            set
            {
                if (value == this.image) return;
                this.image = value;
                this.OnPropertyChanged();
            }
        }

        public Type ViewModel
        {
            get { return this.viewModel; }
            set
            {
                if (value == this.viewModel) return;
                this.viewModel = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
