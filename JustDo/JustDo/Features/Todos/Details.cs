using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation;

using JustDo.Infrastructure.Db.Entity;
using JustDo.Infrastructure.Errors;
using JustDo.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SmartAnalyzers.CSharpExtensions.Annotations;

namespace JustDo.Features.Todos {
    public static class Details {
        public class Handler : IRequestHandler<Query, TodoEnvelope> {
            private readonly TodoContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(TodoContext context, ILogger<Handler> logger) {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<TodoEnvelope> Handle(Query q, CancellationToken ct) {
                if (q is null) {
                    throw new ArgumentNullException(nameof(q));
                }

                var strategy = _context.Database.CreateExecutionStrategy();

                var todo = await strategy.ExecuteAsync(async (ct) => {
                    var existingTodo = await _context.Todos.SingleOrDefaultAsync(x => x.Id == q.Id, ct).ConfigureAwait(false);

                    if (existingTodo is null) {
                        throw new RestException(
                            System.Net.HttpStatusCode.NoContent,
                            new ErrorResponse[] {
                                new ErrorResponse {
                                    Error = ErrorCodes.E_OBJECT_NOT_FOUND,
                                    Message = $"Cannot find todo with ID [{q.Id}]"
                                }
                            }
                        );
                    }

                    return new TodoEnvelope {
                        Todo = new Models.Todo {
                            Done = existingTodo.Done,
                            DueDateUtc = existingTodo.DueDateUtc,
                            Id = existingTodo.Id,
                            Name = existingTodo.Name,
                            Priority = existingTodo.Priority
                        }
                    };
                }, ct).ConfigureAwait(false);

                return todo;
            }
        }

        [InitOnly]
        [DataContract]
        public class Query : IRequest<TodoEnvelope> {

            [DataMember(Name = "id")]
            public Guid Id { get; set; }
        }

        public class QueryValidator : AbstractValidator<Query> {

            public QueryValidator() {
                RuleFor(x => x.Id).NotEmpty();
            }
        }
    }
}