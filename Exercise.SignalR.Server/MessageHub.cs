using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exercise.SignalR.Server
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/hubs-api-guide-server
    /// </summary>
    public class MessageHub : Hub
    {
        // Connection ID and name
        private static readonly Dictionary<string, string> Users = new Dictionary<string,string>();
        private static readonly Dictionary<string, HashSet<string>> Rooms = new Dictionary<string, HashSet<string>>();

        public async Task SignIn(string name)
        {
            Users[Context.ConnectionId] = name;

            App.Messager.Send<string>($"User {name} connected");

            await JoinRoom("Main room");

            var userRooms = Users.Select(a => new { Name = a.Value, Rooms = Rooms[a.Key] });

            foreach (var user in userRooms)
            {
                foreach (var room in user.Rooms)
                {
                    Clients.Caller.UserJoinRoom(room, user);
                }
            }
        }

        public void Send(string name, string room, string message)
        {
            Clients.Group(room).AddMessage(name, room, message);
        }

        public async Task JoinRoom(string roomName)
        {
            await Groups.Add(Context.ConnectionId, roomName);

            Rooms[roomName].Add(Users[Context.ConnectionId]);

            Clients.All.UserJoinRoom(roomName, Users[Context.ConnectionId]);
        }

        public async Task LeaveRoom(string roomName)
        {
            await Groups.Remove(Context.ConnectionId, roomName);

            Rooms[roomName].Remove(Users[Context.ConnectionId]);

            Clients.All.UserLeaveRoom(roomName, Users[Context.ConnectionId]);
        }

        public override Task OnConnected()
        {
            App.Messager.Send<string>("Client connected: " + Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            App.Messager.Send<string>("Client disconnected: " + Context.ConnectionId);

            Users.Remove(Context.ConnectionId);

            Clients.All.OnUserChanged(Users.Select(a => a.Value).ToList());

            return base.OnDisconnected(stopCalled);
        }
        public override async Task OnReconnected()
        {
            await base.OnReconnected();
        }
    }
}
