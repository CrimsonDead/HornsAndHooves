using Microsoft.AspNetCore.Identity;

namespace DBL.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set;}
        public ICollection<ChatUser>? ChatUsers { get; set; }
        public ICollection<Message>? Messages { get; set; }
    }
}
