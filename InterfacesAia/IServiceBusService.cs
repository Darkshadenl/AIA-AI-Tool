using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace InterfacesAia
{
	public interface IServiceBusService
	{
        Task<HubConnection> ExecuteAsync();
        HubConnection GetConnection();
    }
}

