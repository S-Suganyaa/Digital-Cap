using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace DigitalCap.Persistence.Repositories
{
    public class PlatformUserRepository :  IPlatformUserRepository
    {
        private readonly IHttpContextAccessor _httpContext;
        
    }
}
