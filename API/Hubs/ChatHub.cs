using DBL.DTOs;
using DBL.Identity;
using DBL.Interfaces;
using DBL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IRepository<ChatDTO, string> _chatRepository;
        private readonly IRepository<UserDTO, string> _userRepository;
        private readonly IRepository<MessageDTO, string> _messageRepository;

        public ChatHub(
            IRepository<ChatDTO, string> chatRepository,
            IRepository<UserDTO, string> userRepository,
            IRepository<MessageDTO, string> messageRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        [Authorize(RoleNames.USER)]
        public async Task Send(string userId, string chatId, string message)
        {
            try
            {
                var chat = await _chatRepository.GetItemAsync(new ChatDTO { Id = chatId }, false);

                if (chat == null)
                {
                    throw new ApplicationException($"Can't connect to chat. Chat {chatId} is not found.");
                }

                var user = chat.Users.FirstOrDefault(i => i.Id == userId);

                if (user == null)
                {
                    throw new ApplicationException($"User {userId} is not found");
                }

                _messageRepository.AddItemAsync(new MessageDTO { Content = message, SendDate = DateTime.Now, Chat = new ChatDTO { Id = chatId }, User = new UserDTO { Id = userId } });

                await _messageRepository.SaveChangesAsync();

                await Clients.Group(chatId).SendAsync("Receive", userId, message);
            }
            catch (Exception ex)
            {
                SendError(userId, ex.Message);
            }
        }

        [Authorize(RoleNames.USER)]
        public async Task AddToChat(string chatId, string userId)
        {
            try
            {
                var chat = await _chatRepository.GetItemAsync(new ChatDTO { Id = chatId }, false);

                if (chat == null)
                {
                    throw new ApplicationException($"Can't connect to chat. Chat {chatId} is not found.");
                }

                var user = chat.Users.FirstOrDefault(i => i.Id == userId);

                if (user == null)
                {
                    throw new ApplicationException($"User {userId} is not found");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            }
            catch (Exception ex)
            {
                SendError(userId, ex.Message);
            }

        }

        [Authorize(RoleNames.USER)]
        public async Task RemoveFromChat(string chatId, string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }

        [Authorize(RoleNames.USER)]
        private async Task SendError(string userId, string message)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await Clients.Group(userId).SendAsync("Receive", userId, message);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

        }
    }
}
