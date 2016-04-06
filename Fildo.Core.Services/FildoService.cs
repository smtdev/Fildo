using Fildo.Core.DTOs;
using Fildo.Core.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fildo.Core.Services
{
    public class FildoService
    {
        public string Hash { get; set; }
        public string IdUser { get; set; }

        public List<NetEasePlaylist> GetNetEasePlaylist()
        {
            List<NetEasePlaylist> pls = new List<NetEasePlaylist>();
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "10520166", Name = "Electronic Music", PicUrl = "http://p3.music.126.net/5z9yfCsZ7bvTKUNN-jxqow==/7856010581293779.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "180106", Name = "UK chart weekly chart", PicUrl = "http://p4.music.126.net/sNdnRpXmCwFsAj2EX4-OVw==/5666882929660308.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "60198", Name = "American Billboard chart week", PicUrl = "http://p3.music.126.net/koBrRZkYaqrXfep4zr420g==/6058309069460360.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "11641012", Name = "iTunes", PicUrl = "http://p4.music.126.net/hM7U6d25M3oj7tajgcnRjg==/5790028232058309.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "3812895", Name = "Beatport Electronic Dance", PicUrl = "http://p3.music.126.net/VxjQ-nTkeAIcFdXSKbNMug==/1240249116178109.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "3779629", Name = "New Music", PicUrl = "http://p4.music.126.net/LRtxrRfP3g6Gn3vyvWEXOw==/7808731581312441.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "3778678", Name = "Hot Music", PicUrl = "http://p4.music.126.net/eSXJexcoihfSe8ERgOdMnQ==/2920302885027135.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "2884035", Name = "Original Music", PicUrl = "http://p3.music.126.net/LgoU8FreFrdLvSY3ZTFu5g==/2902710698975677.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "19723756", Name = "Soar Music", PicUrl = "http://p3.music.126.net/TkNqminF5jTCzk12Mf5Acg==/7744959906898786.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "21845217", Name = "KTV Hi", PicUrl = "http://p4.music.126.net/UN8g4epoFk-I4DV_C8w32A==/2922501907760617.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "120001", Name = "Hit FM", PicUrl = "http://p4.music.126.net/YqwesOCRe9KPuA4rRQIhfg==/5684475115701568.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "60131", Name = "Japanese Oricon weekly chart", PicUrl = "http://p3.music.126.net/vEaaj-nECgH7kjBRT1DlQA==/5684475115701565.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "3733003", Name = "Korea Melon chart weekly chart", PicUrl = "http://p4.music.126.net/9YSGHPRdVazKSiNGl3uwpg==/5920870115713082.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "60255", Name = "Korea Mnet chart weekly chart", PicUrl = "http://p3.music.126.net/tSl2BF3dZi4cLMD70_fYLw==/5739450697092147.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "46772709", Name = "Korean Melon Soundtrack weekly chart", PicUrl = "http://p3.music.126.net/v_cgiZ305WeM4GJiGIOu7Q==/7815328650414104.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "112504", Name = "China TOP RTHK list)", PicUrl = "http://p3.music.126.net/1MzrbY-8PvvDgFah3uCMig==/5739450697092148.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "64016", Name = "China TOP (Mainland list)", PicUrl = "http://p4.music.126.net/QjYkd-QFVv6e-34_gyePCw==/1269935930127361.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "10169002", Name = "RTHK Chinese song Down", PicUrl = "http://p4.music.126.net/TUJxLWCg2WIlFpvyobXlaQ==/6020925673858141.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "4395559", Name = "Chinese Top", PicUrl = "http://p4.music.126.net/2YXiTGtOn2GwSl4iUfQOHQ==/5985741301868412.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "1899724", Name = "Chinese hip-hop", PicUrl = "http://p3.music.126.net/8gUF9TrXGNoRO6cKVaCzrQ==/5972547162256485.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "27135204", Name = "French NRJ EuroHot 30", PicUrl = "http://p4.music.126.net/kn5Nb3HA-8c3y-KYVVDk-w==/6623458046388376.jpg?param=200y200" });
            pls.Add(new NetEasePlaylist() { IsRealPl = true, Id = "112463", Name = "Taiwan Hito", PicUrl = "http://p4.music.126.net/s09Lt4X_6ckE1lpFHOc2tQ==/5952755952970970.jpg?param=200y200" });
            return pls;
        }

        public async Task SavePL(string userId, string hash, string plName, string plId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("copypl", plId),
                        new KeyValuePair<string, string>("n", plName),
                        new KeyValuePair<string, string>("i", userId),
                        new KeyValuePair<string, string>("i2", hash)
                    });
                    await client.PostAsync("http://fildo.net/pl.php", content);
                }
            }
            catch (Exception)
            {
                
            }

        }

        public async Task SavePL(string userId, string hash, string plName, List<Song> songs)
        {
            try
            {
                string pl = "{\"name\": \"tempPl\",\"id\": null,\"audios\": [{";
                foreach (Song song in songs)
                {
                    pl += "\"aid\": \"" + song.Id + "\",";
                    pl += "\"owner_id\": \"music\",";
                    pl += "\"title\": \"" + song.Title + "\",";
                    pl += "\"artist\": \"" + song.Artist + "\",";
                    pl += "\"duration\": null,";
                    pl += "\"url\": \"" + song.Url + "\",";
                    pl += "\"lyrics_id\": null,";
                    pl += "\"album\": \"\",";
                    pl += "\"mp3\": \"" + song.Url + "\",";
                    pl += "\"name\": \"" + song.Artist + " - " + song.Title + "\",";
                    pl += "\"oga\": \"\",";
                    pl += "\"poster\": \"\"";
                    pl += "}, {";
                }
                pl += "}]}";

                using (HttpClient client = new HttpClient())
                {

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("pl", pl),
                        new KeyValuePair<string, string>("n", plName),
                        new KeyValuePair<string, string>("i", userId),
                        new KeyValuePair<string, string>("i2", hash)
                    });
                    await client.PostAsync("http://fildo.net/pl.php", content);
                }
            }
            catch (Exception)
            {

            }

        }

        public async Task<List<AutocompleteSearch>> GetSimilar(string artist)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var list = await client.GetStringAsync("http://fildo.net/similar.php?artist=" + artist + "&m=1");
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<AutocompleteSearch>>(list);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> Register(string username, string password, string email)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetStringAsync("http://fildo.net/registerExt.php?u=" + username + "&p=" + password + "&m=" + email);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetLyric(string id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetStringAsync("http://fildo.net/lyric.php?songId=" + id);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task ImportNetease(string idpl, string id, string idhash)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    await client.GetStringAsync("http://fildo.net/getplnetease.php?term=" + idpl + "&id=" + id + "&idhash=" + idhash);
                }
            }
            catch (Exception)
            {
                
            }
        }

        public async Task<List<Playlist>> GetPlaylists(string user, string pass)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync("https://fildo.net/pass.php?u=" + user + "&p=" + pass);
                    if (content == "0")
                    {
                        return null;

                    }
                    else if (content.StartsWith("hash"))
                    {
                        this.Hash = content.Split(':')[1];
                        this.IdUser = content.Split(':')[2];
                        return new List<Playlist>();
                    }
                    else
                    {
                        var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Playlist>>(content);
                        this.Hash = list.FirstOrDefault().Hash;
                        this.IdUser = list.FirstOrDefault().IdUser;
                        return list;
                    }
                    
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Playlist>> GetPublicPlaylists()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync("http://fildo.net/publicPlJson.php?data=1");
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Playlist>>(content);
                }
            }
            catch (Exception)
            {
                return new List<Playlist>();
            }

        }

        public async Task<List<SongNetease>> GetNetEasePlaylistSongs(string plid)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync("http://fildo.net/getplnetease.php?term=" + plid);
                    var list = Newtonsoft.Json.JsonConvert.DeserializeObject<NeteasePl>(content);

                    var result = list.audios.Select(p => new SongNetease { Artist = p.artist, Id = p.aid, Title = p.title, Url = p.mp3 }).ToList();

                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<SongNetease>> GetPlaylistSongs(string plid)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync("https://fildo.net/pljson.php?id=" + plid);
                    List<SongPlaylist> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SongPlaylist>>(content);

                    var result = list.Select(p => new SongNetease { Artist = p.artist, Id = p.aid, Title = p.title }).ToList();

                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int> GetVersion(int platform)
        {
            try
            {
                // Android
                if (platform == 1)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string content = await client.GetStringAsync("http://fildo.net/AndroidVersion.txt");
                        int total = 0;
                        var values = content.Split('.');
                        int val;
                        if (int.TryParse(values[0], out val))
                        {
                            total += val * 10000;
                        }
                        if (int.TryParse(values[1], out val))
                        {
                            total += val * 100;
                        }
                        if (int.TryParse(values[2], out val))
                        {
                            total += val;
                        }
                        return total;

                    }
                }

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<List<Album>> GetTopAlbums()
        {
            try
            {
                string url = "http://fildo.net/topalbumsjson.html";
                List<Album> albums = new List<Album>();

                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync(url);
                    var data = JToken.Parse(content);

                    foreach (var albumJsonMain in data)
                    {
                        foreach (var albumJson in albumJsonMain)
                        {
                            Album album = new Album();
                            album.Name = albumJson["album"].ToString();
                            album.Artist = albumJson["name"].ToString();
                            album.AlbumId = albumJson["id"].ToString();
                            album.ImageUrl = albumJson["picUrl"].ToString();
                            albums.Add(album);
                        }
                    }
                }

                return albums;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<AutocompleteSearch>> GetTopArtists()
        {
            try
            {
                string url = "http://fildo.net/topartistsjson.html";
                List<AutocompleteSearch> autocompleteSearches = new List<AutocompleteSearch>();

                using (HttpClient client = new HttpClient())
                {
                    string content = await client.GetStringAsync(url);
                    var data = JToken.Parse(content);

                    foreach (var albumJsonMain in data)
                    {
                        foreach (var albumJson in albumJsonMain)
                        {
                            AutocompleteSearch autocompleteSearch = new AutocompleteSearch();
                            autocompleteSearch.Name = albumJson["name"].ToString();
                            autocompleteSearch.PicUrl = albumJson["picUrl"].ToString();
                            autocompleteSearch.Id = albumJson["id"].ToString();
                            autocompleteSearch.ResultType = "Artist";
                            autocompleteSearches.Add(autocompleteSearch);
                        }
                    }
                }

                return autocompleteSearches;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task DeletePlaylist(string s)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    await client.GetStringAsync("http://fildo.net/pl.php?idDelete=" + s);
                }
            }
            catch (Exception)
            {
                // Ignore it
            }
        }
    }
}
