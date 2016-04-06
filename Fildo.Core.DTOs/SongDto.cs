using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.DTOs
{
    public class ArtistDto
    {
        public int img1v1Id { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public List<object> alias { get; set; }
        public string briefDesc { get; set; }
        public string picUrl { get; set; }
        public int picId { get; set; }
        public int albumSize { get; set; }
        public string img1v1Url { get; set; }
        public string trans { get; set; }
        public int musicSize { get; set; }
    }

    public class AlbumDto
    {
        public List<object> songs { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public int size { get; set; }
        public int status { get; set; }
        public string description { get; set; }
        public string tags { get; set; }
        public List<object> alias { get; set; }
        public List<ArtistDto> artists { get; set; }
        public string briefDesc { get; set; }
        public ArtistDto artist { get; set; }
        public string picUrl { get; set; }
        public long picId { get; set; }
        public string commentThreadId { get; set; }
        public long publishTime { get; set; }
        public string company { get; set; }
        public int copyrightId { get; set; }
        public string blurPicUrl { get; set; }
        public int companyId { get; set; }
        public long pic { get; set; }
    }

    public class HMusic
    {
        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public string extension { get; set; }
        public long dfsId { get; set; }
        public int playTime { get; set; }
        public int bitrate { get; set; }
        public int sr { get; set; }
        public double volumeDelta { get; set; }
    }

    public class MMusic
    {
        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public string extension { get; set; }
        public long dfsId { get; set; }
        public int playTime { get; set; }
        public int bitrate { get; set; }
        public int sr { get; set; }
        public double volumeDelta { get; set; }
    }

    public class LMusic
    {
        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public string extension { get; set; }
        public long dfsId { get; set; }
        public int playTime { get; set; }
        public int bitrate { get; set; }
        public int sr { get; set; }
        public double volumeDelta { get; set; }
    }

    public class BMusic
    {
        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public string extension { get; set; }
        public long dfsId { get; set; }
        public int playTime { get; set; }
        public int bitrate { get; set; }
        public int sr { get; set; }
        public double volumeDelta { get; set; }
    }

    public class Audition
    {
        public string name { get; set; }
        public int id { get; set; }
        public int size { get; set; }
        public string extension { get; set; }
        public long dfsId { get; set; }
        public int playTime { get; set; }
        public int bitrate { get; set; }
        public int sr { get; set; }
        public double volumeDelta { get; set; }
    }

    public class SongDto
    {
        public bool starred { get; set; }
        public double popularity { get; set; }
        public int starredNum { get; set; }
        public int playedNum { get; set; }
        public int dayPlays { get; set; }
        public int hearTime { get; set; }
        public string mp3Url { get; set; }
        public List<object> rtUrls { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public int position { get; set; }
        public int duration { get; set; }
        public int status { get; set; }
        public List<object> alias { get; set; }
        public List<ArtistDto> artists { get; set; }
        public int score { get; set; }
        public AlbumDto album { get; set; }
        public string commentThreadId { get; set; }
        public int fee { get; set; }
        public HMusic hMusic { get; set; }
        public MMusic mMusic { get; set; }
        public LMusic lMusic { get; set; }
        public int copyrightId { get; set; }
        public int mvid { get; set; }
        public int ftype { get; set; }
        public int rtype { get; set; }
        public object rurl { get; set; }
        public string copyFrom { get; set; }
        public BMusic bMusic { get; set; }
        public Audition audition { get; set; }
        public object ringtone { get; set; }
        public string disc { get; set; }
        public int no { get; set; }
        public object crbt { get; set; }
        public object rtUrl { get; set; }
    }

    public class Equalizers
    {
    }
}
