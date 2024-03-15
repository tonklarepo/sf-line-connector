using System;

namespace Auth
{
    public interface IJwtHandler
    {
        JsonWebToken Create(string username, string displayName, bool is_contractor, bool is_admin, bool is_pis, string refreshToken = null);
    }
}