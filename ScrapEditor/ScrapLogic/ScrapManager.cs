using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace ScrapEditor.ScrapLogic
{
    public interface IScrapManager
    {
        void StartScrap();
        List<string> GetConsoles();
        
        Task<GameInfo> ScrapGame(BasicInfo info);
    }

    public class ScrapManager : IScrapManager
    {
        private Database db;
        private List<IScrapProvider> _providers;
        public ScrapManager(Database db)
        {
            this.db = db;
        }

        public async void StartScrap()
        {
            var interfaceType = typeof(IScrapProvider);
            _providers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x=> (IScrapProvider) Activator.CreateInstance(x)).ToList();
            var unlistedGames = new List<BasicInfo>();
            foreach (var provider in _providers)
            {
                Console.WriteLine("Starting scrap with " + provider.GetName());
                //Get list for one provider
                unlistedGames.AddRange(await provider.GetGamesList());
                Console.WriteLine("Finished scrap with " + provider.GetName());
            }
            Console.WriteLine("Starting formatting of games...");
            //Format all games.
            var formattedGames = new Dictionary<long, List<BasicInfo>>();
            var helper = new Dictionary<string, long>();
            long i = 0;
            foreach (var game in unlistedGames)
            {
                if (!helper.ContainsKey(game.Name + game.Console))
                {
                    formattedGames.Add(i, new List<BasicInfo>());
                    game.ScrapEditorID = i;
                    formattedGames[i].Add(game);
                    helper.Add(game.Name + game.Console, i);
                    i++;
                }
                else
                {
                    if (formattedGames[helper[game.Name + game.Console]].All(m => m.Provider != game.Provider))
                        formattedGames[helper[game.Name + game.Console]].Add(game);
                }
            }
            Console.WriteLine("Finished !");
            Console.WriteLine("Starting comparaison with db");
            using (IAsyncDocumentSession session = db.store.OpenAsyncSession())
            {
                //Compare formatted data with the database:
                IQueryable<BasicInfo> query = session
                    .Query<BasicInfo>();

                var results = await session.Advanced.StreamAsync(query);
                while (await results.MoveNextAsync())
                {
                    StreamResult<BasicInfo> gameStream = results.Current;
                    var game = gameStream.Document;
                    if (!helper.ContainsKey(game.Name + game.Console)) continue;
                    formattedGames.Remove(helper[game.Name + game.Console]);
                }
                Thread.Sleep(10000);
                foreach (var (key, value) in formattedGames)
                {
                    foreach (var game in value)
                    {
                        game.Id = game.Provider + "-" + key;
                        await session.StoreAsync(game);
                    }
                }
                await session.SaveChangesAsync();
                Console.WriteLine("Finished !");
            }

        }
        public List<string> GetConsoles()
        {
            var final = new List<string>();
            foreach (var provider in _providers)
            {
                final.AddRange(provider.GetSupportedSystems());
            }
            return final;
        }

        public async Task<GameInfo> ScrapGame(BasicInfo info)
        {
            var provider = _providers.First(x => x.GetName() == info.Provider);
            return await provider.GetGameInfo(info);
        }

        public string GetRegionId(string longname)
        {
            //TODO: Add a permanent Method
            switch (longname)
            {
                case "United Kingdom":
                    return "uk";
                case "world":
                    return "wor";
                case "Spain (ES)":
                    return "sp";
                case "Germany (DE)":
                    return "de";
                case "France (FR)":
                    return "fr";
                default:
                    return "wor";
            }
        }

        public string GetRegionIdName(string longname)
        {
            switch (longname)
            {
                case "United Kingdom":
                    return "eu";
                case "world":
                    return "wor";
                case "Spain (ES)":
                    return "eu";
                case "Germany (DE)":
                    return "eu";
                case "France (FR)":
                    return "eu";
                default:
                    return "wor";
            }
        }

    }
}