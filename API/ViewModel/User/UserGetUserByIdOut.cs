using DBL.DTOs;

namespace API.ViewModel.User
{
    public class UserGetUserByIdOut
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public List<UserGetUserByIdOutChat>? Chats { get; set; }
    }
}
