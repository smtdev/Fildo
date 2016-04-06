using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.Entities
{
    public class SongPlaylist
{
        public string aid { get; set; }
        public string owner_id { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string duration { get; set; }
        public string url { get; set; }
        public object lyrics_id { get; set; }
        public string album { get; set; }
        public string mp3 { get; set; }
        public string name { get; set; }
        public string oga { get; set; }
        public string poster { get; set; }
    }
}
