using System.Threading.Tasks;
using Identity.API.Models;
using Identity.API.Services;
using IdentityServer4.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Identity.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IOptionsSnapshot<AppSettings> _settings;
        private readonly IRedirectService _redirectSvc;

        public HomeController(IIdentityServerInteractionService interaction, IOptionsSnapshot<AppSettings> settings,IRedirectService redirectSvc)
        {
            _interaction = interaction;
            _settings = settings;
            _redirectSvc = redirectSvc;
        }

        public IActionResult Index(string returnUrl)
        {
            return this.View();
        }

        public IActionResult ReturnToOriginalApplication([CanBeNull] string returnUrl)
        {
            if (returnUrl != null)
                return this.Redirect(_redirectSvc.ExtractRedirectUriFromReturnUrl(returnUrl));
            else
                return this.RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Shows the error page
        /// </summary>
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;
            }

            return this.View("Error", vm);
        }
    }
}