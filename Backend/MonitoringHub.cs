using Microsoft.AspNetCore.SignalR;
using LiveMonitoringCenter.Services;

namespace LiveMonitoringCenter
{
    public class MonitoringHub : Hub
    {
        private readonly ConnectionsTracker _tracker;

        public MonitoringHub(ConnectionsTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            await _tracker.RegisterConnectionAsync(Context.ConnectionId);
            await Clients.Others.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _tracker.RemoveConnectionAsync(Context.ConnectionId);
            await Clients.Others.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendBroadcastMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveBroadcastMessage", Context.ConnectionId, message);
        }

        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            _tracker.UpdateRoom(Context.ConnectionId, roomName);
            await Clients.Caller.SendAsync("JoinedRoom", roomName);
        }

        public async Task LeaveRoom(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            _tracker.UpdateRoom(Context.ConnectionId, string.Empty);
            await Clients.Caller.SendAsync("LeftRoom", roomName);
        }

        public async Task SendGroupMessage(string roomName, string message)
        {
            await Clients.Group(roomName).SendAsync("ReceiveRoomMessage", Context.ConnectionId, message);
        }

        public async Task SendPrivateMessage(string targetConnectionId, string message)
        {
            await Clients.Client(targetConnectionId).SendAsync("ReceivePrivateMessage", Context.ConnectionId, message);
            await Clients.Caller.SendAsync("PrivateMessageSentConfirmation", targetConnectionId, message);
        }
    }
}
