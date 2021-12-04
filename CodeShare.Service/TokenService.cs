using CodeShare.IService;
using CodeShare.Model;
using CodeShare.MongoDBRepository;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeShare.Service
{
    public class TokenService : ITokenService
    {
        public async Task<Token> CreateTokenAsync(Token token)
        {
            return await MongoDBHelper<Token>.AddModelAsync(token);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await MongoDBHelper<Token>.DeleteManyAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Token>> GetAllAsync()
        {
            return await MongoDBHelper<Token>.GetAllAsync();
        }

        public async Task<Token> GetTokenAsync(ObjectId id)
        {
            return await MongoDBHelper<Token>.GetAsync(x => x.Id == id);
        }

        public async Task<Token> UpdateAsync(Token token)
        {
            await MongoDBHelper<Token>.UpdateAsync(token, token.Id);
            return token;
        }

        public async Task<Token> GetTokenByType(WeiChatEnum weiChatEnum, TokenEnum tokenEnum)
        {
            return await MongoDBHelper<Token>.GetAsync(x => x.WeiChatType == weiChatEnum && x.TokenType == tokenEnum);
        }
    }
}
