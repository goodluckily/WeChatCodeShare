using CodeShare.Model;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeShare.IService
{
    public interface ITokenService
    {
        Task<Token> GetTokenByType(WeiChatEnum weiChatEnum = WeiChatEnum.CodeShare, TokenEnum tokenEnum = TokenEnum.Token);

        Task<Token> CreateTokenAsync(Token token);

        Task DeleteAsync(ObjectId id);

        Task<IEnumerable<Token>> GetAllAsync();

        Task<Token> GetTokenAsync(ObjectId id);

        Task<Token> UpdateAsync(Token token);
    }
}
