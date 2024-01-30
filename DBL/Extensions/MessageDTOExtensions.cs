using DBL.DTOs;
using DBL.Models;

namespace DBL.Extensions
{
    public static class MessageDTOExtensions
    {
        public static Message ToMessage(this MessageDTO dto) => new Message
        {
            Id = dto.Id,
            Content = dto.Content,
            SendDate = dto.SendDate,
            ChatId = dto.Chat.Id,
            UserId = dto.User.Id,

        };
    }
}
