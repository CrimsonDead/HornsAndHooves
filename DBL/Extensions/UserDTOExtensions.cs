using DBL.DTOs;
using DBL.Models;

namespace DBL.Extensions
{
    public static class UserDTOExtensions
    {
        public static User ToUser(this UserDTO dto) => new User
        {
            Id = dto.Id,
            UserName = dto.UserName,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,

        };

    }
}
