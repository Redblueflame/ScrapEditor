using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Moq;
using NUnit.Framework;
using ScrapEditor;
using ScrapEditor.LoginLogic;

namespace Tests
{
    public class Tests
    {
        private HttpTest _httpTest;
        private LoginScreenScraper _login;
        private Configuration _config;
        [SetUp]
        public void CreateHttpTest() {
            _httpTest = new HttpTest();
            _config = new Configuration
            {
                DevID = "PleaseReplaceMe",
                DevPassword = "PleaseReplaceMe",
                SoftName = "ScrapEditor"
            };
            Mock<IScreenScraperAPI> mockAPI = new Mock<IScreenScraperAPI>();
            mockAPI.Setup(t => 
                t.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true)
            );
            var api = mockAPI.Object;
            //Test with a sucessful login
            _login = new LoginScreenScraper(api);
        }

        [TearDown]
        public void DisposeHttpTest() {
            _httpTest.Dispose();
        }

        [Test]
        public void TestNewConfiguration()
        {
            if (File.Exists("test.json"))
            {
                File.Delete("test.json");
            }
            var config = Configuration.LoadConfiguration("test.json");
            Assert.AreEqual("PleaseReplaceMe", config.DevID);
            Assert.AreEqual("PleaseReplaceMe", config.DevPassword);
            Assert.AreEqual("ScrapEditor", config.SoftName);
            var text = File.ReadAllText("test.json");
            Assert.AreEqual("{\"DevID\":\"PleaseReplaceMe\",\"DevPassword\":\"PleaseReplaceMe\",\"SoftName\":\"ScrapEditor\"}", text);
            File.Delete("test.json");
        }

        [Test]
        public void TestExistingConfiguration()
        {
            File.WriteAllText("test.json","{\"DevID\":\"redblueflame\",\"DevPassword\":\"UnitTests\",\"SoftName\":\"ScrapEditorV0.1\"}");
            var config = Configuration.LoadConfiguration("test.json");
            Assert.AreEqual("redblueflame", config.DevID);
            Assert.AreEqual("UnitTests", config.DevPassword);
            Assert.AreEqual("ScrapEditorV0.1", config.SoftName);
            File.Delete("test.json");
        }

        [Test]
        public async Task TestValidScreenScraperLogin()
        {
            _httpTest.RespondWith(
                "{\n\t\"header\" : {\n\t\t\"APIversion\" : \"2.0\",\n\t\t\"dateTime\" : \"2019-08-07 10:20:53\",\n\t\t\"commandRequested\" : \"https://www.screenscraper.fr/api/ssuserInfos.php?devid=redblueflame&devpassword=tests&softname=tests0.1&output=json&ssid=redblueflame&sspassword=tests\",\n\t\t\"success\": \"true\",\n\t\t\"error\": \"\"\n\t},\n\t\"response\" : {\n\t\t\"ssuser\" : {\n\t\t\t\"id\": \"redblueflame\",\n\t\t\t\"niveau\": \"15\",\n\t\t\t\"contribution\": \"4\",\n\t\t\t\"uploadsysteme\": \"0\",\n\t\t\t\"uploadinfos\": \"78\",\n\t\t\t\"romasso\": \"0\",\n\t\t\t\"uploadmedia\": \"23\",\n\t\t\t\"maxthreads\": \"10\",\n\t\t\t\"maxdownloadspeed\": \"43008\",\n\t\t\t\"requeststoday\": \"\",\n\t\t\t\"maxrequestsperday\": \"\",\n\t\t\t\"visites\": \"31\",\n\t\t\t\"datedernierevisite\": \"2019-08-07 10:02:02\",\n\t\t\t\"favregion\": \"\"\n\t\t\t}\n\t\t}\n\t}",
                200);
            var api = new ScreenScraperAPI(_config);
            Assert.IsTrue(await api.Login("test", "test"));
        }

        [Test]
        public async Task TestInvalidScreenScraperLogin()
        {
            _httpTest.RespondWith(
                "Erreur de login : Vérifier les identifiants utilisateurs !            \n\n\n",
                403
            );
            var api = new ScreenScraperAPI(_config);
            Assert.IsFalse(await api.Login("test", "test"));
        }
        [Test]
        public async Task TestGuidLogin() {
            var id = await _login.LoginUser("test", "test");
            Assert.That(id, Does.Match(@"^[a-z0-9]{8}-([a-z0-9]{4}-){3}[a-z0-9]{12}$"));
        }

        [Test]
        public async Task TestGetLogin()
        {
            var id = await _login.GetUser();
        }
    }
}