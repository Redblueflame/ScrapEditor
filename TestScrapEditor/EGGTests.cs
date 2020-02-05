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
                Link = "https://www.everygamegoing.com/landingItem/index/machine_type_group_default_folder/dragon/publisher_folder/ai/format_folder/tapes/item_title/Adventureland",
                Console = "Dragon 32",
                InternalId = "11505",
                Name = "Adventureland",
                Provider = "EveryGameGoing",
            };
            var game = await provider.GetGameInfo(info);
            Assert.AreEqual(
                "The first real text adventure, in which you must collect magical objects and return them to a safe location to win. ", game.Description[0].Value);
            Assert.AreEqual("Adventure International", game.Editor);
            Assert.AreEqual("Text Adventure", game.Genres);
            Assert.AreEqual("1984-01-01", game.ReleaseDate[0].Value);
        }
    }
}