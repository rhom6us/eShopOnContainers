using Android.Content;
using System;
using Android.App;
using Xamarin.Forms.Platform.Android;
using Object = Java.Lang.Object;
using View = Android.Views.View;
using Xamarin.Facebook;
using eShopOnContainers.Core.Controls;
using eShopOnContainers.Droid.Renderers;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(FacebookLoginButton), typeof(FacebookLoginButtonRendererAndroid))]
namespace eShopOnContainers.Droid.Renderers
{
    public class FacebookLoginButtonRendererAndroid : ButtonRenderer
    {
        private static Activity _activity;

        public FacebookLoginButtonRendererAndroid(Context context) : base(context) {
           
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            _activity = this.Context as Activity;

            //DEBUG
            //Xamarin.Facebook.Login.LoginManager.Instance.LogOut();

            if (this.Control != null)
            {
                var button = this.Control;
                button.SetOnClickListener(ButtonClickListener.Instance.Value);
            }

         
        }

        private class ButtonClickListener : Object, IOnClickListener
        {
            public static readonly Lazy<ButtonClickListener> Instance = new Lazy<ButtonClickListener>(() => new ButtonClickListener());

            public void OnClick(View v) {
                var renderer = (FacebookLoginButtonRendererAndroid)v.Tag;
                var controller = (IFacebookLoginButtonController) renderer.Element;
                var myIntent = new Intent(_activity, typeof(FacebookActivity));
                FacebookActivity.SignedIn += token => {
                    controller.SetValueFromRenderer(FacebookLoginButton.AccessTokenProperty, token);
                    controller.SendSignedIn();
                };
                _activity.StartActivityForResult(myIntent, 0);
            }
        }
    }
}