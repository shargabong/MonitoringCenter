using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace LiveMonitoringCenter.Services
{
    public class ConnectionsTracker
    {
        private readonly ConcurrentDictionary<string, string> _connections = new();
        private readonly IHubContext<MonitoringHub> _hubContext;

        public ConnectionsTracker(IHubContext<MonitoringHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task RegisterConnectionAsync(string connectionId)
        {
            _connections.TryAdd(connectionId, string.Empty);
            await NotifyOnlineCountAsync();
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
            await NotifyOnlineCountAsync();
        }

        public void UpdateRoom(string connectionId, string roomName)
        {
            if (_connections.ContainsKey(connectionId))
            {
                _connections[connectionId] = roomName;
            }
        }

        public async Task SendSystemMessageAsync(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveSystemMessage", message);
        }

        public async Task SendExternalRoomNotificationAsync(string roomName, string message)
        {
            await _hubContext.Clients.Group(roomName).SendAsync("ReceiveRoomMessage", "System_External", message);
        }

        public async Task SendExternalPrivateMessageAsync(string connectionId, string message)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceivePrivateMessage", "System_External", message);
        }

        private async Task NotifyOnlineCountAsync()
        {
            int count = _connections.Count;
            await _hubContext.Clients.All.SendAsync("UpdateOnlineCount", count);
        }
    }
}
