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

namespace JustDo.Features.Todos {
    public static class Update {
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

                    if (!string.IsNullOrEmpty(c.Name) && !c.Name.Equals(existingTodo.Name, StringComparison.Ordinal)) {
                        existingTodo.Name = c.Name;
                    }

                    if (c.Done.HasValue && c.Done.Value != existingTodo.Done) {
                        existingTodo.Done = c.Done.Value;
                    }

                    if (c.DueDate.HasValue) {
                        var dueDate = c.DueDate.Value;

                        if (dueDate.Kind != DateTimeKind.Utc) {
                            dueDate = dueDate.ToUniversalTime();
                        }

                        if (existingTodo.DueDateUtc != dueDate) {
                            existingTodo.DueDateUtc = dueDate;
                        }
                    }

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

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "dueDate")]
            public DateTime? DueDate { get; set; }

            [DataMember(Name = "done")]
            public bool? Done { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command> {

            public CommandValidator() {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.DueDate).Must(x=>x.Value.Kind == DateTimeKind.Utc).When(x=>x.DueDate.HasValue);
            }
        }
    }
}