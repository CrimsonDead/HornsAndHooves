using API.ViewModel.Message;
using DBL.DTOs;

namespace API.Extensions
{
    public static class MessageViewModelExtensions
    {
        public static MessageDTO ToDTO(this MessageCreateIn message) => new MessageDTO
        {
            Content = message.Message,
            SendDate = DateTime.Now,
            Chat = new ChatDTO { Id = message.ChatId },
            User = new UserDTO { Id = message.UserId },

        };

        public static MessageGetByIdOut ToMessageGetByIdOut(this MessageDTO dto) => new MessageGetByIdOut
        {
            Id = dto.Id,
            Message = dto.Content,
            SendDate = dto.SendDate,
            ChatId = dto.Chat?.Id,
            UserId = dto.User?.Id,

        };

        public static MessageGetListOut ToMessageGetListOut(this MessageDTO dto) => new MessageGetListOut
        {
            Id = dto.Id,
            Message = dto.Content,
            SendDate = dto.SendDate,
            UserId = dto.User?.Id,

        };

        public static MessageDTO ToDTO(this MessageOnlyFields mssage) => new MessageDTO
        {
            Id = mssage.Id,
            Content = mssage.Content,
            SendDate = mssage.SendDate,

        };
    }
}
