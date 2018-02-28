using Android.Content;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Xamarin.Forms.Platform.Android;
using eShopOnContainers.Core.Controls;
using eShopOnContainers.Droid.Renderers;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(FacebookLoginButton), typeof(FacebookLoginButtonRenderer))]
namespace eShopOnContainers.Droid.Renderers
{
    public partial class FacebookLoginButtonRenderer : ButtonRenderer, Android.Views.View.IOnClickListener
    {

        public FacebookLoginButtonRenderer(Context context) : base(context) {  }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);
            //DEBUG
            //Xamarin.Facebook.Login.LoginManager.Instance.LogOut();

            this.Control?.SetOnClickListener(this);
        }

        void IOnClickListener.OnClick(Android.Views.View v) {
            var renderer = (FacebookLoginButtonRenderer)v.Tag;
            var controller = (IFacebookLoginButtonController)renderer.Element;
            //MessagingCenter.Subscribe<FacebookLoginActivity, Xamarin.Facebook.Login.LoginResult>(this, Core.ViewModels.Base.MessageKeys.FacebookLogin,
            this.SubscribeFacebookLoginSuccess(loginResult => {
                    var token = new Core.Models.Token.FacebookAccessToken {
                        Token = loginResult.AccessToken.Token,
                        ApplicationId = loginResult.AccessToken.ApplicationId,
                        DeclinedPermissions = loginResult.AccessToken.ApplicationId.ToList().AsReadOnly(),
                        Expires = System.DateTimeOffset.FromUnixTimeMilliseconds(loginResult.AccessToken.Expires.Time),
                        IsExpired = loginResult.AccessToken.IsExpired,
                        LastRefresh = System.DateTimeOffset.FromUnixTimeMilliseconds(loginResult.AccessToken.LastRefresh.Time),
                        Permissions = loginResult.AccessToken.Permissions.ToList().AsReadOnly(),
                        UserId = loginResult.AccessToken.UserId
                    };
                    controller.SetValueFromRenderer(FacebookLoginButton.AccessTokenProperty, token.Token);
                    controller.SendSignedIn();
                });
            ((Activity)this.Context).StartActivityForResult<FacebookLoginActivity>();
        }
    }
}