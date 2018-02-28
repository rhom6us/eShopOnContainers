using System;
using Android.App;
using Android.Content;
using Android.OS;
using eShopOnContainers.Core.ViewModels;
using eShopOnContainers.Services;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;

[assembly: MetaData("com.facebook.sdk.ApplicationId", Value = "@string/app_id")]

namespace eShopOnContainers.Droid.Renderers
{
    [Activity(Label = "")]
    public class FacebookActivity : Activity {
        public static Action<string> SignedIn;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //if (AccessToken.CurrentAccessToken != null)
            //{
            //    App.PostSuccessFacebookAction(AccessToken.CurrentAccessToken.Token);
            //    this.Finish();
            //    return;
            //}

        }

        protected override void OnPostCreate(Bundle savedInstanceState) {
            base.OnPostCreate(savedInstanceState);
            callbackManager = CallbackManagerFactory.Create();
            LoginManager.Instance.RegisterCallback(callbackManager, new FacebookCallBack());

            LoginManager.Instance.LogInWithReadPermissions(this, new[] { "public_profile", "user_tagged_places" });
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
            this.Finish();
        }

        private ICallbackManager callbackManager;
    }

    public class FacebookCallBack : Java.Lang.Object, IFacebookCallback, IDisposable
    {
        #region IFacebookCallback implementation

        public void OnCancel() { }

        public void OnError(FacebookException p0) { }

        public void OnSuccess(Java.Lang.Object p0)
        {
            
            var loginResult = (LoginResult)p0;
            FacebookActivity.SignedIn?.Invoke(loginResult.AccessToken.Token);
            //App.PostSuccessFacebookAction(loginResult.AccessToken.Token);
          
        }

        #endregion
    }
}
