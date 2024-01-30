namespace DBL.Models
{
    public class Message
    {
        public string? Id { get; set; }
        public string? Content { get; set; }
        public DateTime SendDate { get; set; }
        public string? ChatId { get; set; }
        public string? UserId { get; set; }
        public Chat? Chat { get; set; }
        public User? User { get; set; }
    }
}
