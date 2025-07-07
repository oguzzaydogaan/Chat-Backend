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
                await RemoveClient(id);
            }
            var clientWS = new WSClient(this, ws, _serviceScopeFactory);
            Clients.TryAdd(id, clientWS);
            await clientWS.ListenClient(id, validTo);
        }
        public async Task RemoveClient(int id)
        {
            var client = Clients[id];
            if (client != null)
            {
                await client.Close();
                
                Clients.TryRemove(id, out var _);
            }
        }



        
    }
}