using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace SmartTutor.DataAccess.Claims
{
    public class ClaimService : IClaimService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetUserId()
        {
            var idString = GetClaim(ClaimTypes.NameIdentifier);
            if (int.TryParse(idString, out var userId))
            {
                return userId;
            }
            return null;
        }

        public string? GetClaim(string key)
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(key)?.Value;
        }

        public string? GetIpAddress()
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (_httpContextAccessor.HttpContext?.Request.Headers.ContainsKey("X-Forwarded-For") == true)
            {
                ip = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            }
            return ip;
        }

        public string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
        }


    }
}
