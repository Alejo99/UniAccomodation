using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniAccomodation.Models;

namespace UniAccomodation.Infrastructure.Authorization
{
    public class IsAdvertOwnerAuthorizationHandler : AuthorizationHandler<IsAdvertOwnerRequirement, Advert>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdvertOwnerRequirement requirement, Advert resource)
        {
            if (context.User.HasClaim(claim => claim.Type.Equals("UniAccomodationId")))
            {
                var landlordId = context.User.Claims
                    .FirstOrDefault(c => c.Type == "UniAccomodationId").Value;
                if(resource.LandlordId.Equals(Int32.Parse(landlordId)))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.FromResult(0);
        }
    }
}
