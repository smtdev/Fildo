namespace Fildo.Core.DTOs.Xiami
{
    using System.Collections.Generic;

    public class XiamiTrack
    {
        public string title { get; set; }
        public string song_id { get; set; }
        public string album_id { get; set; }
        public string album_name { get; set; }
        public int object_id { get; set; }
        public string object_name { get; set; }
        public int insert_type { get; set; }
        public string background { get; set; }
        public int grade { get; set; }
        public string artist { get; set; }
        public string aritst_type { get; set; }
        public string artist_url { get; set; }
        public string location { get; set; }
        public object ms { get; set; }
        public string lyric { get; set; }
        public string lyric_url { get; set; }
        public string pic { get; set; }
        public string album_pic { get; set; }
        public int length { get; set; }
        public int tryhq { get; set; }
        public string artist_id { get; set; }
        public string rec_note { get; set; }
        public string music_type { get; set; }
    }
}