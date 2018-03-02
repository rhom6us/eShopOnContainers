// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Identity.API
{
    public class AppSettings
    {
        public string MvcClient { get; set; }

        public bool UseCustomizationData { get; set; }
        public string EventBusConnection { get; set; }
        public SocialAuthenticationSettings Authentication { get; set; }
    }
    public class FacebookSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AppId { get; set; }
    }
}
