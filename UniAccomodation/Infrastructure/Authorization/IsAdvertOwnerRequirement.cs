using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniAccomodation.Infrastructure.Authorization
{
    public class IsAdvertOwnerRequirement : IAuthorizationRequirement
    {
    }
}
