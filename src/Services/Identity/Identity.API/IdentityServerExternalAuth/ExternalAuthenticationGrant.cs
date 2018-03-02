using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.API.IdentityServerExternalAuth.Providers;
using Identity.API.IntegrationEvents;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.API.IdentityServerExternalAuth
{
    public class ExternalAuthenticationGrant<TUser> : IExtensionGrantValidator where TUser : IdentityUser, new()
    {

        private readonly IIdentityIntegrationEventService _integrationService;

        [NotNull]
        private readonly UserManager<TUser> _userManager;

        private readonly IOptions<AppSettings> _appSettings;

        // private readonly IGoogleAuthProvider _googleAuthProvider;
        //private readonly ILinkedInAuthProvider _linkedAuthProvider;
        // private readonly IGitHubAuthProvider _githubAuthProvider;
        public ExternalAuthenticationGrant(
            [NotNull] UserManager<TUser> userManager,
            [NotNull] IEnumerable<IExternalAuthProvider> providers, 
            [NotNull] IIdentityIntegrationEventService integrationService, 
            [NotNull] IOptions<AppSettings> appSettings,
            [NotNull] ILoggerFactory loggerFactory
            ) {
            _logger = loggerFactory.CreateLogger< ExternalAuthenticationGrant<TUser>>();
            _userManager = userManager;
            _integrationService = integrationService;
            _appSettings = appSettings;

            _providers = providers.ToDictionary(p => p.ProviderKey, StringComparer.OrdinalIgnoreCase);


        }


        private readonly Dictionary<string, IExternalAuthProvider> _providers;
        private readonly ILogger<ExternalAuthenticationGrant<TUser>> _logger;

        //public string GrantType => "external";


        async Task IExtensionGrantValidator.ValidateAsync(ExtensionGrantValidationContext context) {
            var provider = context.Request.Raw.Get("provider");
            if (string.IsNullOrWhiteSpace(provider)) {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid provider");
                return;
            }

            //var userId = context.Request.Raw.Get("userId");
            //if (string.IsNullOrWhiteSpace(userId)) {
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid userId");
            //    return;
            //}

            //var applicationId = context.Request.Raw.Get("applicationId");
            //if (string.IsNullOrWhiteSpace(applicationId)) {
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid applicationId");
            //    return;
            //}


            var token = context.Request.Raw.Get("token");
            if (string.IsNullOrWhiteSpace(token)) {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid external token");
                return;
            }


            if (!_providers.ContainsKey(provider)) {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "invalid provider");
                return;
            }

            var userInfo = _providers[provider].GetUserInfo(token);

            if (userInfo == null) {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "couldn't retrieve user info from specified provider, please make sure that access token is not expired.");
                return;
            }

            var externalId = userInfo.Value<string>("id");
            if (string.IsNullOrWhiteSpace(externalId)) {
                throw new InvalidOperationException($"Did not receive a Id from the provider ({provider})");
            }

            try {
                var user = await _userManager.FindByLoginAsync(provider, externalId);
                if (user == null) { // new user

                    //TODO: validate accessToken is from ours 
                    var email = context.Request.Raw.Get("email") ?? userInfo.Value<string>("email");
                    var username = context.Request.Raw.Get("username") ?? userInfo.Value<string>("username") ?? email ?? externalId;
                    user = new TUser {Email = email, UserName = username};
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded) {
                        throw new IdentityException("failed to create user", result.Errors);
                    }

                    result = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, externalId, provider));
                    if (!result.Succeeded) {
                        throw new IdentityException("failed add login", result.Errors);
                    }

                    user = await _userManager.FindByIdAsync(user?.Id) ?? throw new InvalidOperationException("Could not load newly created user");

                }

                var userClaims = await _userManager.GetClaimsAsync(user);
                context.Result = new GrantValidationResult(user.Id, provider, userClaims, provider, null);
                await _integrationService.AccessTokenReceived(user.Id, provider, _appSettings.Value.Authentication[provider]?.ApplicationId, externalId, token);

            }

            catch (IdentityException e) {
                _logger.LogWarning(e, "UserManager failed to do as expected in the custom token_exchange grant");
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest,
                    "user could not be created, please try again",
                    e.Errors.ToDictionary(p => p.Code, p => p.Description as object));

            }
            catch (Exception ex) {
                _logger.LogError(ex, "unexpected error");
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "user could not be created, please try again");
            }

            
        }

        [NotNull]
        string IExtensionGrantValidator.GrantType => ExternalAuthenticationGrant<TUser>.GrantType;


        [NotNull]
        public static string GrantType => "token_exchange";
    }
}