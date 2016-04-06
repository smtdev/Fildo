using Fildo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.IPlatform
{
    public interface IPersist
    {
        void PersistStringList(List<string> toPersist, string key);
        List<string> GetStringList(string key);
        string GetString(string key);
    }
}
