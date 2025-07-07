using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Services
{
    public class WSClientListManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public ConcurrentDictionary<int, WSClient> Clients = new ConcurrentDictionary<int, WSClient>();
        public WSClientListManager(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task AddClient(int id, WebSocket ws, DateTime validTo)
        {
            if (Clients.ContainsKey(id))
            {
                await RemoveClient(id,"Another device connected.");
            }
            var clientWS = new WSClient(this, ws, _serviceScopeFactory);
            Clients.TryAdd(id, clientWS);
            try
            {
                await clientWS.ListenClient(id, validTo);
            }
            catch (Exception ex)
            {
                await RemoveClient(id, ex.Message);
            }

        }
        public async Task RemoveClient(int id, string reason)
        {
            var client = Clients[id];
            if (client != null)
            {
                await client.Close(reason);

                Clients.TryRemove(id, out var _);
            }
        }
    }
}