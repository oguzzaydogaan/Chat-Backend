using Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Services
{
    public class WSClientListManager
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public ConcurrentDictionary<int, WSClient> Clients = new();
        public WSClientListManager(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task AddClient(int id, WebSocket ws, DateTime validTo)
        {
            await RemoveClient(id, "Another device connected");
            var clientWS = new WSClient(this, ws, _serviceScopeFactory);
            if (!Clients.TryAdd(id, clientWS))
            {
                await clientWS.Close("Client could not be added");
                return;
            }
            try
            {
                await clientWS.ListenClient(id, validTo);
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
                    await callee.Close(reason);
                    Clients.TryRemove(id, out _);
                }
            }
        }
    }
}