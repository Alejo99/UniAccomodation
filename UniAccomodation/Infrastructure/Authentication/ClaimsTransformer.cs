using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using UniAccomodation.Data;
using UniAccomodation.Models;

namespace UniAccomodation.Infrastructure.Authentication
{
    /// <summary>
    /// Adds some claims to the Identity
    /// </summary>
    public class ClaimsTransformer : IClaimsTransformation
    {
        private UniAccomodationDbContext context;

        public ClaimsTransformer(UniAccomodationDbContext dbContext)
        {
            context = dbContext;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // Get identity to get the id
            var identity = ((ClaimsIdentity)principal.Identity);
            var userId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            // Search user by id
            ApplicationUser currentUser = context.Users.FirstOrDefault(usr => usr.Id == userId);

            // Create new ClaimsIdentity, using the existing identity
            // As it is an async call, it may be called more than once, we don't want to add claims twice
            var ci = new ClaimsIdentity(identity.Claims, identity.AuthenticationType, identity.NameClaimType, identity.RoleClaimType);
            // Add our application Id
            ci.AddClaim(new Claim("UniAccomodationId", currentUser.ApplicationId.ToString()));

            // Create  and return new principal
            return Task.FromResult(new ClaimsPrincipal(ci));
        }
    }
}
