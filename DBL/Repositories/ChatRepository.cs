using DBL.DTOs;
using DBL.Extensions;
using DBL.Interfaces;
using DBL.Models;
using Microsoft.EntityFrameworkCore;

namespace DBL.Repositories
{
    public class ChatRepository : 
        IRepository<ChatDTO, string>,
        ISearchable<ChatDTO, string>
    {
        private readonly ApplicationContext _context;

        public ChatRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<ChatDTO> AddItemAsync(ChatDTO item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var chat = item.ToChat();

            chat.Id = Guid.NewGuid().ToString();

            await _context.Chats.AddAsync(chat);
            
            if (item.Users != null)
            {
                foreach (var user in item.Users)
                {
                    await _context.ChatUsers.AddAsync(new ChatUser { ChatId = chat.Id, UserId = user.Id });
                }
            }
            else
                throw new ApplicationException("Chat must has at least one user");

            return chat.ToDTO();
        }

        public async Task DeleteAsync(string id)
        {
            var item = await _context.Chats.FirstOrDefaultAsync(u => u.Id == id);

            _context.Chats.Remove(item);
        }

        public async Task<List<ChatDTO>> GetEntityByFilterAsync(string filter, int page = 0, int pageSize = 0)
        {
            if (page < 0 || pageSize < 0)
            {
                throw new ApplicationException("Page number and page size can not be equel or less then zero.", new ArgumentOutOfRangeException(nameof(page)));
            }

            var items = new List<ChatDTO>();

            foreach (var item in page != 0 && pageSize != 0 ?
                await _context.Chats
                    .Join(
                        _context.ChatUsers
                            .Join(
                                _context.Users.Where(i => i.FirstName.Contains(filter) || i.MiddleName.Contains(filter) || i.LastName.Contains(filter)),
                                cu => cu.UserId,
                                u => u.Id,
                                (cu, u) => new ChatUser { ChatId = cu.ChatId, UserId = cu.UserId }),
                        c => c.Id,
                        cu => cu.ChatId,
                        (c, cu) => new Chat { Id = c.Id })
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToListAsync() :
                await _context.Chats
                    .Join(
                        _context.ChatUsers
                            .Join(
                                _context.Users.Where(i => i.FirstName.Contains(filter) || i.MiddleName.Contains(filter) || i.LastName.Contains(filter)),
                                cu => cu.UserId,
                                u => u.Id,
                                (cu, u) => new ChatUser { ChatId = cu.ChatId, UserId = cu.UserId }),
                        c => c.Id,
                        cu => cu.ChatId,
                        (c, cu) => new Chat { Id = c.Id })
                    .ToListAsync())
            {
                items.Add(item.ToDTO(messages: _context.Messages.Where(i => i.ChatId == item.Id).ToList()));
            }

            return items;
        }

        public async Task<ChatDTO> GetItemAsync(ChatDTO item, bool throwIfNotFound = true)
        {
            var chat = await _context.Chats.FirstOrDefaultAsync(i => i.Id == item.Id);

            if (chat == null && throwIfNotFound)
                throw new ApplicationException($"Chat {item.Id} object is not found");

            var m = _context.Messages.Where(i => i.ChatId == chat.Id).ToList();

            return chat.ToDTO(
                _context.Users
                    .Join(
                        _context.ChatUsers.Where(i => i.ChatId == chat.Id),
                        u => u.Id,
                        cu => cu.UserId,
                        (u, cu) => new User 
                        { 
                            Id = u.Id, 
                            UserName = u.UserName,
                            FirstName = u.FirstName,
                            MiddleName = u.MiddleName,
                            LastName = u.LastName,
                            PhoneNumber = u.PhoneNumber,

                        })
                    .ToList(),
                _context.Messages.Where(i => i.ChatId == chat.Id).OrderBy(i => i.SendDate).ToList());
        }

        public async Task<IEnumerable<ChatDTO>> GetItemsAsync(int page = 0, int pageSize = 0)
        {
            if (page < 0 || pageSize < 0)
            {
                throw new ApplicationException("Page number and page size can not be equel or less then zero.", new ArgumentOutOfRangeException(nameof(page)));
            }

            var items = new List<ChatDTO>();

            foreach (var item in page != 0 && pageSize != 0 ?
                await _context.Chats
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToListAsync() :
                await _context.Chats.ToListAsync())
            {
                items.Add(item.ToDTO(messages: _context.Messages.Where(i => i.ChatId == item.Id).ToList()));
            }

            return items;
        }

        public int PageCount(int pageSize = 0)
        {
            return (int)Math.Round(_context.Chats.Count() / (float)pageSize, MidpointRounding.ToPositiveInfinity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<ChatDTO> UpdateAsync(ChatDTO item)
        {
            var updatedItem = await _context.Chats.FirstOrDefaultAsync(i => i.Id == item.Id);

            _context.Chats.Update(updatedItem);

            return item;
        }
    }
}
