using System;

namespace CodeShare.IService
{
    public interface ITokenService
    {
        string GetToken(Guid id);
    }
}
