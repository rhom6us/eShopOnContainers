namespace eShopOnContainers.Droid.Renderers {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]

    internal static class FacebookSdkExtensions {
        public static Xamarin.Facebook.ICallbackManager RegisterCallback([NotNull] this Xamarin.Facebook.ICallbackManager callbackManager, [NotNull] Xamarin.Facebook.IFacebookCallback callback) {
            Xamarin.Facebook.Login.LoginManager.Instance.RegisterCallback(callbackManager, callback);
            return callbackManager;
        }
   
        public const string FacebookLogin = "FacebookLogin";
        public static void SendFacebookLoginSuccess(this FacebookLoginButtonRenderer.FacebookLoginActivity sender, Xamarin.Facebook.Login.LoginResult loginResult) {
            Xamarin.Forms.MessagingCenter.Send(sender, FacebookLogin, loginResult);
        }
        public static void SendFacebookLoginError(this FacebookLoginButtonRenderer.FacebookLoginActivity sender, Xamarin.Facebook.FacebookException error) {
            Xamarin.Forms.MessagingCenter.Send(sender, FacebookLogin, error);
        }
        public static void SendFacebookLoginCanceled(this FacebookLoginButtonRenderer.FacebookLoginActivity sender) {
            Xamarin.Forms.MessagingCenter.Send(sender, FacebookLogin);
        }

        public static void SubscribeFacebookLoginSuccess(this FacebookLoginButtonRenderer subscriber, System.Action<FacebookLoginButtonRenderer.FacebookLoginActivity, Xamarin.Facebook.Login.LoginResult> callback) {
            Xamarin.Forms.MessagingCenter.Subscribe(subscriber, FacebookLogin, callback);
        }
        public static void SubscribeFacebookLoginError(this FacebookLoginButtonRenderer subscriber, System.Action<FacebookLoginButtonRenderer.FacebookLoginActivity, Xamarin.Facebook.FacebookException> callback) {
            Xamarin.Forms.MessagingCenter.Subscribe(subscriber, FacebookLogin, callback);
        }

        public static void SubscribeFacebookLoginCanceled(this FacebookLoginButtonRenderer subscriber, System.Action<FacebookLoginButtonRenderer.FacebookLoginActivity> callback) {
            Xamarin.Forms.MessagingCenter.Subscribe(subscriber, FacebookLogin, callback);
        }
        public static void SubscribeFacebookLoginSuccess(this FacebookLoginButtonRenderer subscriber, System.Action<Xamarin.Facebook.Login.LoginResult> callback) {
            FacebookSdkExtensions.SubscribeFacebookLoginSuccess(subscriber, (sender,result) => callback(result));
        }
        public static void SubscribeFacebookLoginError(this FacebookLoginButtonRenderer subscriber, System.Action<Xamarin.Facebook.FacebookException> callback) {
            FacebookSdkExtensions.SubscribeFacebookLoginError(subscriber, (sender, error) => callback(error));
        }

        public static void SubscribeFacebookLoginCanceled(this FacebookLoginButtonRenderer subscriber, System.Action callback) {
            FacebookSdkExtensions.SubscribeFacebookLoginCanceled(subscriber, sender => callback());
        }

    }
}