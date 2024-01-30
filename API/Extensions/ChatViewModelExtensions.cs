using API.ViewModel.Chat;
using DBL.DTOs;

namespace API.Extensions
{
    public static class ChatViewModelExtensions
    {
        public static ChatOnlyFields ToOnlyFieldsObject(this ChatDTO dto) => new ChatOnlyFields
        {
            Id = dto.Id,

        };

        public static ChatDTO ToDTO(this ChatOnlyFields chat) => new ChatDTO
        {
            Id = chat.Id,

        };

        public static ChatGetByIdOut ToChatGetByIdOut(this ChatDTO dto) => new ChatGetByIdOut
        {
            Id = dto.Id,
            Users = new Func<List<ChatGetByIdOutUser>>(() =>
            {
                var list = new List<ChatGetByIdOutUser>();

                if (dto.Users != null)
                    foreach (var item in dto.Users)
                    {
                        list.Add(new ChatGetByIdOutUser
                        {
                            Id = item.Id,
                            UserName = item.UserName,
                            FirstName = item.FirstName,
                            MiddleName = item.MiddleName,
                            LastName = item.LastName,
                            PhoneNumber = item.PhoneNumber,

                        });
                    }

                return list;
            }).Invoke(),
            Messages = new Func<List<ChatGetByIdOutMessage>>(() =>
            {
                var list = new List<ChatGetByIdOutMessage>();

                if (dto.Messages != null)
                    foreach (var item in dto.Messages)
                    {
                        list.Add(new ChatGetByIdOutMessage
                        {
                            Id = item.Id,
                            Content = item.Content,
                            SendDate = item.SendDate,
                            UsreId = item.User.Id,

                        });
                    }

                return list;
            }).Invoke(),
        };

        public static ChatGetListOut ToChatGetListOut(this ChatDTO dto) => new ChatGetListOut
        {
            Id = dto.Id,
            LastMessage = dto.Messages.OrderByDescending(i => i.SendDate).FirstOrDefault()?.Content,

        };
    }
}
