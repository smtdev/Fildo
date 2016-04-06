namespace Fildo.Core.ViewModels
{
    using Entities;
    using System.Collections.Generic;
    using System.Windows.Input;
    using System;
    using IPlatform;
    using Others;
    using Workers;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Resources;
    using MvvmCross.Core.ViewModels;

    public class InfoViewModel : BaseViewModel
    {
        public InfoViewModel(INetEase netEase, INetwork network, IPlayer player, IDialog dialog)
            : base(netEase, network, dialog)
        {
            

        }

        
    }
}
