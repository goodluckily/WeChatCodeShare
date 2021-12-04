using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CodeShare.Model
{
    public enum WeiChatEnum 
    {
        [EnumMember(Value = "CodeShare")]
        CodeShare = 1,
    }
    public enum TokenEnum
    {
        [EnumMember(Value = "JSToken")]
        Token = 1,
        [EnumMember(Value = "JSToken")]
        JsToken = 2,
    }
}
