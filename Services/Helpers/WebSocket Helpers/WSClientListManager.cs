using Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Services
{
    public class WSClientListManager
    {
        public ConcurrentDictionary<int, WSClient> Clients = new();
        private readonly IServiceScopeFactory _scopeFactory;
        public WSClientListManager(IServiceScopeFactory scopeFactory) {
            _scopeFactory = scopeFactory;
        }

        public async Task AddClient(int id, WebSocket ws, DateTime validTo)
        {
            await RemoveClient(id, "Another device connected");
            var wSClient = new WSClient(ws,_scopeFactory);
            if (!Clients.TryAdd(id, wSClient))
            {
                await wSClient.CloseAsync("Client could not be added");
                return;
            }
            try
            {
                await wSClient.ListenClient(validTo);
                await RemoveClient(id, "Connection timed out. Please reconnect", ws);
            }
            catch (TokenExpiredException ex)
            {
                await RemoveClient(id, ex.Message, ws);
            }
            catch (Exception ex)
            {
                await RemoveClient(id, ex.Message, ws);
            }

        }

        public async Task RemoveClient(int id, string reason, WebSocket? caller = null)
        {
            if (Clients.TryGetValue(id, out var callee))
            {
                if (caller == null || caller == callee._client)
                {
                    await callee.CloseAsync(reason);
                    Clients.TryRemove(id, out _);
                }
            }
        }

        public WebSocket FindClient(int id)
        {
            if (Clients.TryGetValue(id, out var client))
            {
                if (client != null)
                {
                    return client._client;
                }
            }
            throw new Exception("Client can't found.");
        }
        
    }
}