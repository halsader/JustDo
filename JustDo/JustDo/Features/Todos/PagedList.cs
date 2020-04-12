using System;
using System.Data;
using System.Linq;
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
    public static class PagedList {
        public class Handler : IRequestHandler<Query, TodoPagedListEnvelope> {
            private readonly TodoContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(TodoContext context, ILogger<Handler> logger) {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<TodoPagedListEnvelope> Handle(Query q, CancellationToken ct) {
                if (q is null) {
                    throw new ArgumentNullException(nameof(q));
                }

                var strategy = _context.Database.CreateExecutionStrategy();

                var todo = await strategy.ExecuteAsync(async (ct) => {
                    using var t = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct).ConfigureAwait(false);

                    var dbTodos = FilterTodo.ApplyFilters(_context.Todos, q.Filters);
                    var totalCount = await dbTodos.CountAsync(ct).ConfigureAwait(false);

                    var pagedTodos = dbTodos.Skip(q.Page.Value * q.ItemsPerPage.Value).Take(q.ItemsPerPage.Value);

                    var todos = await pagedTodos
                        .Select(x => new Todo {
                            Done = x.Done,
                            DueDateUtc = x.DueDateUtc,
                            Id = x.Id,
                            Name = x.Name,
                            Priority = x.Priority
                        })
                        .ToListAsync(ct)
                        .ConfigureAwait(false);

                    await t.CommitAsync(ct).ConfigureAwait(false);

                    var todoGroups = OrderTodo.GroupAndOrder(todos, q.GroupOrder, q.TodoOrder);

                    return new TodoPagedListEnvelope {
                        TodoPaged = new Paged<System.Collections.Generic.SortedDictionary<DateTime, System.Collections.Generic.IReadOnlyCollection<Todo>>> {
                            Items = todoGroups,
                            ItemsPerPage = q.ItemsPerPage,
                            PageNum = q.Page,
                            TotalItems = totalCount
                        }
                    };
                }, ct).ConfigureAwait(false);

                return todo;
            }
        }

        [InitOnly]
        [DataContract]
        public class Query : IRequest<TodoPagedListEnvelope> {

            [DataMember(Name = "filters")]
            public TodoFilterCollection Filters { get; set; }

            [DataMember(Name = "groupOrder")]
            public Order GroupOrder { get; set; }

            [DataMember(Name = "todoOrder")]
            public Order[] TodoOrder { get; set; }

            [DataMember(Name = "page")]
            public int? Page { get; set; } = 1;

            [DataMember(Name = "itemsPerPage")]
            public int? ItemsPerPage { get; set; } = 25;
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