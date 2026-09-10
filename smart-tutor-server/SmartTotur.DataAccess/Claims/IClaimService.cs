using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.DataAccess.Claims
{
    public interface IClaimService
    {
        int? GetUserId();
        string? GetClaim(string key);
        string? GetIpAddress();
        string? GetUserAgent();
    }
}
