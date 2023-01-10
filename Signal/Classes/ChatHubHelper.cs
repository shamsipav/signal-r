using Microsoft.AspNetCore.SignalR;

namespace Signal.Classes
{
    public class ChatHubHelper
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatHubHelper(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void SendHangfire(string message)
        {
            _hubContext.Clients.All.SendAsync("Hangfire", message);
        }
    }
}
