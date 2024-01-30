using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DBL.Identity
{
    public class TokenConstants
    {
        public const string LOGIN_PROVIDER = "TailWorldAuth";
        public const string ISSUER = "TailWorldAuth";
        public const string AUDIENCE = "TailWorldClient";
        const string KEY = "IZMA333Incorporated";
        public const int REFRESH_LIFETIME = 333;
        public const int ACCESS_LIFETIME = 4320;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }

    }
}
