using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using eShopOnContainers.Core.Models.User;
using eShopOnContainers.Core.Services.Identity;
using eShopOnContainers.Core.Services.OpenUrl;
using eShopOnContainers.Core.Services.Settings;
using eShopOnContainers.Core.Validations;
using eShopOnContainers.Core.ViewModels.Base;
using IdentityModel.Client;
using Xamarin.Forms;

namespace eShopOnContainers.Core.ViewModels {
    public class LoginViewModel : ViewModelBase {
        public ValidatableObject<string> UserName {
            get => _userName;
            set {
                _userName = value;
                this.RaisePropertyChanged(() => this.UserName);
            }
        }

        public ValidatableObject<string> Password {
            get => _password;
            set {
                _password = value;
                this.RaisePropertyChanged(() => this.Password);
            }
        }

        public bool IsMock {
            get => _isMock;
            set {
                _isMock = value;
                this.RaisePropertyChanged(() => this.IsMock);
            }
        }

        public bool IsValid {
            get => _isValid;
            set {
                _isValid = value;
                this.RaisePropertyChanged(() => this.IsValid);
            }
        }

        public bool IsLogin {
            get => _isLogin;
            set {
                _isLogin = value;
                this.RaisePropertyChanged(() => this.IsLogin);
            }
        }

        public string LoginUrl {
            get => _authUrl;
            set {
                _authUrl = value;
                this.RaisePropertyChanged(() => this.LoginUrl);
            }
        }

        private string _facebookToken;
        public string FacebookToken {
            get => _facebookToken;
            set {
                _facebookToken = value;
                this.RaisePropertyChanged(() => this.FacebookToken);
            }
        }
        public ICommand MockSignInCommand => new Command(async () => await this.MockSignInAsync());

        public ICommand SignInCommand =>
            new Command(
                async () => {
                    this.IsBusy = true;

                    await Task.Delay(10);

                    this.LoginUrl = _identityService.CreateAuthorizationRequest();

                    this.IsValid = true;
                    this.IsLogin = true;
                    this.IsBusy = false;
                });

        public ICommand RegisterCommand => new Command(() => {
            _openUrlService.OpenUrl(GlobalSetting.Instance.RegisterWebsite);
        });

        public ICommand ExternalSignInCommand => new Command<string>(async token => {
            IsLogin = true;
            await Task.WhenAll(_identityService.ExchangeToken(token), NavigationService.NavigateToAsync<MainViewModel>());
            await NavigationService.RemoveLastFromBackStackAsync();
        });

        public ICommand NavigateCommand =>
            new Command<string>(
                async url => {
                    var unescapedUrl = WebUtility.UrlDecode(url);

                    if (unescapedUrl.Equals(GlobalSetting.Instance.LogoutCallback)) {
                        _settingsService.AuthAccessToken = string.Empty;
                        _settingsService.AuthIdToken = string.Empty;
                        this.IsLogin = false;
                        this.LoginUrl = _identityService.CreateAuthorizationRequest();
                    } else {
                        if (unescapedUrl.Contains(GlobalSetting.Instance.IdentityCallback)) {
                            var authResponse = new AuthorizeResponse(url);
                            if (!string.IsNullOrWhiteSpace(authResponse.Code)) {
                                var userToken = await _identityService.GetTokenAsync(authResponse.Code);
                                var accessToken = userToken.AccessToken;

                                if (!string.IsNullOrWhiteSpace(accessToken)) {
                                    _settingsService.AuthAccessToken = accessToken;
                                    _settingsService.AuthIdToken = authResponse.IdentityToken;
                                    await NavigationService.NavigateToAsync<MainViewModel>();
                                    await NavigationService.RemoveLastFromBackStackAsync();
                                }
                            }
                        }
                    }
                });

        public ICommand SettingsCommand => new Command(() => {
            NavigationService.NavigateToAsync<SettingsViewModel>();
        });

        public ICommand ValidateUserNameCommand => new Command(() => _userName.Validate());

        public ICommand ValidatePasswordCommand => new Command(() => _password.Validate());

       

        public LoginViewModel(ISettingsService settingsService, IOpenUrlService openUrlService, IIdentityService identityService) {
            _settingsService = settingsService;
            _openUrlService = openUrlService;
            _identityService = identityService;

            _userName = new ValidatableObject<string>();
            _password = new ValidatableObject<string>();

            this.InvalidateMock();
            this.AddValidations();
        }

        public override Task InitializeAsync(object navigationData) {
            if (navigationData is LogoutParameter logoutParameter) {
                if (logoutParameter.Logout)
                    this.Logout();
            }

            return base.InitializeAsync(navigationData);
        }

        private async Task MockSignInAsync() {
            this.IsBusy = true;
            this.IsValid = true;
            var isValid = this.Validate();
            var isAuthenticated = false;

            if (isValid) {
                try {
                    await Task.Delay(10);

                    isAuthenticated = true;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"[SignIn] Error signing in: {ex}");
                }
            } else
                this.IsValid = false;

            if (isAuthenticated) {
                _settingsService.AuthAccessToken = GlobalSetting.Instance.AuthToken;

                await NavigationService.NavigateToAsync<MainViewModel>();
                await NavigationService.RemoveLastFromBackStackAsync();
            }

            this.IsBusy = false;
        }

   

        private void Logout() {
            var authIdToken = _settingsService.AuthIdToken;
            var logoutRequest = _identityService.CreateLogoutRequest(authIdToken);

            if (!string.IsNullOrEmpty(logoutRequest)) {
                // Logout
                this.LoginUrl = logoutRequest;
            }

            if (_settingsService.UseMocks) {
                _settingsService.AuthAccessToken = string.Empty;
                _settingsService.AuthIdToken = string.Empty;
            }

            _settingsService.UseFakeLocation = false;
        }

        private bool Validate() {
            var isValidUser = this.ValidateUserName();
            var isValidPassword = this.ValidatePassword();

            return isValidUser && isValidPassword;
        }

        private bool ValidateUserName() {
            return _userName.Validate();
        }

        private bool ValidatePassword() {
            return _password.Validate();
        }

        private void AddValidations() {
            _userName.Validations.Add(new IsNotNullOrEmptyRule<string> {ValidationMessage = "A username is required."});
            _password.Validations.Add(new IsNotNullOrEmptyRule<string> {ValidationMessage = "A password is required."});
        }

        public void InvalidateMock() {
            this.IsMock = _settingsService.UseMocks;
        }

        private readonly IIdentityService _identityService;
        private readonly IOpenUrlService _openUrlService;

        private readonly ISettingsService _settingsService;
        private string _authUrl;
        private bool _isLogin;
        private bool _isMock;
        private bool _isValid;
        private ValidatableObject<string> _password;
        private ValidatableObject<string> _userName;
    }
}
