using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Security.Claims;
using AutoMapper.QueryableExtensions;

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

        static MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateProjection<User, UserDTO>());

        public async Task Send(string message)
        {
            string authorEmail = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Пример использования AutoMapper - на клиент передаётся объект без пароля
            var author = _chatDBContext.Users.AsNoTracking().ProjectTo<UserDTO>(config).FirstOrDefault(u => u.Email == authorEmail);
            DateTime sendTime = DateTime.Now;

            Message sendMessage = new Message { Text = message, SendTime = sendTime, UserId = author.Id };
            _chatDBContext.Messages.Add(sendMessage);
            _chatDBContext.SaveChanges();

            await Clients.All.SendAsync("Receive", message, author, sendTime);
        }

        public async Task SendToUser(string message, int sendToId)
        {
            string authorEmail = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var author = _chatDBContext.Users.AsNoTracking().ProjectTo<UserDTO>(config).FirstOrDefault(u => u.Email == authorEmail);

            User user = _chatDBContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == sendToId);
            string emailSendTo = user.Email;

            DateTime sendTime = DateTime.Now;

            // Показываем сообщение двум пользователям (автору и получателю)
            await Clients.User(emailSendTo).SendAsync("MessageTo", message, author, sendTime);
            await Clients.User(authorEmail).SendAsync("MessageTo", message, author, sendTime);
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
            List<Message> messagesList = _chatDBContext.Messages.Include(u => u.User).AsNoTracking().ToList();

            var serializerSettings = new JsonSerializerSettings{ ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var messages = JsonConvert.SerializeObject(messagesList, serializerSettings);

            await Clients.User(email).SendAsync("Connect", user.Id, user.Name, messages);

            // Обновляем список пользователей онлайн
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
