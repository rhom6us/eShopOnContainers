namespace eShopOnContainers.Core
{
    public class GlobalSetting
    {
        public const string AzureTag = "Azure";
        public const string MockTag = "Mock";
        public const string DefaultEndpoint = "http://13.88.8.119";

        private string _baseEndpoint;
        private static readonly GlobalSetting _instance = new GlobalSetting();

        public GlobalSetting()
        {
            AuthToken = "INSERT AUTHENTICATION TOKEN";
            BaseEndpoint = DefaultEndpoint;
        }

        public static GlobalSetting Instance => _instance;

        public string BaseEndpoint
        {
            get => _baseEndpoint;
            set
            {
                _baseEndpoint = value;
                UpdateEndpoint(_baseEndpoint);
            }
        }

        public string ClientId => "xamarin";

        public string ClientSecret => "secret";

        public string AuthToken { get; set; }

        public string RegisterWebsite { get; set; }

        public string CatalogEndpoint { get; set; }

        public string OrdersEndpoint { get; set; }

        public string BasketEndpoint { get; set; }

        public string IdentityEndpoint { get; set; }

        public string LocationEndpoint { get; set; }

        public string MarketingEndpoint { get; set; }

        public string UserInfoEndpoint { get; set; }

        public string TokenEndpoint { get; set; }

        public string LogoutEndpoint { get; set; }

        public string IdentityCallback { get; set; }

        public string LogoutCallback { get; set; }

        private void UpdateEndpoint(string baseEndpoint)
        {
            RegisterWebsite = $"https://abeb159c.ngrok.io/Account/Register";
            CatalogEndpoint = $"{baseEndpoint}:5101";
            OrdersEndpoint = $"{baseEndpoint}:5102";
            BasketEndpoint = $"{baseEndpoint}:5103";
            IdentityEndpoint = $"https://abeb159c.ngrok.io/connect/authorize";
            UserInfoEndpoint = $"https://abeb159c.ngrok.io/connect/userinfo";
            TokenEndpoint = $"https://abeb159c.ngrok.io/connect/token";
            LogoutEndpoint = $"https://abeb159c.ngrok.io/connect/endsession";
            IdentityCallback = $"https://abeb159c.ngrok.io/xamarincallback";
            LogoutCallback = $"https://abeb159c.ngrok.io/Account/Redirecting";
            LocationEndpoint = $"{baseEndpoint}:5109";
            MarketingEndpoint = $"{baseEndpoint}:5110";
        }
    }
}