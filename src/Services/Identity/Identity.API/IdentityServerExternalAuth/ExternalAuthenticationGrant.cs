using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using IdentityServerExternalAuth.Helpers;
using IdentityServerExternalAuth.Interfaces;
using IdentityServerExternalAuth.Interfaces.Processors;
using IdentityServerExternalAuth.Repositories.Interfaces;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;

namespace IdentityServerExternalAuth.ExtensionGrant
{
    public class ExternalAuthenticationGrant<TUser> : IExtensionGrantValidator where TUser : IdentityUser, new()
    {
        [NotNull]
        private readonly UserManager<TUser> _userManager;

        [NotNull]
        private readonly IProviderRepository _providerRepository;

        [NotNull]
        private readonly INonEmailUserProcessor _nonEmailUserProcessor;

        [NotNull]
        private readonly IEmailUserProcessor _emailUserProcessor;

        // private readonly IGoogleAuthProvider _googleAuthProvider;
        //private readonly ILinkedInAuthProvider _linkedAuthProvider;
        // private readonly IGitHubAuthProvider _githubAuthProvider;
        public ExternalAuthenticationGrant(
            [NotNull] UserManager<TUser> userManager,
            [NotNull] IProviderRepository providerRepository,
            [NotNull] IFacebookAuthProvider facebookAuthProvider,
            //  IGoogleAuthProvider googleAuthProvider,
            [NotNull] ITwitterAuthProvider twitterAuthProvider,
             //  ILinkedInAuthProvider linkeInAuthProvider,
             //  IGitHubAuthProvider githubAuthProvider,
            [NotNull] INonEmailUserProcessor nonEmailUserProcessor,
            [NotNull] IEmailUserProcessor emailUserProcessor
        ) {
            _userManager = userManager;
            _providerRepository = providerRepository;
            _nonEmailUserProcessor = nonEmailUserProcessor;
            _emailUserProcessor = emailUserProcessor;


            _providers = new Dictionary<ProviderType, IExternalAuthProvider>
            {
                [ProviderType.Facebook] = facebookAuthProvider,
                // [ProviderType.Google] = _googleAuthProvider,
                [ProviderType.Twitter] = twitterAuthProvider,
                //[ProviderType.LinkedIn] = _linkedAuthProvider
            };
        }


        private readonly Dictionary<ProviderType, IExternalAuthProvider> _providers;

        public string GrantType => "external";



        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var provider = context.Request.Raw.Get("provider");
            if (string.IsNullOrWhiteSpace(provider))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid provider");
                return;
            }


            var token = context.Request.Raw.Get("external_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid external token");
                return;
            }

            var requestEmail = context.Request.Raw.Get("email");

            var providerType = (ProviderType)Enum.Parse(typeof(ProviderType), provider, true);

            if (!Enum.IsDefined(typeof(ProviderType), providerType))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid provider");
                return;
            }

            var userInfo = _providers[providerType].GetUserInfo(token);

            if (userInfo == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "couldn't retrieve user info from specified provider, please make sure that access token is not expired.");
                return;
            }

            var externalId = userInfo.Value<string>("id");
            if (!string.IsNullOrWhiteSpace(externalId))
            {

                var user = await _userManager.FindByLoginAsync(provider, externalId);
                if (null != user)
                {
                    user = await _userManager.FindByIdAsync(user.Id);
                    var userClaims = await _userManager.GetClaimsAsync(user);
                    context.Result = new GrantValidationResult(user.Id, provider, userClaims, provider, null);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(requestEmail))
            {
                context.Result = await _nonEmailUserProcessor.ProcessAsync(userInfo, provider);
                return;
            }

            context.Result = await _emailUserProcessor.ProcessAsync(userInfo, requestEmail, provider);
            return;
        }
    }
}