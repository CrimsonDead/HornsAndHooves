using DBL.DTOs;
using DBL.Models;

namespace DBL.Extensions
{
    public static class ChatDTOExtensions
    {
        public static Chat ToChat(this ChatDTO dto) => new Chat
        {
            Id = dto.Id,

        };

    }
}
