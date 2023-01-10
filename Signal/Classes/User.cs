
using Newtonsoft.Json;

namespace Signal.Classes
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public bool Online { get; set; } = false;

        [JsonIgnore]
        public List<Message>? Messages { get; set; }
    }
}
