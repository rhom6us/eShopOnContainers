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
