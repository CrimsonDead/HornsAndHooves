using DBL.DTOs;
using DBL.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace API.Infrastructure.Authorization
{
    public class JwtAuthorizationHandler : AuthorizationHandler
    {
        private readonly HttpContext _httpContext;
        private readonly AuthorizationManager<UserDTO> _authorizationManager;

        public JwtAuthorizationHandler(
            IHttpContextAccessor httpContextAccessor,
            AuthorizationManager<UserDTO> authorizationManager)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _authorizationManager = authorizationManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            try
            {
                var token = _httpContext.Request.Headers.Authorization;

                var tokenName = await _authorizationManager.GetTokenNameAsync(token);

                if (tokenName == _authorizationManager.AccessToken &&
                    requirement is RolesAuthorizationRequirement roleRequirement)
                {
                    var role = await _authorizationManager.GetTokenRoleAsync(token);

                    if (roleRequirement.AllowedRoles.Contains(role))
                    {
                        context.Succeed(roleRequirement);

                        return;
                    }
                    else
                    {
                        context.Fail(new AuthorizationFailureReason(
                            this, 
                            $"Role {role} has no enough permissions"));

                        return;
                    }
                }
                else if (tokenName == _authorizationManager.RefreshToken &&
                    requirement is DenyAnonymousAuthorizationRequirement authRequirement)
                {
                    var res = await _authorizationManager.CheckTokenAsync(token);

                    if (res)
                    {
                        context.Succeed(authRequirement);

                        return;
                    }
                    else
                    {
                        await _authorizationManager.DenyTokenAsync(token);

                        context.Fail(new AuthorizationFailureReason(
                            this,
                            "Not authorized"));

                        return;
                    }

                }


            }
            catch (ArgumentException)
            {
                context.Fail(new AuthorizationFailureReason(
                    this,
                    "Invalid token"));
            }
            
        }

    }
}
