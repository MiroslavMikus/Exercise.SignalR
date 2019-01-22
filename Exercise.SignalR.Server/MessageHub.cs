﻿using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace Exercise.SignalR.Server
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/hubs-api-guide-server
    /// </summary>
    public class MessageHub : Hub
    {
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
    }
}
