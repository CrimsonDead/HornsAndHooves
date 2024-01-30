using DBL.DTOs;
using DBL.Models;

namespace DBL.Extensions
{
    public static class ChatExtensions
    {
        public static ChatDTO ToDTO(this Chat chat, List<User> users = null, List<Message> messages = null) => new ChatDTO
        {
            Id = chat.Id,
            Users = new Func<List<UserDTO>>(() =>
            {
                var list = new List<UserDTO>();

                if (users != null)
                    foreach (var item in users)
                    {
                        if (item != null)
                            list.Add(new UserDTO
                            {
                                Id = item.Id,
                                UserName = item.UserName,
                                FirstName = item.FirstName,
                                MiddleName = item.LastName,
                                LastName = item.LastName,
                                PhoneNumber = item .PhoneNumber,
                            });
                    }

                return list;
            }).Invoke(),
            Messages = new Func<List<MessageDTO>>(() =>
            {
                var list = new List<MessageDTO>();

                if (messages != null)
                    foreach (var item in messages.OrderBy(i => i.SendDate))
                    {
                        if (item != null)
                            list.Add(new MessageDTO
                            {
                                Id = item.Id,
                                Content = item.Content,
                                SendDate = item.SendDate,
                                User = new UserDTO { Id = item.UserId }
                            });
                    }

                return list;
            }).Invoke(),

        };
    }
}
