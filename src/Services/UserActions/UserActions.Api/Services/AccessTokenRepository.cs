using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserActions.Api.IntegrationEvents.Handlers;

namespace UserActions.Api.Services
{


    public class AccessTokenRepository : IAccessTokenRepository {
        private readonly ISet<AccessTokenData> _db;


        public AccessTokenRepository() {
            _db = new HashSet<AccessTokenData>();
        }
        //public AccessTokenRepository([NotNull]UserActionsContext db) {
        //    _db = db.Set<AccessTokenData>();
        //}



        //[CanBeNull]
        //public  Task<string> GetAccessTokenAsync([NotNull] string provider, [CanBeNull] string applicationId, [NotNull] string surrogateId) {
        //    return _db
        //        .Where(p => p.Provider == provider)
        //        .Where(p => p.ApplicationId == applicationId)
        //        .Where(p => p.SurrogateId == surrogateId)
        //        .Select(p => p.AccessToken)
        //        .SingleOrDefaultAsync();
        //}

        //public async Task UpdateAccessTokenAsync([NotNull] string provider, [CanBeNull] string applicationId, [NotNull] string surrogateId, [NotNull] string accessToken) {


        //    var token = await _db.Set<AccessTokenData>()
        //        .Where(p => p.Provider == provider)
        //        .Where(p => p.ApplicationId == applicationId)
        //        .Where(p => p.SurrogateId == surrogateId)
        //        .SingleOrDefaultAsync()
        //            ??
        //        (await _db.Set<AccessTokenData>()
        //            .AddAsync(new AccessTokenData {
        //                Provider = provider,
        //                ApplicationId = applicationId,
        //                SurrogateId = surrogateId
        //            })).Entity;

        //    token.AccessToken = accessToken;

        //    await _db.SaveChangesAsync();
        //}

        public async Task<string> GetAccessTokenAsync(string provider, string applicationId, string surrogateId) {
            return _db
                .Where(p => p.Provider == provider)
                .Where(p => p.ApplicationId == applicationId)
                .Where(p => p.SurrogateId == surrogateId)
                .Select(p => p.AccessToken)
                .SingleOrDefault();
        }

        public async Task UpdateAccessTokenAsync(string provider, string applicationId, string surrogateId, string accessToken) {


            var token =  _db
                .Where(p => p.Provider == provider)
                .Where(p => p.ApplicationId == applicationId)
                .Where(p => p.SurrogateId == surrogateId)
                .SingleOrDefault();

            if (token == null) {
                token = (new AccessTokenData {
                    Provider = provider,
                    ApplicationId = applicationId,
                    SurrogateId = surrogateId
                });
                _db.Add(token);
            }
                

            token.AccessToken = accessToken;
        }
    }
}
