using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;
using RestSharp;
using ScrapEditor.LoginLogic;
using ScrapEditor.ScrapLogic;

namespace ScrapEditor
{
    public class ScreenScraperAPI : IScreenScraperAPI
    {
        private readonly ConfigurationFile _config;
        private RestClient _client;
        public ScreenScraperAPI(ConfigurationFile config)
        {
            _config = config;
            _client = new RestClient("https://www.screenscraper.fr/api2");
        }
        /// <summary>
        /// Check if login is valid on ScreenScraper
        /// </summary>
        /// <param name="userName">Username to verify</param>
        /// <param name="password">Password to verify</param>
        /// <returns>True if valid, false if an error occured or invalid</returns>
        public async Task<bool> Login(string userName, string password)
        {
            try
            {
                // TODO: Change the request system
                var request = new RestRequest("jeuRecherche.php", Method.GET)
                    .AddQueryParameter("devid", _config.DevID)
                    .AddQueryParameter("devpassword", _config.DevPassword)
                    .AddQueryParameter("softname", _config.SoftName)
                    .AddQueryParameter("output", "json")
                    .AddQueryParameter("ssid", userName)
                    .AddQueryParameter("sspassword", password);
                var resp = await _client.ExecuteGetAsync(request);
                // Console.WriteLine(person.StatusCode);
                return resp.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                // Console.WriteLine($"Error {e.Call.Response.StatusCode} while making request. \n Error response: {await e.Call.Response.Content.ReadAsStringAsync()} in request {e.Call.FlurlRequest.Url}");
                return false;
            }
        }

        public GameInfo GetScreenScraperInfo(long id, long systemId, User user)
        {
            for (var i = 0; i < 4; i++)
            {
                var request = new RestRequest("jeuInfos.php", Method.GET)
                    .AddQueryParameter("devid", _config.DevID)
                    .AddQueryParameter("devpassword", _config.DevPassword)
                    .AddQueryParameter("softname", _config.SoftName)
                    .AddQueryParameter("output", "json")
                    .AddQueryParameter("ssid", user.Username)
                    .AddQueryParameter("sspassword", user.Password)
                    .AddQueryParameter("systemeid", systemId.ToString())
                    .AddQueryParameter("gameid", id.ToString());
                var resp = _client.Get(request);
                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    Thread.Sleep(500);
                    continue;
                }
                var jsonData = JObject.Parse(resp.Content);
                var game = jsonData["response"]["jeu"];
                var names = game["noms"]?
                    .Select(name =>
                        new RegionalInfo<string>(name["region"].ToString(), name["text"].ToString())
                    ).ToList();
                var editor = game["editeur"]?["text"]?.ToString();
                var developer = game["developpeur"]?["text"]?.ToString();
                var nbPlayers = game["joueurs"]?["text"]?.ToString();
                var synopsis = game["synposis"]?
                    .Select(name =>
                        new RegionalInfo<string>(name["region"].ToString(), name["text"].ToString())
                    ).ToList();
                var releaseDate = game["dates"]?
                    .Select(name =>
                        new RegionalInfo<string>(name["region"].ToString(), name["text"].ToString())
                    ).ToList();
                return new GameInfo()
                {
                    Names = names,
                    Editor = editor,
                    Developer = developer,
                    NbPlayers = nbPlayers,
                    Description = synopsis,
                    ReleaseDate = releaseDate
                };
            }
            return new GameInfo();
        }

        public User GetDefaultUser()
        {
            return new User(_config.DefaultUser, _config.DefaultPassword);
        }
        public long GetGameId(string name, string systemId, User user)
        {
            name = name.ToLowerInvariant().Replace(".", "").Replace(":", "").Replace("-", "");
            for (var i = 0; i < 4; i++)
            {
                try
                {
                    var request = new RestRequest("jeuRecherche.php", DataFormat.Json)
                        .AddQueryParameter("devid", _config.DevID)
                        .AddQueryParameter("devpassword", _config.DevPassword)
                        .AddQueryParameter("softname", _config.SoftName)
                        .AddQueryParameter("output", "json")
                        .AddQueryParameter("ssid", user.Username)
                        .AddQueryParameter("sspassword", user.Password)
                        .AddQueryParameter("systemeid", systemId)
                        .AddQueryParameter("recherche", name);
                    var resp = _client.Get(request);
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    var jsonData = JObject.Parse(resp.Content);
                    foreach (var game in jsonData["response"]["jeux"])
                    {
                        //TODO: Make the system platform invariant
                        if (!game.HasValues || long.Parse(game["systeme"]["id"].ToString()) != 91) continue;
                        if (game["noms"].Select(uScreenscraperName => uScreenscraperName["text"].ToString().ToLowerInvariant()
                            .Replace(".", "")
                            .Replace(":", "")
                            .Replace("-", "")).Any(ssName => name == ssName))
                        {
                            return (long) game["id"];
                        }
                    }
                    return long.MaxValue;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error has occured while executing request to ScreenScraper");
                    Console.WriteLine(e);
                    Thread.Sleep(500);
                }
            }
            return long.MaxValue;
        }
        public bool PublishText(string type, string value, string source, long gameId, User user, string region = "", string lang = "")
        {
            var request = new RestRequest("botProposition.php", Method.POST)
                .AddQueryParameter("ssid", user.Username)
                .AddQueryParameter("sspassword", user.Password);
            request.RequestFormat = DataFormat.Json;
            request.AlwaysMultipartFormData = true;
            request.AddParameter("gameid", gameId.ToString(), ParameterType.GetOrPost)
                .AddParameter("modiftypeinfo", type, ParameterType.GetOrPost)
                .AddParameter("modiftexte", value, ParameterType.GetOrPost)
                .AddParameter("modifsource", source, ParameterType.GetOrPost);
            if (region != "")
            {
                request.AddParameter("modifregion", region, ParameterType.GetOrPost);

            }
            if (lang != "")
            {
                request.AddParameter("modiflangue", lang, ParameterType.GetOrPost);
            }

            var resp = _client.Execute(request);
            return resp.StatusCode == HttpStatusCode.OK;
        }
    }
}