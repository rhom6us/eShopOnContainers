using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.IdentityServerExternalAuth {
    public class IdentityException : Exception {
        public IdentityException(string message, IEnumerable<IdentityError> errors) : base(message) {
            this.Errors = errors;
        }

        public IdentityException(IEnumerable<IdentityError> errors) {
            this.Errors = errors;
        }

        public IdentityException(Exception innerException, IEnumerable<IdentityError> errors) : base("See InnerException", innerException) {
            this.Errors = errors;
        }

        public IdentityException(string message, Exception innerException, IEnumerable<IdentityError> errors) : base(message, innerException) {
            this.Errors = errors;
        }
        public override string Message => string.Join('\n',base.Message, "See the 'Errors' property for more info");
        public IEnumerable<IdentityError> Errors { get; set; }
    }
}