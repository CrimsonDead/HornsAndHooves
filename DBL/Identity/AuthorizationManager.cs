using DBL.DTOs;
using DBL.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace DBL.Identity
{
    public class AuthorizationManager<TUserDTO>
        where TUserDTO : UserDTO
    {
        public const string REFRESH_TOKEN = "Refresh";
        public string RefreshToken { get { return REFRESH_TOKEN; } }
        
        public const string ACCESS_TOKEN = "Access";
        public string AccessToken { get { return ACCESS_TOKEN; } }


        private readonly ApplicationContext _context;

        public AuthorizationManager(
            UserManager<User> userManager,
            ApplicationContext context)
        {
            UserManager = userManager;
            _context = context;
        }

        public UserManager<User> UserManager { get; }

        public async Task<string> GetTokenNameAsync(string token)
        {
            if (token == string.Empty)
                throw new ArgumentException("No Authorization data");

            var claim = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            return claim == null ? REFRESH_TOKEN : ACCESS_TOKEN;
        }

        public async Task<bool> CheckTokenAsync(string token)
        {
            if (token == string.Empty)
                throw new ArgumentException("No Authorization data");

            var tokenName = await GetTokenNameAsync(token);

            if (tokenName == REFRESH_TOKEN)
            {
                var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;

                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                DateTime endTime = DateTime.UnixEpoch.Add(TimeSpan.FromSeconds(
                    Convert.ToInt64(claims.FirstOrDefault(c => c.Type == "exp").Value))).ToLocalTime();
                
                var user = await UserManager.FindByIdAsync(userId);

                var dbToken = _context.UserTokens.FirstOrDefault(t => 
                    t.UserId == user.Id &&
                    t.LoginProvider == TokenConstants.LOGIN_PROVIDER &&
                    t.Name == REFRESH_TOKEN &&
                    t.Value == token &&
                    t.Valid);
                
                return dbToken != null && dbToken.EndDate > DateTime.Now;
            }
            else if (tokenName == ACCESS_TOKEN)
            {
                var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
                
                DateTime endTime = DateTime.UnixEpoch.Add(TimeSpan.FromSeconds(
                    Convert.ToInt64(claims.FirstOrDefault(c => c.Type == "exp").Value))).ToLocalTime();

                return endTime > DateTime.Now;

            }
            else
            {
                throw new Exception("Wrong token name");
            }

        }

        public async Task DenyTokenAsync(string token)
        {
            if (token == string.Empty)
                throw new ArgumentException("No Authorization data");

            var tokenName = await GetTokenNameAsync(token);

            if (tokenName == REFRESH_TOKEN)
            {
                var userName = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                var user = await UserManager.FindByNameAsync(userName);

                DisableToken(user, tokenName);
            }
            else
            {
                throw new Exception("Wrong token name");
            }
        }

        public async Task<string> GetTokenRoleAsync(string token)
        {
            if (token == string.Empty)
                throw new ArgumentException("No Authorization data");

            var tokenName = await GetTokenNameAsync(token);

            if (tokenName == ACCESS_TOKEN)
            {
                var userRole = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;

                return userRole == RoleNames.ADMIN
                    ? RoleNames.ADMIN
                    : userRole == RoleNames.USER
                        ? RoleNames.USER
                        : string.Empty;
            }
            else
            {
                throw new Exception("Wrong token name");
            }
        }

        public async Task<AuthorizationResult> SignoutAsync(string token)
        {
            if (token == string.Empty)
                throw new ArgumentException("No Authorization data");

            var tokenName = await GetTokenNameAsync(token);

            if (tokenName == REFRESH_TOKEN)
            {
                var userId = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

                var user = await UserManager.FindByIdAsync(userId);

                DisableToken(user, tokenName);

                return new AuthorizationResult
                {
                    Succeded = true,

                };
            }
            else
            {
                throw new Exception("Wrong token name");
            }
        }

        public async Task<AuthorizationResult> NoPasswordSignInAsync(string firstName , string middleName, string phoneNumber)
        {
            var dbUser = await UserManager.FindByNameAsync(phoneNumber);


            if (dbUser == null || dbUser.FirstName != firstName || dbUser.MiddleName != middleName)
                return new AuthorizationResult
                {
                    Succeded = false,
                    Error = "User not found",

                };
            else
            {
                var refreshJWT = await GenerateJWT(dbUser, REFRESH_TOKEN);
                var accessJWT = await GenerateJWT(dbUser, ACCESS_TOKEN);

                await RegisterToken(dbUser, refreshJWT);

                return new AuthorizationResult
                {
                    Succeded = true,
                    RefreshToken = refreshJWT,
                    AccessToken = accessJWT,

                };

            }
        }

        public async Task<AuthorizationResult> PasswordSignInAsync(string userName, string password)
        {
            var dbUser = await UserManager.FindByNameAsync(userName);

            if (dbUser == null)
                return new AuthorizationResult
                {
                    Succeded = false,
                    Error = "User not found",

                };

            if (await UserManager.CheckPasswordAsync(dbUser, password))
            {
                var refreshJWT = await GenerateJWT(dbUser, REFRESH_TOKEN);
                var accessJWT = await GenerateJWT(dbUser, ACCESS_TOKEN);

                await RegisterToken(dbUser, refreshJWT);

                return new AuthorizationResult
                {
                    Succeded = true,
                    RefreshToken = refreshJWT,
                    AccessToken = accessJWT,

                };

            }
            else
                return new AuthorizationResult
                {
                    Succeded = false,
                    Error = "Wrong password",

                };
        }

        private async Task RegisterToken(User user, string token)
        {
            var setTokenResult = await UserManager
                .SetAuthenticationTokenAsync(user, TokenConstants.LOGIN_PROVIDER, REFRESH_TOKEN, token);

            if (setTokenResult.Succeeded)
            {
                var dbToken = _context.UserTokens.FirstOrDefault(t =>
                    t.UserId == user.Id &&
                    t.LoginProvider == TokenConstants.LOGIN_PROVIDER &&
                    t.Name == REFRESH_TOKEN);

                dbToken.Valid = true;
                dbToken.EndDate = DateTime.UtcNow.Add(TimeSpan.FromMinutes(TokenConstants.REFRESH_LIFETIME));

                _context.UserTokens.Update(dbToken);
            }
            else
            {
                var error = FailedResultAsString(setTokenResult);

                throw new Exception(error);
            }
        }

        private async Task<string> GenerateJWT(User user, string tokenName)
        {
            ArgumentNullException.ThrowIfNull(user);

            List<Claim> claims;

            switch (tokenName)
            {
                case REFRESH_TOKEN:
                    claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),

                    };

                    break;
                case ACCESS_TOKEN:
                    claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Role, await GetUserRoleAsync(user.Id)),

                    };

                    break;
                default:
                    throw new Exception($"Wrong token type");
            }

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityTokenHandler()
                .WriteToken(new JwtSecurityToken(
                    issuer: TokenConstants.ISSUER,
                    audience: TokenConstants.AUDIENCE,
                    notBefore: now,
                    claims: claims,
                    expires: tokenName != REFRESH_TOKEN 
                        ? tokenName != ACCESS_TOKEN
                            ? now
                            : now.Add(TimeSpan.FromMinutes(TokenConstants.ACCESS_LIFETIME))
                        : now.Add(TimeSpan.FromMinutes(TokenConstants.REFRESH_LIFETIME)),
                    signingCredentials: new SigningCredentials(
                        TokenConstants.GetSymmetricSecurityKey(),
                        SecurityAlgorithms.HmacSha256))
                );

            return jwt;
        }

        public async Task<string> GetUserRoleAsync(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);

            ArgumentNullException.ThrowIfNull(user);

            var roles = (await UserManager.GetRolesAsync(user));

            if (roles.FirstOrDefault(r => r == RoleNames.ADMIN) != null)
            {
                return RoleNames.ADMIN;
            }

            if (roles.FirstOrDefault(r => r == RoleNames.USER) != null)
            {
                return RoleNames.USER;
            }

            throw new Exception("User has no roles");
        }

        private string FailedResultAsString(IdentityResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            string error = string.Empty;

            foreach (var item in result.Errors)
            {
                error += $"{item.Description}. \n";
            }

            return error;
        }

        public async Task<AuthorizationResult> RefreshTokenAsync(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                var claim = new JwtSecurityTokenHandler()
                    .ReadJwtToken(token).Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                var user = await UserManager.FindByIdAsync(claim.Value);

                DisableToken(user, REFRESH_TOKEN);

                var refreshJWT = await GenerateJWT(user, REFRESH_TOKEN);
                var accessJWT = await GenerateJWT(user, ACCESS_TOKEN);

                await RegisterToken(user, refreshJWT);

                return new AuthorizationResult
                {
                    Succeded = true,
                    RefreshToken = refreshJWT,
                    AccessToken = accessJWT,

                };
            }
            else
            {
                throw new ArgumentNullException();
            }

        }

        private void DisableToken(User user, string tokenName)
        {
            var dbToken = _context.UserTokens.FirstOrDefault(t =>
                t.UserId == user.Id &&
                t.LoginProvider == TokenConstants.LOGIN_PROVIDER &&
                t.Name == tokenName);

            dbToken.Valid = false;

            _context.UserTokens.Update(dbToken);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
