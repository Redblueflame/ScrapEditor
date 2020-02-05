using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ScrapEditor.ScrapLogic;

namespace TestScrapEditor
{
    public class EGGTests
    {
        [Test]
        public async Task TestLinkGetting()
        {
            var provider = new EGGProvider();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var games = await provider.GetGamesList();
            watch.Stop();
            Console.WriteLine("Execution time: " + watch.ElapsedMilliseconds);
            Assert.Greater(games.Count, 75);
            Console.WriteLine("NbElements: " + games.Count);
        }

        [Test]
        public async Task TestGameGet()
        {
            var provider = new EGGProvider();
            var info = new BasicInfo
            {
                Link = "http://www.everygamegoing.com/landingItem/index/machine_type_group_default_folder/dragon/publisher_folder/dacc/format_folder/tapes/item_title/747-Flight-Simulator-Dacc",
                Console = "Dragon 32",
                InternalId = "11702",
                Name = "747 Simulator",
                Provider = "EveryGameGoing",
            };
            var game = await provider.GetGameInfo(info);
            Assert.AreEqual(
                "No short description is available for this item yet. ", game.Description[0].Value);
            Assert.AreEqual(game.Editor, "Dacc");
            Assert.AreEqual("Arcade; Flight Simulator",game.Genres);
            Assert.AreEqual("1985-03-01", game.ReleaseDate[0].Value);
        }
    }
}