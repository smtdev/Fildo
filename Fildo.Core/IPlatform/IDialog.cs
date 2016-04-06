using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.IPlatform
{
    public interface IDialog
    {
        void ShowAlert(string message, int duration);
    }
}
