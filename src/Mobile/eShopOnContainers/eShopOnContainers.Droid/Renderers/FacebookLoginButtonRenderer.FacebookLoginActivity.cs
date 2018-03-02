
[assembly: Android.App.MetaData("com.facebook.sdk.ApplicationId", Value = "@string/app_id")]
namespace eShopOnContainers.Droid.Renderers {
    public partial class FacebookLoginButtonRenderer {

        [Android.App.Activity(Label = "")]
        public class FacebookLoginActivity : Android.App.Activity, Xamarin.Facebook.IFacebookCallback {
            public FacebookLoginActivity() {
                _callbackManager = Xamarin.Facebook.CallbackManagerFactory.Create().RegisterCallback(this);
            }

            protected override void OnCreate(Android.OS.Bundle bundle) {
                base.OnCreate(bundle);

                
                Xamarin.Facebook.Login.LoginManager.Instance.LogInWithReadPermissions(this, new[] {"public_profile", "user_tagged_places", "user_posts", "user_likes"});
            }

            protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Android.Content.Intent data) {
                base.OnActivityResult(requestCode, resultCode, data);

                _callbackManager.OnActivityResult(requestCode, (int) resultCode, data);
                this.Finish();
            }


            void Xamarin.Facebook.IFacebookCallback.OnCancel() => this.SendFacebookLoginCanceled();

            void Xamarin.Facebook.IFacebookCallback.OnError(Xamarin.Facebook.FacebookException error) => this.SendFacebookLoginError(error);

            void Xamarin.Facebook.IFacebookCallback.OnSuccess(Java.Lang.Object result) =>
                this.SendFacebookLoginSuccess((Xamarin.Facebook.Login.LoginResult) result);

            private readonly Xamarin.Facebook.ICallbackManager _callbackManager;
        }
    }
}