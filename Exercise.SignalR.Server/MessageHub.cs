using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exercise.SignalR.Server
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/hubs-api-guide-server
    /// </summary>
    public class MessageHub : Hub
    {
        private static List<string> Users = new List<string>();

        public void SignIn(string name)
        {
            Users.Add(name);

            Clients.All.OnUserChanged(Users);
        }

        public void SignOut(string name)
        {
            Users.Remove(name);

            Clients.All.OnUserChanged(Users);
        }

        public void Send(string name, string message)
        {
            Clients.All.AddMessage(name, message);
        }

        public override Task OnConnected()
        {
            App.Messager.Send<string>("Client connected: " + Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            App.Messager.Send<string>("Client disconnected: " + Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }
        public override async Task OnReconnected()
        {
            await base.OnReconnected();    
        }
    }
}
