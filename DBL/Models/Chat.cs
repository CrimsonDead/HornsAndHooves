namespace DBL.Models
{
    public class Chat
    {
        public string? Id { get; set; }
        public ICollection<ChatUser>? ChatUsers { get; set; }
        public ICollection<Message>? Messages { get; set; }

    }
}