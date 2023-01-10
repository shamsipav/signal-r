namespace Signal.Classes
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }

        public bool Online { get; set; } = false;
    }
}
