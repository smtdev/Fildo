using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Entities;
    using Services;

    public interface INetEase
    {
        Task SavePL(string userId, string hash, string plName, List<Song> songs);
        Task SavePL(string userId, string hash, string plName, string plId);

        string GetRandomProxy();
        Task<Song> FixUrl(Song song, bool forceHost = false);

        Task<List<SongQQ>> SearchQQ(string toSearch);
        Task<VkResult> SearchVK(string toSearch);
        Task<bool> UrlExists(string url);
        Task<bool> UrlExists(string url, string host);

        Task<List<SongNetease>> GetNetEasePlaylistSongs(string plid);
        Task<List<AutocompleteSearch>> GetRecommendations();
        List<NetEasePlaylist> GetNetEasePlaylist();

        string GetHash();
        string GetIdUser();

        Task ImportNetease(string idpl, string id, string idhash);

        Task<List<Playlist>> GetPlaylists(string user, string pass);

        Task<List<Playlist>> GetPublicPlaylists();

        Task<List<SongNetease>> GetPlaylistSongs(string plid);

        Task<int> GetVersion(int platform);

        Task<List<Album>> GetTopAlbums();

        Task<List<AutocompleteSearch>> GetTopArtists();

        Task<List<SongNetease>> GetSongsForAlbum(string albumId, string artistName);

        Task<List<Album>> SearchAlbums(string artistId, string artistName);

        Task<List<AutocompleteSearch>> GetSimilar(string artist);

        Task<List<SongNetease>> SearchArtist(string id, string name);

        Task<SongNetease> GetSong(string songId);

        Task<List<AutocompleteSearch>> Autocomplete(string search);

        Task<List<AutocompleteSearch>> SearchXiami(string search);

        Task<string> GetLyric(string id);

        Task<string> Register(string username, string password, string email);
        Task<VkResult> SearchVK(string previousSearch, string captchaSid, string captchaText);

        void DeletePlaylist(string s);

        Task<List<SongNetease>> GetXiamiSongs(AutocompleteSearch autocompleteSearch);
    }
}
