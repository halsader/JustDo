using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation;

using MediatR;

namespace JustDo.Infrastructure {
    public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> {
        #region Public Constructors

        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators) {
            _validators = validators.ToList();
        }

        #endregion Public Constructors

        #region Public Methods

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next) {
            var context = new ValidationContext(request);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0) {
                throw new ValidationException(failures);
            }

            return next();
        }

        #endregion Public Methods

        #region Private Fields

        private readonly List<IValidator<TRequest>> _validators;

        #endregion Private Fields
    }
}