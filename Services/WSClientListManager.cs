using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Services
{
    public class WSClientListManager
    {

        public ConcurrentDictionary<int, WSClient> Clients = new ConcurrentDictionary<int, WSClient>();

        public async Task AddClient(int id, WebSocket ws, DateTime validTo, MessageService messageService)
        {
            if (Clients.ContainsKey(id))
            {
                await RemoveClient(id);
            }
            var clientWS = new WSClient(this, ws, messageService);
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