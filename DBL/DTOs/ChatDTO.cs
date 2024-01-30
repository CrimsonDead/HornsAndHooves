using DBL.Models;

namespace DBL.DTOs
{
    public class ChatDTO
    {
        public string? Id { get; set; }
        public List<UserDTO>? Users { get; set; }
        public List<MessageDTO>? Messages { get; set; }
    }
}
