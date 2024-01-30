using DBL.Models;

namespace DBL.DTOs
{
    public class MessageDTO
    {
        public string? Id { get; set; }
        public string? Content { get; set; }
        public DateTime SendDate { get; set; }
        public ChatDTO? Chat { get; set; }
        public UserDTO? User { get; set; }
    }
}
