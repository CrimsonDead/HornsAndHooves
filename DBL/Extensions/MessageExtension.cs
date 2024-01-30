using DBL.DTOs;
using DBL.Models;

namespace DBL.Extensions
{
    public static class MessageExtension
    {
        public static MessageDTO ToDTO(this Message message) => new MessageDTO
        {
            Id = message.Id,
            Content = message.Content,
            SendDate = message.SendDate,
            Chat = new ChatDTO
            {
                Id = message.ChatId,

            },
            User = new UserDTO 
            { 
                Id = message.User?.Id,
                UserName = message.User?.UserName,
                FirstName = message.User?.FirstName,
                MiddleName = message.User?.LastName,
                LastName = message.User?.LastName,
                PhoneNumber = message.User?.PhoneNumber,
                
            },
        };
    }
}
