namespace Fildo.Core.DTOs.Xiami
{
    using System.Collections.Generic;

    public class XiamiAutocompleteDto
    {
        public List<XiamiSongDto> songs { get; set; }
        public List<XiamiArtistDto> artists { get; set; }
        public List<XiamiAlbumDto> albums { get; set; }
    }
}