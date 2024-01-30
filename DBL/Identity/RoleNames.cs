namespace DBL.Identity
{
    public class RoleNames
    {
        public const string ADMIN = "Admin";
        public const string USER = "User";

        public static bool IsRoleName(string roleName)
        {
            return roleName == ADMIN ||
                roleName == USER;
        }

        public static List<string> GetRoleListByPolicy(string policy)
        {
            if (policy == USER)
            {
                return new List<string> { USER, ADMIN };
            }
            else if (policy == ADMIN)
            {
                return new List<string> { ADMIN };
            }
            else
            {
                throw new ArgumentException("Wrong policy name", policy);
            }

        }


    }
}
