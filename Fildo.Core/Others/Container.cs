using Fildo.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.Others
{
    public static class Container
    {
        public static List<Playlist> Playlists { get; set; }
        public static bool ArePublic { get; set; }

    }
}
