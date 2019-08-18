using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace ScrapEditor.ScrapLogic
{
    public class EGGProvider : IScrapProvider
    {
        
        public async Task<List<BasicInfo>> GetGamesList()
        {
            var isResult = true;
            var links = new List<BasicInfo>();
            var page = 0;
            while (isResult)
            {
                try
                {
                    //TODO: Add possiblity to scrap multiple consoles.
                    isResult = false;
                    var html = @"http://www.everygamegoing.com/egg/search/index/?keyword=1&search_type=machine&page=" +
                               page;
                    var web = new HtmlWeb();
                    var htmlDoc = await web.LoadFromWebAsync(html);
                    var tempNodes = htmlDoc.DocumentNode.SelectSingleNode("//div[@id=\"search_results\"]").ChildNodes
                        .Where(node => node.Name == "div")
                        .Skip(1).ToList();
                    var tempInfo = tempNodes.Select(node => node.ChildNodes.Where(elem => elem.Name == "div").ToList())
                        .Select(elems => new BasicInfo
                        {
                            Link = elems[1].ChildNodes.FindFirst("a").GetAttributeValue("href", "none"),
                            InternalId = elems[1].ChildNodes.FindFirst("a").InnerText,
                            Name = elems[2].ChildNodes.FindFirst("a").InnerText,
                            Console = "Dragon 32",
                            Provider = GetName()
                        }).ToList();
                    if (!tempInfo.Any()) continue;
                    isResult = true;
                    links.AddRange(tempInfo);
                    page++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    isResult = true;
                }
            }

            return new HashSet<BasicInfo>(links).ToList();
        }

        public async Task<GameInfo> GetGameInfo(BasicInfo info)
        {
            var html = info.Link;
            var web = new HtmlWeb();
            var htmlDoc = await web.LoadFromWebAsync(html);
            var region = htmlDoc.DocumentNode.SelectSingleNode("//img[@class=\"img narrow-border\"]") != null
                ? htmlDoc.DocumentNode.SelectSingleNode("//img[@class=\"img narrow-border\"]")
                    .GetAttributeValue("alt", "world")
                : "world";
            var genre = htmlDoc.DocumentNode.SelectSingleNode("//span[@itemprop=\"genre\"]")?.InnerText;
            var description = htmlDoc.DocumentNode.SelectSingleNode("//h3[text()=\"How To Play\"]")?.NextSibling
                .InnerText;
            var dateString =
                string.Concat(htmlDoc.DocumentNode.SelectNodes("//p")
                                  .First(p => p.InnerText.Contains("Original Release Date: "))?.InnerText.Skip(24) ??
                              "none".Skip(0))?.Split('\n')?[0];
            DateTime date;
            var replaced = dateString.Substring(0, 4)
                               .Replace("nd", "")
                               .Replace("th", "")
                               .Replace("rd", "")
                               .Replace("st", "")
                           + dateString.Substring(4);
            DateTime.TryParseExact(replaced, "d MMM yyyy",
                new CultureInfo("en-us"), DateTimeStyles.AssumeLocal,
                out date);
            var publisher = htmlDoc.DocumentNode.SelectNodes("//span")
                .First(span => span.InnerText.Contains("Publisher: ")).InnerText.Skip(13);
            var consoleStr = htmlDoc.DocumentNode.SelectNodes("//p")
                .First(p => p.InnerText.Contains("Machine Compatibility: ")).InnerText.Substring(23);
            var consoles = consoleStr.Split(',');
            var console = consoles.FirstOrDefault(c => (GetSupportedSystems()).Contains(c));
            var imgs = htmlDoc.DocumentNode.SelectNodes("//img")
                .Where(img => img.GetAttributeValue("src", "none").Contains("ills_thumbs"))
                .Select(imgNode => imgNode.GetAttributeValue("src", "none")).Select(value =>
                    new RegionalInfo<GameImage>(region, new GameImage {ImgType = "UNKNOWN", ImgURL = value})).ToList();
            var cover = htmlDoc.DocumentNode.SelectSingleNode("//div[text()=\"Front Cover\"]") != null
                ? htmlDoc.DocumentNode.SelectSingleNode("//div[text()=\"Front Cover\"]").ChildNodes.FindFirst("a")
                    .GetAttributeValue("href", "none")
                : null;
            if (cover == null)
            {
                imgs.Add(new RegionalInfo<GameImage>(region, new GameImage
                {
                    ImgType = "COVER_UNTREATED",
                    ImgURL = cover
                }));
            }

            var game = new GameInfo
            {
                Id = GetName() + "-" + info.Id,
                Console = console,
                Genres = genre,
                Description = new List<RegionalInfo<string>> {new RegionalInfo<string>(region, description)},
                Editor = string.Concat(publisher),
                ReleaseDate = new List<RegionalInfo<string>>
                {
                    new RegionalInfo<string>(region,
                        $"{date:yyyy-MM-dd}")
                },
                Names = new List<RegionalInfo<string>> {new RegionalInfo<string>(region, info.Name)},
                Images = imgs,
                Link = info.Link,
                ScrapDate = DateTime.Now,
                Provider = GetName(),
                InternalId = info.ScrapEditorID,
                LastEditTime = DateTime.Now
            };
            return game;
        }

        public List<string> GetSupportedSystems()
        {
            return new List<string> {"Dragon 32"};
        }
        
        public string GetName()
        {
            return "EveryGameGoing";
        }
    }
}