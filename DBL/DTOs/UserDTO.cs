using DBL.Models;

namespace DBL.DTOs
{
    public class UserDTO
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public List<ChatDTO>? Chats { get; set; }
        public List<MessageDTO>? Messages { get; set; }

    }
}
