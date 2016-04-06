using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.Entities
{
    public class SongListInitItem
    {
        public SongListEnum ItemType { get; set; }
        public object Item { get; set; }
    }
}
