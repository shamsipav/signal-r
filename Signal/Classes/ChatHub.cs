using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Signal.Classes
{
    [Authorize]
    public class ChatHub: Hub
    {
        private readonly ChatDBContext _chatDBContext;
        public ChatHub(ChatDBContext chatDBContext)
        {
            _chatDBContext = chatDBContext;
        }

        public async Task Send(string message)
        {
            string userName = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            await Clients.All.SendAsync("Receive", message, userName);
        }

        public override async Task OnConnectedAsync()
        {
            string email = Context.User.FindFirst(ClaimTypes.Name).Value;
            User? user = _chatDBContext.Users.FirstOrDefault(u => u.Email == email);

            user.Online = true;
            _chatDBContext.Users.Update(user);
            _chatDBContext.SaveChanges();

            await Clients.All.SendAsync("Notify", $"{user.Name} вошел в чат");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string email = Context.User.FindFirst(ClaimTypes.Name).Value;
            User? user = _chatDBContext.Users.FirstOrDefault(u => u.Email == email);

            user.Online = false;
            _chatDBContext.Users.Update(user);
            _chatDBContext.SaveChanges();

            await Clients.All.SendAsync("Notify", $"{user.Name} покинул чат");
            await base.OnDisconnectedAsync(exception);
        }

        public List<User> GetUsersList()
        {
            return (List<User>)_chatDBContext.Users.Where(u => u.Online == true);
        }
    }
}
