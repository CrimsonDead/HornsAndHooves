using DBL.DTOs;
using DBL.Extensions;
using DBL.Identity;
using DBL.Interfaces;
using DBL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace DBL.Repositories
{
    public class UserRepository : IRepository<UserDTO, string>
    {
        private readonly ApplicationContext _context;

        public UserRepository(
            UserManager<User> userManager,
            ApplicationContext context)
        {
            ArgumentNullException.ThrowIfNull(userManager);

            UserManager = userManager;
            _context = context;
        }

        public UserManager<User> UserManager { get; }

        public async Task<UserDTO> AddItemAsync(UserDTO item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var user = item.ToUser();

            user.Id = Guid.NewGuid().ToString();

            IdentityResult registrationResult;
            IdentityResult roleAssigningResult;

            if (string.IsNullOrEmpty(item.Password))
            {
                registrationResult = await UserManager.CreateAsync(user);

                if (registrationResult.Succeeded)
                {
                    roleAssigningResult = await UserManager.AddToRoleAsync(user, RoleNames.USER);
                }
                else
                    throw new ApplicationException(FailedIdentityResultAsString(registrationResult));
            }
            else
            {
                registrationResult = await UserManager.CreateAsync(user, item.Password);

                if (registrationResult.Succeeded)
                {
                    roleAssigningResult = await UserManager.AddToRoleAsync(user, RoleNames.ADMIN);
                }
                else
                    throw new ApplicationException(FailedIdentityResultAsString(registrationResult));
            }

            if (roleAssigningResult.Succeeded)
                return await GetItemAsync(new UserDTO { FirstName = user.FirstName, MiddleName = user.MiddleName, LastName = user.LastName, PhoneNumber = user.PhoneNumber });
            else
                throw new ApplicationException(FailedIdentityResultAsString(roleAssigningResult));
        }

        public async Task DeleteAsync(string id)
        {
            var item = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            _context.Users.Remove(item);
        }

        public async Task<UserDTO> GetItemAsync(UserDTO item, bool throwIfNotFound = true)
        {
            User? dbUser;

            if (!string.IsNullOrEmpty(item.Id))
                dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == item.Id);
            else
                dbUser = await _context.Users.FirstOrDefaultAsync(u => u.FirstName == item.FirstName && u.MiddleName == item.MiddleName && u.PhoneNumber == item.PhoneNumber);

            if (dbUser == null)
            {
                if (throwIfNotFound)
                    throw new ApplicationException("User is not found");
            }
            else
            {
                dbUser.ChatUsers = _context.ChatUsers
                    .Where(i => i.UserId == dbUser.Id)
                    .Select(i => new ChatUser
                    {
                        ChatId = i.ChatId,
                        Chat = new Chat { Id = i.ChatId, Messages = _context.Messages.Where(ii => ii.ChatId == i.ChatId).ToList() },
                    })
                    .ToList();

                dbUser.Messages = _context.Messages.Where(i => i.UserId == dbUser.Id).OrderBy(i => i.SendDate).ToList();
            }

            return dbUser?.ToDTO();
        }

        public async Task<IEnumerable<UserDTO>> GetItemsAsync(int page = 0, int pageSize = 0)
        {
            if (page < 0 || pageSize < 0)
            {
                throw new ApplicationException("Page number and page size can not be equel or less then zero.", new ArgumentOutOfRangeException(nameof(page)));
            }

            var items = new List<UserDTO>();

            foreach (var item in page == 0 && pageSize == 0 ?
                await _context.Users
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToListAsync() :
                await _context.Users.ToListAsync())
            {
                items.Add(item.ToDTO());
            }

            return items;
        }

        public int PageCount(int pageSize = 0)
        {
            return (int)Math.Round(_context.Users.Count() / (float)pageSize, MidpointRounding.ToPositiveInfinity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<UserDTO> UpdateAsync(UserDTO item)
        {
            var updatedItem = await _context.Users.FirstOrDefaultAsync(u => u.Id == item.Id);

            updatedItem.UserName = item.UserName;
            updatedItem.NormalizedUserName = item.UserName?.ToUpper();
            updatedItem.FirstName = item.FirstName;
            updatedItem.MiddleName = item.MiddleName;
            updatedItem.LastName = item.LastName;
            updatedItem.PhoneNumber = item.PhoneNumber;

            _context.Users.Update(updatedItem);

            return item;
        }

        private string FailedIdentityResultAsString(IdentityResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            string error = string.Empty;

            foreach (var item in result.Errors)
            {
                error += $"{item.Description}. \n";
            }

            return error;
        }

        public async Task InitDefaultRolesAsync()
        {
            if (await _context.Roles.FirstOrDefaultAsync(i => i.Name != RoleNames.ADMIN) == null)
            {
                await _context.Roles.AddAsync(new Role
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = RoleNames.ADMIN,
                    NormalizedName = RoleNames.ADMIN.ToUpper(),
                });
            }
            else
                throw new ApplicationException("Roles has been initialized.");
            
            if (await _context.Roles.FirstOrDefaultAsync(i => i.Name != RoleNames.USER) == null)
            {
                await _context.Roles.AddAsync(new Role
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = RoleNames.USER,
                    NormalizedName = RoleNames.USER.ToUpper(),
                });
            }
            else
                throw new ApplicationException("Roles has been initialized.");
        }

        public async Task<UserDTO> GetAdminAsync()
        {
            var dbUser = await _context.Users
                .Join(
                    _context.UserRoles
                        .Join(
                            _context.Roles.Where(i => i.Name == RoleNames.ADMIN),
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => new { UserId = ur.UserId, RoleId = ur.RoleId }),
                    u => u.Id,
                    ur => ur.UserId,
                    (u, ur) => new User { Id = u.Id, UserName = u.UserName, FirstName = u.FirstName, MiddleName = u.MiddleName, LastName = u.LastName, PhoneNumber = u.PhoneNumber })
                .FirstOrDefaultAsync();

            if (dbUser == null)
                throw new ApplicationException("User is not found");

            dbUser.ChatUsers = _context.ChatUsers
                .Where(i => i.UserId == dbUser.Id)
                .Select(i => new ChatUser 
                {
                    ChatId = i.ChatId,
                    Chat = new Chat { Id = i.ChatId, Messages = _context.Messages.Where(ii => ii.ChatId == i.ChatId).ToList() },
                })
                .ToList();

            dbUser.Messages = _context.Messages.Where(i => i.UserId == dbUser.Id).OrderBy(i => i.SendDate).ToList();

            return dbUser?.ToDTO();
        }
    }
}
