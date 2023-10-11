using aia_api.Configuration.Records;
using InterfacesAia;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace aia_api.src.Services
{
	public class ServiceBusService : IServiceBusService
    {
        private readonly IOptions<Settings> _settings;
        private HubConnection _connection;

        public ServiceBusService(IOptions<Settings> settings)
        {
            _settings = settings;
        }

        public async Task<HubConnection> ExecuteAsync()
        {
            var uri = _settings.Value.ServiceBusUrl;
            _connection = new HubConnectionBuilder().WithUrl(uri).Build();


            await _connection.StartAsync();
            Console.WriteLine("Connection state: {0}", _connection.State);
            return _connection;
        }

        public HubConnection GetConnection() => _connection;
    }
}

