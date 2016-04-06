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
    using IPlatform;
    using MvvmCross.Platform;
    public class NetEase : INetEase
    {
        private const string SearchedArtistKey = "SearchedArtists";
        private List<AutocompleteSearch> autocompleteSearches;
        private bool searching;
        private readonly NetEaseService netEaseService;
        private readonly FildoService fildoService;
        private readonly IPersist persist;

        public NetEase()
        {
            this.searching = false;
            // TODO DI
            this.persist = Mvx.Resolve<IPersist>();
            this.netEaseService = new NetEaseService();
            this.fildoService = new FildoService();
        }

        public async Task SavePL(string userId, string hash, string plName, string plId)
        {
            await this.fildoService.SavePL(userId, hash, plName, plId);
        }

        public async Task SavePL(string userId, string hash, string plName, List<Song> songs)
        {
            await this.fildoService.SavePL(userId, hash, plName, songs);
        }


        public string GetRandomProxy()
        {
            Random random = new Random();
            return this.netEaseService.AvailableProxy[random.Next(0, this.netEaseService.AvailableProxy.Count - 1)] + "/m1";
        }

        public async Task<Song> FixUrl(Song song, bool forceHost = false)
        {
            bool exists = await this.UrlExists(song.Url);
            if (!exists || forceHost)
            {
                bool found = false;

                song.Url = song.Url.Replace("http://p3.music.126.net/", "http://221.228.64.228/");
                song.Url = song.Url.Replace("http://p4.music.126.net/", "http://221.228.64.228/");
                if (!await this.UrlExists(song.Url, "m1.music.126.net"))
                {
                    song.Url = song.Url.Replace("http://221.228.64.228/", "http://p3.music.126.net/");
                    //song.Url = song.Url.Replace("http://p3", this.GetRandomProxy());


                    //var vkSongs = await this.SearchVK(song.Artist + " " + song.Title);
                    //if ((vkSongs != null) && (vkSongs.Songs.Count > 0))
                    //{

                    //    foreach (var item in vkSongs.Songs)
                    //    {
                    //        if ((item.Artist == song.Artist) && (item.Title == song.Title))
                    //        {
                    //            song.Url = item.Url;
                    //            found = true;
                    //            break;
                    //        }
                    //    }
                        /*
                        if (!found)
                        {
                            foreach (var item in vkSongs)
                            {
                                if (item.Artist == song.Artist)
                                {
                                    song.Url = item.Url;
                                    found = true;
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            song.Url = vkSongs.FirstOrDefault().Url;
                            found = true;
                        }*/
                    //}

                    if (!found)
                    {
                        song.Url = song.Url.Replace("http://p3", this.GetRandomProxy());
                        song.Url = song.Url.Replace("http://p4", this.GetRandomProxy());
                    }
                }
            }

            return song;
        }

        public async Task<List<AutocompleteSearch>> GetRecommendations()
        {
            var searchedArtists = this.persist.GetStringList(SearchedArtistKey);
            if ((searchedArtists.Count == 1) && (string.IsNullOrEmpty(searchedArtists[0])))
            {
                searchedArtists.Clear();
            }
            searchedArtists.Reverse();
            List<AutocompleteSearch> result = new List<AutocompleteSearch>();
            foreach (var searched in searchedArtists)
            {
                var partialResults = await this.GetSimilar(searched);
                result.AddRange(partialResults);
            }
            if (result.Count == 0)
            {
                return null;
            }
            /*Random rand = new Random();
            result = result.OrderBy(c => rand.Next()).Select(c => c).ToList();*/
            return result;
        }

        public List<NetEasePlaylist> GetNetEasePlaylist()
        {
            return this.fildoService.GetNetEasePlaylist();
        }

        public async Task ImportNetease(string idpl, string id, string idhash)
        {
            await this.fildoService.ImportNetease(idpl, id, idhash);
        }

        public string GetHash()
        {
            return this.fildoService.Hash;
        }

        public string GetIdUser()
        {
            return this.fildoService.IdUser;
        }

        public async Task<List<Playlist>> GetPlaylists(string user, string pass)
        {
            return await this.fildoService.GetPlaylists(user, pass);
        }

        public async Task<List<Playlist>> GetPublicPlaylists()
        {
            return await this.fildoService.GetPublicPlaylists();
        }

        public async Task<List<SongNetease>> GetNetEasePlaylistSongs(string plid)
        {
            return await this.fildoService.GetNetEasePlaylistSongs(plid);
        }

        public async Task<List<SongNetease>> GetPlaylistSongs(string plid)
        {
            return await this.fildoService.GetPlaylistSongs(plid);
        }

        public async Task<int> GetVersion(int platform)
        {
            return await this.fildoService.GetVersion(platform);
        }

        public async Task<List<Album>> GetTopAlbums()
        {
            return await this.fildoService.GetTopAlbums();
        }

        public async Task<List<AutocompleteSearch>> GetTopArtists()
        {
            return await this.fildoService.GetTopArtists();
        }

        public async Task<VkResult> SearchVK(string toSearch)
        {
            return await this.netEaseService.SearchVK(toSearch);
        }

        public async Task<VkResult> SearchVK(string previousSearch, string captchaSid, string captchaText)
        {
            return await this.netEaseService.SearchVK(previousSearch, captchaSid, captchaText);
        }

        public async void DeletePlaylist(string s)
        {
            await this.fildoService.DeletePlaylist(s);
        }

        public async Task<List<SongQQ>> SearchQQ(string toSearch)
        {
            return await this.netEaseService.SearchQQ(toSearch);
        }

        public async Task<List<SongNetease>> GetSongsForAlbum(string albumId, string artistName)
        {
            var searchedArtists = this.persist.GetStringList(SearchedArtistKey);
            if ((searchedArtists.Count == 1) && (string.IsNullOrEmpty(searchedArtists[0])))
            {
                searchedArtists.Clear();
            }
            if (!searchedArtists.Contains(artistName))
            {
                searchedArtists.Add(artistName);
            }

            if (searchedArtists.Count > 5)
            {
                searchedArtists.RemoveAt(0);
            }

            this.persist.PersistStringList(searchedArtists, SearchedArtistKey);
            return await this.netEaseService.GetSongsForAlbum(albumId);
        }

        public async Task<List<Album>> SearchAlbums(string artistId, string artistName)
        {
            return await this.netEaseService.SearchAlbums(artistId, artistName);
        }

        public async Task<List<SongNetease>> SearchArtist(string id, string name)
        {
            var searchedArtists = this.persist.GetStringList(SearchedArtistKey);
            if ((searchedArtists.Count == 1) && (string.IsNullOrEmpty(searchedArtists[0])))
            {
                searchedArtists.Clear();
            }
            if (!searchedArtists.Contains(name))
            {
                searchedArtists.Add(name);
            }
            
            if (searchedArtists.Count > 5)
            {
                searchedArtists.RemoveAt(0);
            }

            this.persist.PersistStringList(searchedArtists, SearchedArtistKey);
            return await this.netEaseService.SearchArtist(id);
        }

        public async Task<SongNetease> GetSong(string songId)
        {
            return await this.netEaseService.GetSong(songId);
        }

        public async Task<List<AutocompleteSearch>> Autocomplete(string search)
        {
            return await this.netEaseService.Autocomplete(search);
        }

        public async Task<List<AutocompleteSearch>> SearchXiami(string search)
        {
            return await this.netEaseService.SearchXiami(search);
        }

        public async Task<List<SongNetease>> GetXiamiSongs(AutocompleteSearch autocompleteSearch)
        {
            return await this.netEaseService.GetXiamiSongs(autocompleteSearch);
        }

        public async Task<bool> UrlExists(string url)
        {
            return await this.netEaseService.UrlExists(url);
        }

        public async Task<bool> UrlExists(string url, string host)
        {
            return await this.netEaseService.UrlExists(url, host);
        }

        public async Task<string> GetLyric(string id)
        {
            return await this.fildoService.GetLyric(id);
        }

        public async Task<string> Register(string username, string password, string email)
        {
            return await this.fildoService.Register(username, password, email);
        }

        public async Task<List<AutocompleteSearch>> GetSimilar(string artist)
        {
            return await this.fildoService.GetSimilar(artist);
        }
    }
}
