using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Moq;
using NUnit.Framework;
using RestSharp;
using ScrapEditor;
using ScrapEditor.LoginLogic;

namespace TestScrapEditor
{
    public class LoginTests
    {
        private HttpTest _httpTest;
        private LoginScreenScraper _loginValid;
        private ConfigurationFile _config;
        private string _id;
        [SetUp]
        public void CreateHttpTest() {
            _httpTest = new HttpTest();
            _config = new ConfigurationFile
            {
                DevID = "PleaseReplaceMe",
                DevPassword = "PleaseReplaceMe",
                SoftName = "ScrapEditor",
                DBLink = "http://live-test.ravendb.net",
                DBCertPath = "none"
            };
            Mock<IScreenScraperAPI> mockAPI = new Mock<IScreenScraperAPI>();
            mockAPI.Setup(t => 
                t.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true)
            );
            var api = mockAPI.Object;
            //Test with a sucessful login
            _loginValid = new LoginScreenScraper(api);
            _id = _loginValid.AddUser("UnitTestsUser", "UnitTestsPasswd").Result;
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
            var config = ConfigurationFile.LoadConfiguration("test.json");
            Assert.AreEqual("PleaseReplaceMe", config.DevID);
            Assert.AreEqual("PleaseReplaceMe", config.DevPassword);
            Assert.AreEqual("ScrapEditor", config.SoftName);
            Assert.AreEqual("http://live-test.ravendb.net", config.DBLink);
            Assert.AreEqual("ScrapEditor-Dev", config.DBName);
            Assert.AreEqual("none", config.DBCertPath);
            Assert.AreEqual("ReplaceMePlease", config.DefaultUser);
            Assert.AreEqual("xxxyyyzzz", config.DefaultPassword);
            Assert.That(config.AuthKey, Does.Match("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$"));
            var text = File.ReadAllText("test.json");
            File.Delete("test.json");
        }

        [Test]
        public void TestExistingConfiguration()
        {
            File.WriteAllText("test.json","{\"DevID\":\"redblueflame\",\"DevPassword\":\"UnitTests\",\"SoftName\":\"ScrapEditorV0.1\",\"DBLink\":\"http://live-test.ravendb.net\",\"DBCertPath\":\"none\", \"DBName\":\"ScrapEditor-Dev\"}");
            var config = ConfigurationFile.LoadConfiguration("test.json");
            Assert.AreEqual("redblueflame", config.DevID);
            Assert.AreEqual("UnitTests", config.DevPassword);
            Assert.AreEqual("ScrapEditorV0.1", config.SoftName);
            Assert.AreEqual("http://live-test.ravendb.net", config.DBLink);
            Assert.AreEqual("none", config.DBCertPath);
            Assert.AreEqual("ScrapEditor-Dev", config.DBName);
            File.Delete("test.json");
        }
        /*
        [Test]
        public async Task TestValidScreenScraperLogin()
        {
            _httpTest.RespondWith(
                "{\n\t\"header\" : {\n\t\t\"APIversion\" : \"2.0\",\n\t\t\"dateTime\" : \"2019-08-07 10:20:53\",\n\t\t\"commandRequested\" : \"https://www.screenscraper.fr/api/ssuserInfos.php?devid=redblueflame&devpassword=tests&softname=tests0.1&output=json&ssid=redblueflame&sspassword=tests\",\n\t\t\"success\": \"true\",\n\t\t\"error\": \"\"\n\t},\n\t\"response\" : {\n\t\t\"ssuser\" : {\n\t\t\t\"id\": \"redblueflame\",\n\t\t\t\"niveau\": \"15\",\n\t\t\t\"contribution\": \"4\",\n\t\t\t\"uploadsysteme\": \"0\",\n\t\t\t\"uploadinfos\": \"78\",\n\t\t\t\"romasso\": \"0\",\n\t\t\t\"uploadmedia\": \"23\",\n\t\t\t\"maxthreads\": \"10\",\n\t\t\t\"maxdownloadspeed\": \"43008\",\n\t\t\t\"requeststoday\": \"\",\n\t\t\t\"maxrequestsperday\": \"\",\n\t\t\t\"visites\": \"31\",\n\t\t\t\"datedernierevisite\": \"2019-08-07 10:02:02\",\n\t\t\t\"favregion\": \"\"\n\t\t\t}\n\t\t}\n\t}",
                200);
            var restClient = new Mock<IRestClient>();

            restClient
                .Setup(x => x.Execute(
                    It.IsAny<IRestRequest>(),
                    It.IsAny<Action<IRestResponse, RestRequestAsyncHandle>>()))
                .Callback<IRestRequest, Action<IRestResponse, RestRequestAsyncHandle>>((request, callback) =>
                {
                    callback(new RestResponse { StatusCode = HttpStatusCode.OK }, null);
                });
            var api = new ScreenScraperAPI(_config);
            Assert.IsTrue(await api.Login("test", "test"));
        }
        */
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
            var id = await _loginValid.LoginUser("test", "test");
            Assert.That(id, Does.Match(@"^[a-z0-9]{8}-([a-z0-9]{4}-){3}[a-z0-9]{12}$"));
        }

        [Test]
        public async Task TestGetLogin()
        {
            var user = await _loginValid.GetUser(_id);
            Assert.AreEqual("UnitTestsUser", user.Username);
            Assert.AreEqual("UnitTestsPasswd", user.Password);
        }
    }
}