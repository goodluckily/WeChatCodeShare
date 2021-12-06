using CodeShare.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CodeShare.Model
{
    public class Token : BaseEntity
    {
        public WeiChatEnum WeiChatType { get; set; }

        public TokenEnum TokenType { get; set; }

        public string Access_Token { get; set; }
        public double Expires_In { get; set; }
    }
}
