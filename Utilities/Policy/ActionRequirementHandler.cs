using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities.Policy
{
    public class ActionRequirementHandler : AuthorizationHandler<ActionRequirement>
    {
        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ActionRequirement requirement)
        {
            var actionClaim = context.User.Claims.Where(x => x.Type == "Permission").ToList();

            if (!actionClaim.Any() || !actionClaim.Any(x => x.Value.Contains(requirement.Status)))
            {
                return Task.CompletedTask;
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
