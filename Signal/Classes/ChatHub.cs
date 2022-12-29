using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Signal.Classes
{
    public class ChatHub: Hub
    {
        static List<User> Users = new List<User>();

        public async Task Send(string message, string userName)
        {
            await Clients.All.SendAsync("Receive", message, userName);
        }

        public override async Task OnConnectedAsync()
        {
            string id = Context.ConnectionId;

            if (!Users.Any(x => x.ConnectionId == id))
            {
                Users.Add(new User { ConnectionId = id });
            }

            await Clients.All.SendAsync("Notify", $"{Context.ConnectionId} вошел в чат", Users);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (user != null)
            {
                Users.Remove(user);
            }

            await Clients.All.SendAsync("Notify", $"{Context.ConnectionId} покинул чат", Users);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
