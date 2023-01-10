﻿using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Signal.Classes
{
    public class Message
    {
        public int Id { get; set; }

        public string Text { get; set; }
        public DateTime SendTime { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
