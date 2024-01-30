namespace API.ViewModel.Message
{
    public class MessageGetByIdOut
    {
        public string? Id { get; set; }
        public string? Message { get; set; }
        public DateTime SendDate { get; set; }
        public string? UserId { get; set; }
        public string? ChatId { get; set; }

    }
}
