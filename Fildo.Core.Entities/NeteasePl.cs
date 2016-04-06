using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.Entities
{
    public class NeteasePl
{
        public string name { get; set; }
        public object id { get; set; }
        public List<SongPlaylist> audios { get; set; }
    }
}
