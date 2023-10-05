using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace aia_api.src.Services
{
	public class ServiceBusService
    {
        private static string _uri = "http://localhost:5195/uploadZip";

        private static HubConnection _connection;
        public HubConnection Connection { get => _connection; }

        public static async Task<HubConnection> ExecuteAsync()
        {
            _connection = new HubConnectionBuilder().WithUrl(_uri).Build();

            await _connection.StartAsync();
            Console.WriteLine("Connection state: {0}", _connection.State);
            return _connection;
        }

        public static HubConnection GetConnection() => _connection;
    }
}

