namespace DBL.Models
{
    public class ChatUser
    {
        public string? UserId { get; set; }
        public string? ChatId { get; set; }
        public User? User { get; set; }
        public Chat? Chat { get; set; }
    }
}
