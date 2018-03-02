using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace UserActions.Api
{
    public class SocialAuthenticationSettings
    {
        public OAuthClientSettings Facebook { get; set; }
        public OAuthClientSettings Twitter { get; set; }
        public OAuthClientSettings Foursquare { get; set; }
        public OAuthClientSettings Instagram { get; set; }
        public OAuthClientSettings Flickr { get; set; }
        public OAuthClientSettings Pinterest { get; set; }
        public OAuthClientSettings Vimeo { get; set; }

        private static readonly Lazy<Dictionary<string, PropertyDescriptor>> _props = new Lazy<Dictionary<string, PropertyDescriptor>>(() =>
            TypeDescriptor.GetProperties(typeof(SocialAuthenticationSettings))
                .OfType<PropertyDescriptor>()
                .Where(p => typeof(OAuthClientSettings).IsAssignableFrom(p.PropertyType))
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase)

        );
        public OAuthClientSettings this[string provider] =>
            _props.Value[provider].GetValue(this) as OAuthClientSettings;

    }
    public class OAuthClientSettings
    {

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientSecret { get; set; }

        public string ApplicationId { get; set; }
    }
}