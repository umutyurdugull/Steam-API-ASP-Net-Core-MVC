// Controllers/SteamController.cs
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SteamApiMvcApp.Controllers
{
    public class SteamController : Controller
    {
        private readonly string apiKey = "2EDCB9C589BAC36AEEA34FC390F24E0E";
        private readonly string baseUrl = "http://api.steampowered.com/";
        private readonly HttpClient client = new HttpClient();

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserInfo(string steamId)
        {
            var userInfo = await GetUserInfoAsync(steamId);
            ViewBag.UserInfo = userInfo;
            return View("UserInfo");
        }

        [HttpPost]
        public async Task<IActionResult> FriendList(string steamId)
        {
            var friendListResponse = await GetFriendListAsync(steamId);
            return View("FriendList", friendListResponse);
        }

        [HttpPost]
        public async Task<IActionResult> OwnedGames(string steamId, string gameId)
        {
            var ownedGames = await GetOwnedGamesAsync(steamId, gameId);
            ViewBag.OwnedGames = ownedGames;
            return View("OwnedGames");
        }

        private async Task<string> GetUserInfoAsync(string steamId)
        {
            string url = $"{baseUrl}ISteamUser/GetPlayerSummaries/v2/?key={apiKey}&steamids={steamId}";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseBody);
            JObject player = (JObject)json["response"]["players"][0];
            string personaName = player["personaname"].ToString();
            string profileUrl = player["profileurl"].ToString();
            string avatarUrl = player["avatar"].ToString();
            string loccountrycode = player["loccountrycode"].ToString();
            string steamID = player["steamid"].ToString();
            var profileState = player["profilestate"].ToString();
            var commentPermission = player["commentpermission"].ToString();
            var primaryClanId = player["primaryclanid"].ToString();
            var timeCreated = player["timecreated"].ToString();

            return $"Username: {personaName}\nProfile URL: {profileUrl}\nAvatar URL: {avatarUrl}\nSteam ID: {steamID}\nProfile State: {profileState}\nComment Permission: {commentPermission}\nTime Created: {timeCreated}\nPrimary Clan ID: {primaryClanId}";
        }

        private async Task<FriendListResponse> GetFriendListAsync(string steamId)
        {
            string url = $"{baseUrl}/ISteamUser/GetFriendList/v0001/?key={apiKey}&steamid={steamId}&relationship=friend";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<FriendListResponse>(responseBody);
        }

        public async Task<string> GetOwnedGamesAsync(string steamId, string gameId)
        {
            string url = $"{baseUrl}IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={steamId}&format=json";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseBody);

            int gameCount = (int)json["response"]["game_count"];
            JArray games = (JArray)json["response"]["games"];
            if (games != null)
            {
                foreach (JObject game in games)
                {
                    if (game["appid"].ToString() == gameId)
                    {
                        var playtimeForever = game["playtime_forever"].ToString();
                        var playtime2weeks = game["playtime_2weeks"]?.ToString() ?? "0";
                        return $"Total Games: {gameCount}, Game ID: {gameId}, Playtime Forever: {playtimeForever} minutes, Playtime Last 2 Weeks: {playtime2weeks} minutes";
                    }
                }
            }
            return $"Total Games: {gameCount}, Game with ID {gameId} not found in the user's library.";
        }

        public async Task<string> GetRecentlyPlayedGamesAsync(string steamId)
        {
            string url = $"{baseUrl}IPlayerService/GetRecentlyPlayedGames/v0001/?key={apiKey}&steamid={steamId}&format=json";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseBody);
            JArray games = (JArray)json["response"]["games"];
            if (games != null)
            {
                foreach (JObject game in games)
                {
                    var playtimeForever = game["playtime_forever"].ToString();
                    var playtime2weeks = game["playtime_2weeks"]?.ToString() ?? "0";
                    return $"Playtime Forever: {playtimeForever} minutes, Playtime Last 2 Weeks: {playtime2weeks} minutes";
                }
            }
            return responseBody;
        }
    }

    public class FriendListResponse
    {
        [JsonProperty("friendslist")]
        public FriendsList FriendsList { get; set; }
    }

    public class FriendsList
    {
        [JsonProperty("friends")]
        public List<Friend> Friends { get; set; }
    }

    public class Friend
    {
        [JsonProperty("steamid")]
        public string SteamId { get; set; }

        [JsonProperty("relationship")]
        public string Relationship { get; set; }

        [JsonProperty("friend_since")]
        public long FriendSince { get; set; }
    }
}
