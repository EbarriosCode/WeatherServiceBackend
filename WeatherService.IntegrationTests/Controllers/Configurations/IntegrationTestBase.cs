using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MongoDB.Driver;
using static System.Net.WebRequestMethods;

namespace WeatherService.IntegrationTests.Controllers.Configurations
{
    public class IntegrationTestBase : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MongoDbContainerFixture _mongoFixture = new();
        private IMongoDatabase _database = default!;
        public HttpClient Client { get; private set; } = default!;

        public async Task InitializeAsync()
        {
            await this._mongoFixture.InitializeAsync();
            Client = CreateClient();

            this._database = new MongoClient(this._mongoFixture.ConnectionString).GetDatabase("WeatherIntegrationTests");
        }

        public new async Task DisposeAsync()
        {
            await this._mongoFixture.DisposeAsync();
            await base.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            await this._database.DropCollectionAsync("WeatherRecords");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MongoSettings:ConnectionString"] = this._mongoFixture.ConnectionString,
                    ["MongoSettings:DatabaseName"] = "WeatherIntegrationTests",
                    ["MongoSettings:CollectionName"] = "WeatherRecords",
                    ["Cache:TtlHours"] = "3",
                    ["OpenMeteo:BaseUrl"] = "https://api.open-meteo.com/v1/",
                    ["OpenMeteo:GeoCodingBaseUrl"] = "https://geocoding-api.open-meteo.com/v1/"
                });
            });
        }
    }
}
