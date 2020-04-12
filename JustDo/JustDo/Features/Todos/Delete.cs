using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation;

using JustDo.Infrastructure.Db.Entity;
using JustDo.Infrastructure.Errors;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SmartAnalyzers.CSharpExtensions.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace JustDo.Features.Todos {
    public static class Delete {
        public class Handler : IRequestHandler<Command, Unit> {
            private readonly TodoContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(TodoContext context, ILogger<Handler> logger) {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<Unit> Handle(Command c, CancellationToken ct) {
                if (c is null) {
                    throw new ArgumentNullException(nameof(c));
                }

                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async (ct) => {
                    var existingTodo = await _context.Todos.SingleOrDefaultAsync(x => x.Id == c.Id, ct).ConfigureAwait(false);

                    if (existingTodo is null) {
                        throw new RestException(
                            System.Net.HttpStatusCode.NoContent,
                            new Models.ErrorResponse[] {
                                new Models.ErrorResponse {
                                    Error = Models.ErrorCodes.E_OBJECT_NOT_FOUND,
                                    Message = $"Cannot find todo with ID [{c.Id}]"
                                }
                            }
                        );
                    }

                    _context.Remove(existingTodo);

                    await _context.SaveChangesAsync(ct).ConfigureAwait(false);
                }, ct).ConfigureAwait(false);

                return Unit.Value;
            }
        }

        [InitOnly]
        [DataContract]
        public class Command : IRequest {

            [DataMember(Name = "id")]
            public Guid Id { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command> {

            public CommandValidator() {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        public class ModelExample : IExamplesProvider<Guid> {

            public Guid GetExamples() => Guid.NewGuid();
        }
    }
}