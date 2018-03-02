using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace UserActions.Api.Webhooks.Facebook
{


    [Route("/webhooks/facebook")]
    public class FacebookWebHooksController : Controller {
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public FacebookWebHooksController(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: /<controller>/
        [HttpGet]
        public IActionResult Get([FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string token) {
            if (token == "omgaw")
                return this.Ok(challenge);
            else
                return this.BadRequest();
        }

        [HttpPost]
        public IActionResult Post(/*[FromBody]FacebookWebhookUpdateBindingModel<dynamic> model,*/ [FromHeader(Name = "X-Hub-Signature")]string signature) {

            using (var reader = new StreamReader(this.Request.Body)) {
                var json = reader.ReadToEnd();
            }

            /*

            var entries = model.Entries
                .Where(p => p.Id != "0").ToArray();
                */

            return this.Ok();
        }


    }

}
