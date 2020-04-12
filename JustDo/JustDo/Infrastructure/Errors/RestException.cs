using System;
using System.Net;

using JustDo.Models;

namespace JustDo.Infrastructure.Errors {
    public class RestException : Exception {
        #region Public Constructors

        public RestException(HttpStatusCode code, ErrorResponse[] errors = null) {
            Code = code;
            Errors = errors;
        }

        public RestException() : base() {
        }

        public RestException(string message) : base(message) {
        }

        public RestException(string message, Exception innerException) : base(message, innerException) {
        }

        #endregion Public Constructors

        #region Public Properties

        public HttpStatusCode Code { get; }
        public ErrorResponse[] Errors { get; set; }

        #endregion Public Properties
    }
}