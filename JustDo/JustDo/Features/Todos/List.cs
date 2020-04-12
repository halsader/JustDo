using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation;

using JustDo.Features.Todos.QueryHelpers;
using JustDo.Infrastructure.Db.Entity;
using JustDo.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SmartAnalyzers.CSharpExtensions.Annotations;

namespace JustDo.Features.Todos {
    public static class List {
        public class Handler : IRequestHandler<Query, TodoListEnvelope> {
            private readonly TodoContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(TodoContext context, ILogger<Handler> logger) {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<TodoListEnvelope> Handle(Query q, CancellationToken ct) {
                if (q is null) {
                    throw new ArgumentNullException(nameof(q));
                }

                var strategy = _context.Database.CreateExecutionStrategy();

                var todo = await strategy.ExecuteAsync(async (ct) => {
                    var dbTodos = FilterTodo.ApplyFilters(_context.Todos, q.Filters);

                    var todos = await dbTodos
                        .Select(x => new Todo {
                            Done = x.Done,
                            DueDateUtc = x.DueDateUtc,
                            Id = x.Id,
                            Name = x.Name,
                            Priority = x.Priority
                        })
                        .ToListAsync(ct)
                        .ConfigureAwait(false);

                    var todoGroups = OrderTodo.GroupAndOrder(todos, q.GroupOrder, q.TodoOrder);

                    return new TodoListEnvelope {
                        TodoList = todoGroups
                    };
                }, ct).ConfigureAwait(false);

                return todo;
            }
        }

        [InitOnly]
        [DataContract]
        public class Query : IRequest<TodoListEnvelope> {

            [DataMember(Name = "filters")]
            public TodoFilterCollection Filters { get; set; }

            [DataMember(Name = "groupOrder")]
            public Order GroupOrder { get; set; }

            [DataMember(Name = "todoOrder")]
            public Order[] TodoOrder { get; set; }
        }

        public class QueryValidator : AbstractValidator<Query> {

            public QueryValidator() {
                RuleFor(x => x.Filters.DueDate.From.Value)
                    .Must(x => x.Kind == DateTimeKind.Utc)
                    .When(x =>
                        x.Filters != default
                        && x.Filters.DueDate != default
                        && x.Filters.DueDate.From.HasValue);
                RuleFor(x => x.Filters.DueDate.To.Value)
                    .Must(x => x.Kind == DateTimeKind.Utc)
                    .When(x =>
                        x.Filters != default
                        && x.Filters.DueDate != default
                        && x.Filters.DueDate.To.HasValue);

                RuleFor(x => x.GroupOrder.Field).NotEmpty().When(x => x.GroupOrder != default);
                RuleFor(x => x.GroupOrder.Direction).NotEmpty().When(x => x.GroupOrder != default);

                RuleForEach(x => x.TodoOrder).Must(x => !string.IsNullOrEmpty(x.Field)).When(x => x.TodoOrder?.Length > 0);
                RuleForEach(x => x.TodoOrder).Must(x => x.Direction.HasValue).When(x => x.TodoOrder?.Length > 0);
            }
        }
    }
}