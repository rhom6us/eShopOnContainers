using System;
using System.Collections.Generic;
using System.Linq;

namespace eShopOnContainers.Core.Models.Token
{
    public class FacebookAccessToken
    {
        public FacebookAccessToken() { }
        public string ApplicationId { get; set; }
        public IReadOnlyCollection<char> DeclinedPermissions { get; set; }
        public DateTimeOffset Expires { get; set; }
        public bool IsExpired { get; set; }
        public DateTimeOffset LastRefresh { get; set; }
        public IReadOnlyCollection<string> Permissions { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }

        public static implicit operator string(FacebookAccessToken source) {
            return source?.Token;
        }

    
    }
}