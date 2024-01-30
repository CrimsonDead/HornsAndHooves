using DBL.DTOs;
using DBL.Extensions;
using DBL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DBL.Repositories
{
    public class MessageRepository : IRepository<MessageDTO, string>
    {
        private readonly ApplicationContext _context;

        public MessageRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<MessageDTO> AddItemAsync(MessageDTO item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var message = item.ToMessage();

            message.Id = Guid.NewGuid().ToString();

            await _context.AddAsync(message);

            return message.ToDTO();
        }

        public async Task DeleteAsync(string id)
        {
            var item = await _context.Messages.FirstOrDefaultAsync(i => i.Id == id);

            _context.Messages.Remove(item);
        }

        public async Task<MessageDTO> GetItemAsync(MessageDTO item, bool throwIfNotFound = true)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(i => i.Id == item.Id);

            message.User = await _context.Users.FirstOrDefaultAsync(i => i.Id == message.UserId);

            if ((message == null || message.User == null) && throwIfNotFound)
                throw new ApplicationException("Message object is not found.");

            return message.ToDTO();
        }

        public async Task<IEnumerable<MessageDTO>> GetItemsAsync(int page = 0, int pageSize = 0)
        {
            if (page < 0 || pageSize < 0)
            {
                throw new ApplicationException("Page number and page size can not be equel or less then zero.", new ArgumentOutOfRangeException(nameof(page)));
            }

            var items = new List<MessageDTO>();

            foreach (var item in page == 0 && pageSize == 0 ?
                await _context.Messages
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToListAsync() :
                await _context.Messages.ToListAsync())
            {
                items.Add(item.ToDTO());
            }

            return items;
        }

        public int PageCount(int pageSize = 0)
        {
            return (int)Math.Round(_context.Messages.Count() / (float)pageSize, MidpointRounding.ToPositiveInfinity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<MessageDTO> UpdateAsync(MessageDTO item)
        {
            var updatedItem = await _context.Messages.FirstOrDefaultAsync(i => i.Id == item.Id);

            updatedItem.Content = item.Content;
            updatedItem.SendDate = DateTime.Now;

            _context.Messages.Update(updatedItem);

            return item;
        }
    }
}
