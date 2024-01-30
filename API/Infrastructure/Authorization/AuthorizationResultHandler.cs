using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Net;

namespace API.Infrastructure.Authorization
{
    public class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly IAuthorizationMiddlewareResultHandler _handler;
        public AuthorizationResultHandler()
        {
            _handler = new AuthorizationMiddlewareResultHandler();
        }

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Challenged)
            {
                if (policy.Requirements.Any(r =>
                    r is DenyAnonymousAuthorizationRequirement ||
                    r is RolesAuthorizationRequirement))
                {
                    NotAuthorize(context);
                }
            }
            else
                await _handler.HandleAsync(next, context, policy, authorizeResult);
        }

        private void NotAuthorize(HttpContext context, string message = "Not authorized")
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}
