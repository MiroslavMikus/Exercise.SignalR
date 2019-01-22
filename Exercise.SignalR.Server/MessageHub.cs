using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace Exercise.SignalR.Server
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/hubs-api-guide-server
    /// </summary>
    public class MessageHub : Hub
    {
        event EventHandler<string> StateChanged;

        public void Send(string name, string message)
        {
            Clients.All.AddMessage(name, message);
        }

        public override Task OnConnected()
        {
            StateChanged?.Invoke(this, "Client connected: " + Context.ConnectionId);

            return base.OnConnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            StateChanged?.Invoke(this, "Client disconnected: " + Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }
    }
}
