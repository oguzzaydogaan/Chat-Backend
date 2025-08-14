using Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Helpers.WebSocket_Helpers;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Services
{
    public class WSListManager
    {
        public ConcurrentDictionary<int, WebSocket> Clients = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WSListManager> _logger;
        public WSListManager(IServiceScopeFactory scopeFactory, ILogger<WSListManager> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task AddClient(int id, WebSocket webSocket, DateTime validTo)
        {
            await RemoveClient(id, "Another device connected");
            using var scope = _scopeFactory.CreateScope();
            var _wsManager = scope.ServiceProvider.GetRequiredService<WSManager>();
            if (!Clients.TryAdd(id, webSocket))
            {
                await _wsManager.CloseAsync(id, webSocket, "Something went wrong. Please try again later.");
                _logger.LogError($"WebSocket client with id {id} could not be added to the list.");
                return;
            }
            try
            {
                await _wsManager.ListenClientAsync(webSocket, validTo);
                await RemoveClient(id, "Connection timed out. Please reconnect", webSocket);
            }
            catch (TokenExpiredException ex)
            {
                await RemoveClient(id, ex.Message, webSocket);
            }
            catch(ConfigurationException ex)
            {
                _logger.LogError(ex.Message);
                await RemoveClient(id, "System error. Please try again later.", webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await RemoveClient(id, ex.Message, webSocket);
            }

        }

        public async Task RemoveClient(int id, string reason, WebSocket? caller = null)
        {
            if (Clients.TryGetValue(id, out var callee))
            {
                if (caller == null || caller == callee)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var _wsManager = scope.ServiceProvider.GetRequiredService<WSManager>();
                    await _wsManager.CloseAsync(id, callee, reason);
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
                    return client;
                }
            }
            throw new Exception("Client can't found.");
        }
        
    }
}