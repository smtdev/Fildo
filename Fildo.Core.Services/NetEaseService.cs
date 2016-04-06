namespace Fildo.Core.Services
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
    using Entities;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Globalization;
    using Newtonsoft.Json;
    using DTOs;
    using System.Net;

    using Fildo.Core.DTOs.Xiami;

    public class NetEaseService
    {
        private const string Domain = "http://music.163.com";
        private List<AutocompleteSearch> autocompleteSearches;
        private bool searching;
        public List<string> AvailableProxy { get; private set; } = new List<string>();

        public NetEaseService()
        {
            this.searching = false;
            //foreach (string ip in new string[] { "16", "43" })
            //    AvailableProxy.Add("http://14.215.9." + ip);

            foreach (string ip in new string[] {"33", "34", "35", "36", "37", "38", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "61", "65", "67" })
                this.AvailableProxy.Add("http://219.138.27." + ip);

            //foreach (string ip in new string[] { "13", "17", "18", "19", "21", "28", "31", "32", "34", "35", "37", "175" })
            //    AvailableProxy.Add("http://163.177.171." + ip);
        }

        

        public async Task<List<SongNetease>> GetSongsForAlbum(string albumId)
        {
            try
            {
                string url = Domain + "/api/album/" + albumId + "?ext=true&private_cloud=true&id=" + albumId + "&offset=0&total=true&limit=100";
                if (!this.searching)
                {
                    List<SongNetease> songs = new List<SongNetease>();
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Referrer = new Uri("http://music.163.com");
                        client.DefaultRequestHeaders.Host = "music.163.com";

                        string content = await client.GetStringAsync(url);
                        var data = JToken.Parse(content);
                        var songsJson = data["songs"];
                        this.GetValue(songsJson, null, songs);
                    }

                    return songs;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Album>> SearchAlbums(string artistId, string artistName)
        {
            try
            {
                if (!this.searching)
                {
                    string url = Domain + "/api/artist/albums/" + artistId + "?id=" + artistId + "&offset=0&total=true&limit=100";

                    List<Album> albums = new List<Album>();
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Referrer = new Uri("http://music.163.com");
                        client.DefaultRequestHeaders.Host = "music.163.com";

                        string content = await client.GetStringAsync(url);
                        var data = JToken.Parse(content);
                        var albumsJson = data["hotAlbums"];
                        foreach (var albumJson in albumsJson)
                        {
                            Album album = new Album();
                            album.AlbumId = albumJson["id"].ToString();
                            album.ImageUrl = albumJson["picUrl"].ToString();
                            album.Artist = artistName;
                            album.Name = albumJson["name"].ToString();
                            albums.Add(album);
                        }
                    }

                    return albums;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<SongNetease>> SearchArtist(string id)
        {
            try
            {
                if (!this.searching)
                {
                    string url = Domain + "/api/artist/" + id + "?ext=true&private_cloud=true&top=100&id=" + id;

                    List<SongNetease> hotSongs = new List<SongNetease>();
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Referrer = new Uri("http://music.163.com");
                        client.DefaultRequestHeaders.Host = "music.163.com";

                        string content = await client.GetStringAsync(url);
                        var data = JToken.Parse(content);
                        var songs = data["hotSongs"];
                        this.GetValue(songs, data, hotSongs);
                    }

                    return hotSongs;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UrlExists(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }   

        public async Task<bool> UrlExists(string url, string host)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5);
                    
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);
                    request.Headers.Referrer = new Uri("http://music.163.com");
                    request.Headers.Host = host;
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public async Task<List<AutocompleteSearch>> Autocomplete(string search)
        {
            try
            {
                this.autocompleteSearches = new List<AutocompleteSearch>();
                this.searching = true;
                string url = Domain + "/api/search/suggest/web";
                string Parameters = Uri.EscapeUriString("s=" + search + "&limit=10");

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 20);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

                    //request.Headers.Accept.Add(new MediaTypeWithQualityHeadxerValue("text/plain"));
                    request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8", 0.7));
                    request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us", 0.5));
                    request.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(Parameters)));
                    request.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    request.Headers.Host = "music.163.com";
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
                    request.Headers.Referrer = new Uri("http://music.163.com");

                    var result = await client.SendAsync(request);
                    var content = await result.Content.ReadAsStringAsync();
                    //dynamic obj = JObject.Parse(content);
                    var data = JToken.Parse(content);
                    var jtoken = data["result"]["artists"];
                    this.CompleteResults(jtoken, "Artist");
                    jtoken = data["result"]["albums"];
                    this.CompleteResults(jtoken, "Album");
                    jtoken = data["result"]["songs"];
                    this.CompleteResults(jtoken, "Song");

                    this.searching = false;    
                }
                
                return this.autocompleteSearches;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<SongQQ>> SearchQQ(string toSearch)
        {
            try
            {
                List<SongQQ> songs = new List<SongQQ>();
                using (HttpClient client = new HttpClient())
                {
                    string qqContent = await client.GetStringAsync("http://open.music.qq.com/fcgi-bin/fcg_weixin_music_search.fcg?remoteplace=txt.weixin.officialaccount&w=" + toSearch + "&platform=weixin&jsonCallback=MusicJsonCallback&perpage=50&curpage=1");
                    qqContent = qqContent.Split(new string[] { "(" }, 2, StringSplitOptions.None)[1];
                    qqContent = qqContent.TrimEnd(')');
                    var data = JToken.Parse(qqContent);
                    JArray temp = (JArray)data["list"];
                    if (temp == null)
                    {
                        songs.Add(new SongQQ() { Artist = "CaptchaError" });
                        return songs;
                    }
                    else
                    {
                        var songkkDtos = temp.ToObject<List<SongQQDto>>();

                        foreach (var item in songkkDtos)
                        {
                            SongQQ song = new SongQQ();

                            song.Title = item.songname;
                            song.Artist = item.singername;
                            song.Url = item.m4a;
                            songs.Add(song);
                        }
                    }
                }

                return songs;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<AutocompleteSearch>> SearchXiami(string toSearch)
        {
            try
            {
                var result = new List<AutocompleteSearch>();
                this.searching = true;
                string url = "http://www.xiami.com/search/json?t=4&k=amaral&n=3";

                using(HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 20);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Referrer = new Uri("http://www.xiami.com");
                    
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var xiamiDto = JsonConvert.DeserializeObject<XiamiAutocompleteDto>(content);
                        foreach (XiamiSongDto xiamiSongDto in xiamiDto.songs)
                        {
                            AutocompleteSearch song = new AutocompleteSearch();
                            song.AutoCompleteType = AutoCompleteType.Xiami;
                            song.Id = xiamiSongDto.song_id;
                            song.ArtistName = xiamiSongDto.artist_name;
                            song.ResultType = "Song";
                            song.Name = xiamiSongDto.song_name;
                            result.Add(song);
                        }

                        foreach (var dto in xiamiDto.albums)
                        {
                            AutocompleteSearch album = new AutocompleteSearch();
                            album.AutoCompleteType = AutoCompleteType.Xiami;
                            album.PicUrl = "http://img.xiami.net/" + dto.album_logo;
                            album.Name = dto.title;
                            album.ArtistName = dto.artist_name;
                            album.Id = dto.album_id;
                            album.ResultType = "Album";
                            result.Add(album);
                        }

                        foreach (var dto in xiamiDto.artists)
                        {
                            AutocompleteSearch artist = new AutocompleteSearch();
                            artist.AutoCompleteType = AutoCompleteType.Xiami;
                    
                            artist.PicUrl = "http://img.xiami.net/" + dto.logo;
                            artist.Name = dto.name;
                            artist.Id = dto.artist_id;
                            artist.ResultType = "Artist";
                            result.Add(artist);
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public async Task<List<SongNetease>> GetXiamiSongs(AutocompleteSearch autocompleteSearch)
        {
            List<SongNetease> result = new List<SongNetease>();
            if (autocompleteSearch.ResultType == "Artist")
            {
                string url = "http://www.xiami.com/song/playlist/id/" + autocompleteSearch.Id + "/type/2/cat/json";

                result = await this.ParseAutocompleteXiami(url);
            }
            else if (autocompleteSearch.ResultType == "Album")
            {
                string url = "http://www.xiami.com/song/playlist/id/" + autocompleteSearch.Id + "/type/1/cat/json";

                result = await this.ParseAutocompleteXiami(url);
            }
            else if (autocompleteSearch.ResultType == "Song")
            {
                string url = "http://www.xiami.com/song/playlist/id/" + autocompleteSearch.Id + "/cat/json";

                result = await this.ParseAutocompleteXiami(url);
            }
            return result;
        }

        private async Task<List<SongNetease>> ParseAutocompleteXiami(string url)
        {
            List<SongNetease> result = new List<SongNetease>();
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 20);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Referrer = new Uri("http://www.xiami.com");
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux i686) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1500.95 Safari/537.36");
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var xiamiDto = JsonConvert.DeserializeObject<XiamiSongResponseDto>(content);
                    foreach (XiamiTrack xiamiTrack in xiamiDto.data.trackList)
                    {
                        result.Add(
                            new SongNetease()
                            {
                                Id = xiamiTrack.song_id,
                                Url = this.DecryptXiami(xiamiTrack.location),
                                Artist = xiamiTrack.artist,
                                Title = xiamiTrack.title,
                                Duration = xiamiTrack.length
                            });
                    }
                }
            }
            return result;
        }

        private async Task<string> GetVT()
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync("http://fildo.net/vt.txt");
            }
        }

        public async Task<VkResult> SearchVK(string toSearch, string captchaSid, string captchaText)
        {
            string vt = await this.GetVT();
            return await this.SearchVKWorker(toSearch, "https://api.vkontakte.ru/method/audio.search?access_token=" + vt + "&offset=0&count=200&auto_complete=1&callback=callbackFunc&q=" + toSearch + "&callback=jQuery171049866019282490015_1453655813358&_=1453658418847&captcha_sid=" + captchaSid + "&captcha_key=" + captchaText);
        }

        public async Task<VkResult> SearchVK(string toSearch)
        {
            string vt = await this.GetVT();
            return await this.SearchVKWorker(toSearch, "https://api.vkontakte.ru/method/audio.search?access_token=" + vt + "&offset=0&count=200&auto_complete=1&callback=callbackFunc&q=" + toSearch + " &callback=jQuery171049866019282490015_1453655813358&_=1453658418847");
        }

        private async Task<VkResult> SearchVKWorker(string toSearch, string url)
        {
            VkResult result = new VkResult();
            try
            {
                List<SongVK> songs = new List<SongVK>();
                using (HttpClient client = new HttpClient())
                {
                    string vkContent = await client.GetStringAsync(url);
                    vkContent = vkContent.Split(new string[] { "(" }, 2, StringSplitOptions.None)[1];
                    vkContent = vkContent.Replace("});", "}");
                    if (vkContent.StartsWith("{\"err"))
                    {
                        result.Error = true;
                        VkErrorCaptcha errorCaptcha = JsonConvert.DeserializeObject<VkErrorCaptcha>(vkContent);
                        result.CaptchaSid = errorCaptcha.error.captcha_sid;
                        result.CaptchaUrl = errorCaptcha.error.captcha_img;
                        result.PreviousSearch = toSearch;
                    }
                    else
                    { 
                        var data = JToken.Parse(vkContent);
                        JArray temp = (JArray)data["response"];

                        temp.RemoveAt(0);

                        var listVKOk = temp.ToObject<List<SongVKDto>>();

                        foreach (var item in listVKOk)
                        {
                            SongVK song = new SongVK();

                            song.Title = item.title;
                            song.Artist = item.artist;
                            song.Url = item.url;
                            song.Duration = item.duration;
                            songs.Add(song);
                        }
                        
                    }
                }

                result.Songs = songs;
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<SongNetease> GetSong(string songId)
        {
            try
            {
                string url = Domain + "/api/song/detail?ids=[" + songId + "]";

                if (!this.searching)
                {
                    SongNetease song = new SongNetease();
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Referrer = new Uri("http://music.163.com");
                        client.DefaultRequestHeaders.Host = "music.163.com";

                        string content = await client.GetStringAsync(url);
                        var data = JToken.Parse(content);
                        var songsJson = data["songs"];
                        var asdfasdf = JsonConvert.DeserializeObject<IEnumerable<SongDto>>(songsJson.ToString()).ToList();
                        var artist = data;

                        string extension = ".mp3";

                        foreach (var songJson in songsJson)
                        {
                            var artistJson = songJson["artists"].FirstOrDefault();

                            // TODO Improve this fucking shit.
                            try
                            {
                                song.Title = songJson["hMusic"]["name"].ToString();
                                song.Id = songJson["hMusic"]["dfsId"].ToString();
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    song.Title = songJson["mMusic"]["name"].ToString();
                                    song.Id = songJson["mMusic"]["dfsId"].ToString();
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        song.Title = songJson["lMusic"]["name"].ToString();
                                        song.Id = songJson["lMusic"]["dfsId"].ToString();
                                    }
                                    catch (Exception)
                                    {
                                        try
                                        {
                                            song.Title = songJson["bMusic"]["name"].ToString();
                                            song.Id = songJson["bMusic"]["dfsId"].ToString();
                                        }
                                        catch (Exception)
                                        {
                                            try
                                            {
                                                song.Title = songJson["audition"]["name"].ToString();
                                                song.Id = songJson["audition"]["dfsId"].ToString();
                                                extension = ".m4a";
                                            }
                                            catch (Exception)
                                            {


                                            }
                                        }
                                    }
                                }
                            }

                            song.Artist = artistJson["name"].ToString();
                            var dur = songJson["duration"].ToString();
                            int duration;
                            if (int.TryParse(dur, out duration))
                            {
                                song.Duration = duration;
                            }
                            else
                            {
                                song.Duration = 0;
                            }
                            
                            song.Url = "http://p3.music.126.net/" + this.Decrypt(song.Id) + "/" +
                                        song.Id + extension;
                            break;
                        }
                    }

                    return song;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void GetValue(JToken songs, JToken data, List<SongNetease> hotSongs)
        {
            foreach (var songJson in songs)
            {
                SongNetease song = new SongNetease();
                if (songJson["h"].HasValues && (songJson["h"]["fid"].ToString() != "0"))
                {
                    song.Title = songJson["name"].ToString();
                    song.Id = songJson["id"].ToString();
                    song.Artist = songJson["ar"][0]["name"].ToString();
                    song.Url = "http://p3.music.126.net/" + this.Decrypt(songJson["h"]["fid"].ToString()) + "/" +
                               songJson["h"]["fid"].ToString() + ".mp3";
                    if (data != null)
                    {
                        song.Artist = data["artist"]["name"].ToString();
                    }
                }
                else if (songJson["m"].HasValues && (songJson["m"]["fid"].ToString() != "0"))
                {
                    song.Title = songJson["name"].ToString();
                    song.Id = songJson["id"].ToString();
                    song.Artist = songJson["ar"][0]["name"].ToString();
                    song.Url = "http://p3.music.126.net/" + this.Decrypt(songJson["m"]["fid"].ToString()) + "/" +
                               songJson["m"]["fid"].ToString() + ".mp3";
                    if (data != null)
                    {
                        song.Artist = data["artist"]["name"].ToString();
                    }
                }
                else if (songJson["l"].HasValues && (songJson["l"]["fid"].ToString() != "0"))
                {
                    song.Title = songJson["name"].ToString();
                    song.Id = songJson["id"].ToString();
                    song.Artist = songJson["ar"][0]["name"].ToString();
                    song.Url = "http://p3.music.126.net/" + this.Decrypt(songJson["l"]["fid"].ToString()) + "/" +
                               songJson["l"]["fid"].ToString() + ".mp3";
                    if (data != null)
                    {
                        song.Artist = data["artist"]["name"].ToString();
                    }
                }
                else
                {
                    song.Title = songJson["name"].ToString();
                    song.Id = songJson["id"].ToString();
                    song.Artist = songJson["ar"][0]["name"].ToString();
                    song.Url = string.Empty;
                    if (data != null)
                    {
                        song.Artist = data["artist"]["name"].ToString();
                    }
                }

                /*var dur = songJson["duration"].ToString();
                int duration;
                if (int.TryParse(dur, out duration))
                {
                    song.Duration = duration;
                }
                else
                {
                    song.Duration = 0;
                }*/
                
                hotSongs.Add(song);
            }
        }

        private string Decrypt(string id)
        {
            char[] key = "3go8&$8*3*3h0k(2)2".ToCharArray();
            char[] byte2 = id.ToCharArray();
            for (int i = 0; i < id.Length; i++)
            {
                byte2[i] = (char)(byte2[i] ^ key[i % key.Length]);
            }
            string decripted = new string(byte2);
            string result = ComputeMD5(decripted);
            return result.Replace("/", "_").Replace("+", "-");
        }

        private string DecryptXiami(string encodedLocation)
        {
            var sectionCount = int.Parse(encodedLocation[0].ToString());
            var code = encodedLocation.Substring(1);
            int length = (code.Length / sectionCount) + 1;
            var remainder = code.Length % sectionCount;
            string[] sections = new string[sectionCount];
            var result = string.Empty;

            // split to a few sections
            for (var i = 0; i < sectionCount; i++)
            {
                if (i < remainder)
                {
                    sections[i] = (code.Substring(length * i, length));
                }
                else {
                    sections[i] = (code.Substring((length - 1) * i + remainder, length - 1));
                }
            }

            try
            {

                // rebuild url
                for (var j = 0; j < sections[0].Length; j++)
                {
                    for (var k = 0; k < sections.Count(); k++)
                    {
                        if (k < sections.Count())
                        {
                            result += sections[k].ToCharArray()[j];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ignore
            }

            result = Uri.UnescapeDataString(result);
            return result.Replace('^', '0').Replace('+', ' ');
        }

        private static string ComputeMD5(string str)
        {
            byte[] md5computed = xBrainLab.Security.Cryptography.MD5.GetHash(str);
            return Convert.ToBase64String(md5computed, 0, md5computed.Length);
        }

        private void CompleteResults(JToken jtoken, string type)
        {
            if (jtoken == null)
            {
                return;
            }

            foreach (var item in jtoken)
            {
                string artistInfo = item.ToString();
                var artistData = JToken.Parse(artistInfo);
                if (type == "Artist")
                {
                    var artistImage = artistData["picUrl"];
                    if (string.IsNullOrEmpty(artistImage.ToString()))
                    {
                        artistImage = artistData["img1v1Url"];
                    }

                    this.autocompleteSearches.Add(new AutocompleteSearch()
                    {
                        ResultType = type,
                        PicUrl = artistImage.ToString(),
                        Name = artistData["name"].ToString(),
                        Id = artistData["id"].ToString()
                    });
                }
                else if (type == "Album")
                {
                    string artistImage = artistData["picId"].ToString();
                    string url = "http://p3.music.126.net/" + this.Decrypt(artistImage) + "/" + artistImage + ".jpg";
                    var tempauto = new AutocompleteSearch()
                    {
                        ResultType = type,
                        PicUrl = url,
                        Name = artistData["name"].ToString(),
                        Id = artistData["id"].ToString()
                    };
                    try
                    {
                        tempauto.ArtistName = artistData["artist"]["name"].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    this.autocompleteSearches.Add(tempauto);
                }
                else if (type == "Song")
                {
                    var artistImage = artistData["album"]["picId"].ToString();
                    string url = "http://p3.music.126.net/" + this.Decrypt(artistImage) + "/" + artistImage + ".jpg";

                    this.autocompleteSearches.Add(new AutocompleteSearch()
                    {
                        ResultType = type,
                        PicUrl = url,
                        Name = artistData["name"].ToString(),
                        Id = artistData["id"].ToString(),
                        Duration = item["duration"].ToString()
                    });
                }
            }
        }

        

        private async Task<List<T>> DoRequest<T>(string uriRest)
        {
            using (var httpClient = new HttpClient())
            {
                Uri uri = new Uri(uriRest);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Referrer = new Uri("http://"+ uri.Host);
                httpClient.DefaultRequestHeaders.Host = uri.Host;

                var completeUri = string.Format(CultureInfo.InvariantCulture, "{0}{1}", Domain, uriRest);
                var response = await httpClient.GetAsync(completeUri);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new List<T>();
                    }

                    return JsonConvert.DeserializeObject<IEnumerable<T>>(json).ToList();
                }
                else
                {
                    return new List<T>();
                }
            }
        }
    }
}

