namespace API.ViewModel.Message
{
    public class MessageGetListOut
    {
        public string? Id { get; set; }
        public string? Message { get; set; }
        public DateTime SendDate { get; set; }
        public string? UserId { get; set; }
    }
}
