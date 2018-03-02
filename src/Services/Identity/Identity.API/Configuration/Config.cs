using System.Collections.Generic;
using System.Linq;
using Identity.API.Data;
using IdentityServer4;
using IdentityServer4.Models;
using JetBrains.Annotations;

namespace Identity.API.Configuration
{
    public class Config
    {
        // ApiResources define the apis in your system
        [NotNull]
        public static IEnumerable<ApiResource> GetApis() => new[] {
            new ApiResource("orders", "Orders Service"),
            new ApiResource("basket", "Basket Service"),
            new ApiResource("marketing", "Marketing Service"),
            new ApiResource("locations", "Locations Service")
        };

        [NotNull]
        public static IEnumerable<LoginProvider> GetProviders() => new[] {
            new LoginProvider {
                DisplayName = "Facebook",
                UserInfoEndPoint = "https://graph.facebook.com/v2.12/me"
            },
            new LoginProvider {
                DisplayName = "Google",
                UserInfoEndPoint = "https://www.googleapis.com/oauth2/v2/userinfo"
            },
            new LoginProvider {
                DisplayName = "Twitter",
                UserInfoEndPoint = "https://api.twitter.com/1.1/account/verify_credentials.json"
            },
            new LoginProvider {
                DisplayName = "LinkedIn",
                UserInfoEndPoint = "https://api.linkedin.com/v1/people/~:(id,email-address,first-name,last-name,location,industry,picture-url)?"
            },
            new LoginProvider {
                DisplayName = "GitHub",
                UserInfoEndPoint = "https://api.github.com/user"
            }
        }.Select(p => {
            p.Id = p.Id ?? p.DisplayName?.ToLower();
            return p;
        }).ToArray();

        // Identity resources are data like user ID, name, or email address of a user
        // see: http://docs.identityserver.io/en/release/configuration/resources.html
        [NotNull]
        public static IEnumerable<IdentityResource> GetResources() {
            yield return new IdentityResources.OpenId();
            yield return new IdentityResources.Profile();
        }

        // client want to access resources (aka scopes)
        [NotNull]
        public static Client[] GetClients(Dictionary<string, string> clientsUrl) => new[] {
        
        
            // JavaScript Client
            new Client {
                    ClientId = "js",
                    ClientName = "eShop SPA OpenId Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = {$"{clientsUrl["Spa"]}/"},
                    RequireConsent = false,
                    PostLogoutRedirectUris = {$"{clientsUrl["Spa"]}/"},
                    AllowedCorsOrigins = {$"{clientsUrl["Spa"]}"},
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "orders",
                        "basket",
                        "locations",
                        "marketing"
                    }
                },
                new Client {
                    ClientId = "xamarin",
                    ClientName = "eShop Xamarin OpenId Client",
                    AllowedGrantTypes = new []{GrantType.Hybrid, "token_exchange"},
                    //Used to retrieve the access token on the back channel.
                    ClientSecrets = {
                        new Secret("secret".Sha256())
                    },
                    RedirectUris = { clientsUrl["Xamarin"] },
                    RequireConsent = false,
                    RequirePkce = true,
                    PostLogoutRedirectUris = { $"{clientsUrl["Xamarin"]}/Account/Redirecting" },
                    AllowedCorsOrigins = { "http://eshopxamarin" },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orders",
                        "basket",
                        "locations",
                        "marketing",

                    },
                    //Allow requesting refresh tokens for long lived API access
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true
                },
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    ClientUri = $"{clientsUrl["Mvc"]}",                             // public uri of the client
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = new[]
                    {
                        $"{clientsUrl["Mvc"]}/signin-oidc"
                    },
                    PostLogoutRedirectUris = new []
                    {
                        $"{clientsUrl["Mvc"]}/signout-callback-oidc"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orders",
                        "basket",
                        "locations",
                        "marketing",
                    },
                },
                new Client
                {
                    ClientId = "mvctest",
                    ClientName = "MVC Client Test",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    ClientUri = $"{clientsUrl["Mvc"]}",                             // public uri of the client
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    RedirectUris = new List<string>
                    {
                        $"{clientsUrl["Mvc"]}/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        $"{clientsUrl["Mvc"]}/signout-callback-oidc"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orders",
                        "basket",
                        "locations",
                        "marketing"
                    },
                },
                new Client
                {
                    ClientId = "locationsswaggerui",
                    ClientName = "Locations Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientsUrl["LocationsApi"]}/swagger/o2c.html" },
                    PostLogoutRedirectUris = { $"{clientsUrl["LocationsApi"]}/swagger/" },

                    AllowedScopes =
                    {
                        "locations"
                    }
                },
                new Client
                {
                    ClientId = "marketingswaggerui",
                    ClientName = "Marketing Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientsUrl["MarketingApi"]}/swagger/o2c.html" },
                    PostLogoutRedirectUris = { $"{clientsUrl["MarketingApi"]}/swagger/" },

                    AllowedScopes =
                    {
                        "marketing"
                    }
                },
                new Client
                {
                    ClientId = "basketswaggerui",
                    ClientName = "Basket Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientsUrl["BasketApi"]}/swagger/o2c.html" },
                    PostLogoutRedirectUris = { $"{clientsUrl["BasketApi"]}/swagger/" },

                    AllowedScopes =
                    {
                        "basket"
                    }
                },
                new Client
                {
                    ClientId = "orderingswaggerui",
                    ClientName = "Ordering Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{clientsUrl["OrderingApi"]}/swagger/o2c.html" },
                    PostLogoutRedirectUris = { $"{clientsUrl["OrderingApi"]}/swagger/" },

                    AllowedScopes =
                    {
                        "orders"
                    }
                }
        };
    }
}