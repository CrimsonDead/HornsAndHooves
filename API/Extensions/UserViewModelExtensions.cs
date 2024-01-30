using API.ViewModel.User;
using DBL.DTOs;
using System;

namespace API.Extensions
{
    public static class UserViewModelExtensions
    {
        public static UserDTO ToDTO(this UserCreateAdminIn user) => new UserDTO
        {
            UserName = user.Username,
            Password = user.Password,

        };

        public static UserDTO ToDTO(this UserCreateUserIn user) => new UserDTO
        {
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            UserName = user.Phone,
            PhoneNumber = user.Phone,

        };

        public static UserOnlyFields ToOnlyFieldsObject(this UserDTO dto) => new UserOnlyFields
        {
            Id = dto.Id,
            UserName = dto.UserName,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            Phone = dto.PhoneNumber,
            
        };

        public static UserDTO ToDTO(this UserOnlyFields user) => new UserDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            PhoneNumber = user.Phone,

        };

        public static UserGetUserByIdOut ToUserGetByIdOut(this UserDTO dto) => new UserGetUserByIdOut
        {
            Id = dto.Id,
            UserName = dto?.UserName,
            FirstName = dto?.FirstName,
            MiddleName = dto?.MiddleName,
            LastName = dto?.LastName,
            Phone = dto?.PhoneNumber,
            Chats = dto.Chats
                .Select(i => new UserGetUserByIdOutChat 
                { 
                    Id = i.Id, 
                    LastMessage = i.Messages
                        .Where(ii => ii.Chat.Id == i.Id)
                        .OrderByDescending(ii => ii.SendDate)
                        .FirstOrDefault().Content 
                })
                .ToList(),
            
        };

        public static UserCreateUserOut ToUserCreateUserOut(this UserDTO dto) => new UserCreateUserOut
        {
            Id = dto.Id,
            UserName = dto.UserName,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            Phone = dto.PhoneNumber,
            Chat = dto.Chats.FirstOrDefault().Id,

        };
    }
}
