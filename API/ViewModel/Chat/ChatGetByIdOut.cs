namespace API.ViewModel.Chat
{
    public class ChatGetByIdOut
    {
        public string? Id { get; set; }
        public List<ChatGetByIdOutUser>? Users { get; set; }
        public List<ChatGetByIdOutMessage>? Messages { get; set; }
    }
}
