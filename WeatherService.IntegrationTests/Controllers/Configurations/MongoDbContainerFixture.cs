using Testcontainers.MongoDb;

namespace WeatherService.IntegrationTests.Controllers.Configurations
{
    public class MongoDbContainerFixture : IAsyncLifetime
    {
        private readonly MongoDbContainer _container = new MongoDbBuilder()
            .WithImage("mongo:6.0")
            .Build();

        public string ConnectionString => this._container.GetConnectionString();

        public Task InitializeAsync() => this._container.StartAsync();
        public Task DisposeAsync() => this._container.DisposeAsync().AsTask();
    }
}
