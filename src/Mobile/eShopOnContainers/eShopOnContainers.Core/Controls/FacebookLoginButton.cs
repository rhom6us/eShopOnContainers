using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace eShopOnContainers.Core.Controls
{
    public class FacebookLoginButton : Button, IFacebookLoginButtonController
    {
        public FacebookLoginButton() {
            this.Text = "Sign in with Facebook";
            var facebookBackground = Color.FromHsla(147 / 255d, 106 / 255d, 99 / 255d);
            var foursquareBackground = Color.FromHsla(134/255d, 240 / 255d, 86 / 255d);
            var twitterbackground = Color.FromHsla(131 / 255d, 240 / 255d, 114 / 255d);
            var instagramBackground = Color.FromHsla(138 / 255d, 101 / 255d, 103 / 255d);
            var googlebackground = Color.FromHsla(4/ 255d, 170 / 255d, 131 / 255d);
            this.BackgroundColor = facebookBackground;
            this.TextColor = Color.White;
            
        }
        public static readonly BindableProperty SignInCommandProperty = BindableProperty.Create(nameof(SignInCommand), typeof(ICommand), typeof(FacebookLoginButton));
        public static readonly BindableProperty AccessTokenProperty =
            BindableProperty.Create("AccessToken", typeof(string), typeof(FacebookLoginButton));

        public ICommand SignInCommand{
            get => (ICommand)GetValue(SignInCommandProperty); 
            set => SetValue(SignInCommandProperty, value); 
        }

        public event EventHandler SignedIn; 

        public string AccessToken{
            get => (string)GetValue(AccessTokenProperty); 
            set => SetValue(AccessTokenProperty, value); 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendSignedIn()
        {
            this.SignInCommand?.Execute(this.AccessToken);
            this.SignedIn?.Invoke(this, EventArgs.Empty);
        }

        
    }

    public interface IFacebookLoginButtonController : IButtonController {
        void SendSignedIn();
    }
}
