using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using FluentValidation;

using JustDo.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Npgsql;

namespace JustDo.Infrastructure.Errors {
    public class ErrorHandlingMiddleware {

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger) {
            this.next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context) {
            try {
                await next(context);
            } catch (Exception ex) {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly RequestDelegate next;

        private static Task HandleExceptionAsync(
            HttpContext context,
            Exception exception,
            ILogger<ErrorHandlingMiddleware> logger) {
            ErrorResponse[] errors = null;

            switch (exception) {
                case RestException re:
                    errors = re.Errors;
                    context.Response.StatusCode = (int)re.Code;
                    break;

                case NpgsqlException nex:
                    logger.LogError((int)ErrorCodes.E_DB_CONN, nex, nex.Message);

                    errors = new ErrorResponse[] {
                        new ErrorResponse {
                            Error = ErrorCodes.E_DB_CONN,
                            Message = $"DB Error"
                        }
                    };

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    break;

                case ValidationException vex:
                    errors = vex.Errors.Select(e => new ErrorResponse {
                        Error = ErrorCodes.E_INVALID_DATA,
                        Message = $"{e.PropertyName} has invalid data: {e.ErrorMessage}"
                    }).ToArray();

                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    break;

                case Exception e:
                    errors = new ErrorResponse[] {
                        new ErrorResponse {
                            Error = ErrorCodes.E_UNKNOWN,
                            Message = "Unexpected error"
                        }
                    };

                    logger.LogError((int)ErrorCodes.E_UNKNOWN, exception, $"[{context.Response.StatusCode}]: {e.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.ContentType = "application/json";

            var result = JsonConvert.SerializeObject(new {
                errors
            });

            return context.Response.WriteAsync(result);
        }
    }
}