using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using DBL.Identity;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace API.Infrastructure.Authorization
{
    public class JWTAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public JWTAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            options.Value.AddPolicy(RoleNames.ADMIN, p =>
            {
                p.RequireRole(RoleNames.ADMIN);
            });

            options.Value.AddPolicy(RoleNames.USER, p =>
            {
                p.RequireRole(RoleNames.USER);
            });

            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }
        public Task<AuthorizationPolicy?> GetPolicyAsync (string policyName)
        {
            if (RoleNames.IsRoleName(policyName))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new RolesAuthorizationRequirement(
                    RoleNames.GetRoleListByPolicy(policyName)));
                return Task.FromResult(policy.Build());
            }
            else
            {
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return FallbackPolicyProvider.GetFallbackPolicyAsync();
        }
    }
}
