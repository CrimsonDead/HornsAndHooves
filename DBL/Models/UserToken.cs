using Microsoft.AspNetCore.Identity;

namespace DBL.Models
{
    public class UserToken : IdentityUserToken<string>
    {
        public bool Valid { get; set; }
        public DateTime EndDate { get; set; }

    }
}
