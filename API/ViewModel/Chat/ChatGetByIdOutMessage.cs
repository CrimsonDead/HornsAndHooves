namespace API.ViewModel.Chat
{
    public class ChatGetByIdOutMessage
    {
        public string? Id { get; set; }
        public string? Content { get; set; }
        public DateTime SendDate { get; set; }
        public string UsreId { get; set; }

    }
}