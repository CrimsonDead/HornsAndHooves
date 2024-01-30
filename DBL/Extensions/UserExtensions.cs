using DBL.DTOs;
using DBL.Models;

namespace DBL.Extensions
{
    public static class UserExtensions
    {
        public static UserDTO ToDTO(this User user, List<Chat> chats = null, List<Message> messages = null) => new UserDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Chats = new Func<List<ChatDTO>>(() =>
            {
                var list = new List<ChatDTO>();

                if (user.ChatUsers != null)
                    foreach (var item in user.ChatUsers)
                    {
                        list.Add(new ChatDTO
                        {
                            Id = item.Chat.Id,
                            Messages = item.Chat.Messages.Select(m => new MessageDTO { Id = m.Id, Content = m.Content, SendDate = m.SendDate, Chat = new ChatDTO { Id = item.ChatId } }).ToList(),
                        });
                    }

                return list;
            }).Invoke(),
            Messages = user.Messages.Select(i => new MessageDTO { Id = i.Id, Content = i.Content, SendDate = i.SendDate, Chat = new ChatDTO { Id = i.ChatId } }).ToList(),

        };
    }
}
