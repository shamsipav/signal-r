using Microsoft.EntityFrameworkCore;

namespace Signal.Classes
{
    public class ChatDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ChatDBContext(DbContextOptions<ChatDBContext> options) : base(options)
        {

        }
    }
}
