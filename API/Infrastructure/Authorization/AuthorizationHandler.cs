using Microsoft.AspNetCore.Authorization;

namespace API.Infrastructure.Authorization
{
    public abstract class AuthorizationHandler : IAuthorizationHandler
    {
        public virtual async Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var item in context.Requirements)
            {
                await HandleRequirementAsync(context, item);
                
                if (context.HasFailed)
                {
                    break;
                }    
            }
        }

        protected abstract Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement);
    }
}