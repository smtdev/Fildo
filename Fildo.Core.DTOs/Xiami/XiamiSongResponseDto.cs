namespace Fildo.Core.DTOs.Xiami
{
    using System.Collections.Generic;

    public class XiamiSongResponseDto
    {
        public bool status { get; set; }
        public string message { get; set; }
        public XiamiSongDataDto data { get; set; }
        public object jumpurl { get; set; }
    }
}