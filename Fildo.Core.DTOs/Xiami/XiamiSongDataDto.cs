namespace Fildo.Core.DTOs.Xiami
{
    using System.Collections.Generic;

    public class XiamiSongDataDto
    {
        public List<XiamiTrack> trackList { get; set; }
        public int lastSongId { get; set; }
        public string type { get; set; }
        public int type_id { get; set; }
        public object clearlist { get; set; }
        public int vip { get; set; }
        public int vip_role { get; set; }
        public int hqset { get; set; }
    }
}