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
            string userName = Context.User.FindFirst(ClaimTypes.Name).Value;
            DateTime sendTime = DateTime.Now;

            /* TODO: Change userName to UserID */
            Message sendMessage = new Message { Text = message, SendTime = sendTime, UserName = userName };
            _chatDBContext.Messages.Update(sendMessage);
            _chatDBContext.SaveChanges();

            await Clients.All.SendAsync("Receive", message, userName, sendTime);
        }

        public async Task SendTest(string message)
        {
            await Clients.All.SendAsync("Receive", message);
        }

        public override async Task OnConnectedAsync()
        {
            string email = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            User? user = _chatDBContext.Users.FirstOrDefault(u => u.Email == email);

            user.Online = true;
            _chatDBContext.Users.Update(user);
            _chatDBContext.SaveChanges();

            // Список всех пользователей онлайн
            List<User> onlineUsers = _chatDBContext.Users
                .Where(u => u.Online)
                .ToList();
            
            // Отправляем вошедшему клиенту список всех сообщений в общем чате
            List<Message> messages = _chatDBContext.Messages.ToList();
            await Clients.User(Context.User.FindFirst(ClaimTypes.NameIdentifier).Value).SendAsync("Messages", messages);

            await Clients.All.SendAsync("Notify", $"{user.Name} вошел в чат", onlineUsers);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string email = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            User? user = _chatDBContext.Users.FirstOrDefault(u => u.Email == email);

            user.Online = false;
            _chatDBContext.Users.Update(user);
            _chatDBContext.SaveChanges();

            // Список всех пользователей онлайн
            List<User> onlineUsers = _chatDBContext.Users
                .Where(u => u.Online)
                .ToList();

            await Clients.All.SendAsync("Notify", $"{user.Name} покинул чат", onlineUsers);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
