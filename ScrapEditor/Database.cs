using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using ScrapEditor.ScrapLogic;
namespace ScrapEditor
{
    public interface IDatabase
    {
        IDocumentStore store { get; }
    }

    public class Database : IDatabase
    {
        public IDocumentStore store { get; }
        public Database(ConfigurationFile config)
        {
            // Load certificate
            
            if (config.DBCertPath != "none")
            {
                var clientCertificate = new X509Certificate2(config.DBCertPath);
                store = new DocumentStore
                {
                    Certificate = clientCertificate,
                    Urls = new[] // URL to the Server,
                    {
                        // or list of URLs 
                        config.DBLink // to all Cluster Servers (Nodes)
                    },
                    Database = config.DBName, // Default database that DocumentStore will interact with
                    Conventions = { } // DocumentStore customizations
                };
            }
            else
            {
                store = new DocumentStore
                {
                    Urls = new[] // URL to the Server,
                    {
                        // or list of URLs 
                        config.DBLink // to all Cluster Servers (Nodes)
                    },
                    Database = config.DBName, // Default database that DocumentStore will interact with
                    Conventions = { } // DocumentStore customizations
                };
            }

            store.Initialize();
        }
    }
}