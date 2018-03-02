using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UserActions.Api.Services
{
    public class FacebookReader
    {
        private readonly IAccessTokenRepository _accessTokenRepository;
        private readonly ILastAccessCache _lastAccessCache;
        public FacebookReader(IAccessTokenRepository accessTokenRepository, ILastAccessCache lastAccessCache) {
            _accessTokenRepository = accessTokenRepository;
            _lastAccessCache = lastAccessCache;
        }


        public async Task CheckPosts(string facebookId, DateTimeOffset triggerTime) {
            var lastTime = _lastAccessCache.GetLastTime(facebookId, "user_post");
            if (triggerTime <= lastTime)
                return;
            var token = _accessTokenRepository.GetAccessTokenAsync("facebook", applicationId, facebookId);
            var query = $"access_token={token}&fields=posts.limit(99999).with(location).since({lastTime.ToUnixTimeSeconds()}){{id,created_time,updated_time,privacy,place,message}}";

            using (var client = new HttpClient()) {
                client.BaseAddress = new Uri("https://graph.facebook.com/v2.12");
                var json = await client.GetStringAsync($"me?{query}");
                var data = JsonConvert.DeserializeObject<Rootobject>(json);

                var model = data.posts.data.Select(p => new {
                    Provider = "facebook",
                    SourceId = data.id,
                    TargetId = p.place.id,
                    EventType = "user_post",
                    DateTime = p.created_time,
                    Scope = p.privacy.value,
                });

                using (var transaction = something) {
                    _integrationService.Publish(model);
                    _lastAccessCache.SetLastAccess(facebookId, "user_post", data.posts.data.Max(p => p.updated_time));
                    transaction.Commit();
                }
            }

        }

        public interface ILastAccessCache
        {
            DateTimeOffset GetLastTime(string facebookId, string eventType);
            void SetLastAccess(string facebookId, string userPost, DateTime max);
        }



        public class Rootobject
        {
            public Posts posts { get; set; }
            public string id { get; set; }
        }

        public class Posts
        {
            public Datum[] data { get; set; }
            public Paging paging { get; set; }
        }

        public class Paging
        {
            public string next { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public DateTime created_time { get; set; }
            public DateTime updated_time { get; set; }
            public Privacy privacy { get; set; }
            public Place place { get; set; }
            public string message { get; set; }
        }

        public class Privacy
        {
            public string value { get; set; }
            public string description { get; set; }
            public string friends { get; set; }
            public string allow { get; set; }
            public string deny { get; set; }
        }

        public class Place
        {
            public string id { get; set; }
            public string name { get; set; }
            public Location location { get; set; }
        }

        public class Location
        {
            public string city { get; set; }
            public string country { get; set; }
            public float latitude { get; set; }
            public float longitude { get; set; }
            public string state { get; set; }
            public string street { get; set; }
            public string zip { get; set; }
            public string located_in { get; set; }
        }



    }
}
