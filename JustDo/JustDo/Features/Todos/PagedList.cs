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

using Swashbuckle.AspNetCore.Filters;

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

        public class ModelExample : IExamplesProvider<Query> {

            public Query GetExamples() => new Query {
                Filters = new TodoFilterCollection {
                    Done = TodoDoneOptions.ALL,
                    DueDate = new DateRangeFilter {
                        From = DateTime.Parse("1980-12-11T00:00:00Z"),
                        To = DateTime.Parse("2020-12-01T00:00:00Z"),
                    },
                    Name = "todo"
                },
                GroupOrder = new Order {
                    Direction = Order.DirectionEnum.Asc,
                    Field = "dueDateUtc"
                },
                TodoOrder = new Order[] {
                    new Order {
                        Direction = Order.DirectionEnum.Asc,
                        Field = "name"
                    },
                    new Order {
                        Direction = Order.DirectionEnum.Desc,
                        Field = "done"
                    }
                },
                ItemsPerPage = 100,
                Page = 1
            };
        }

        public class ResponseExample : IExamplesProvider<TodoPagedListEnvelope> {

            public TodoPagedListEnvelope GetExamples() => new TodoPagedListEnvelope {
                TodoPaged = new Paged<System.Collections.Generic.SortedDictionary<DateTime, System.Collections.Generic.IReadOnlyCollection<Todo>>> {
                    Items = new System.Collections.Generic.SortedDictionary<DateTime, System.Collections.Generic.IReadOnlyCollection<Todo>> {
                    { DateTime.Parse("2020-11-12T00:00:00"),
                      new Todo[] {
                        new Todo {
                            Done = true,
                            DueDateUtc = DateTime.Parse("2020-11-12T00:00:00"),
                            Id = Guid.NewGuid(),
                            Name = "MyTodo1",
                            Priority = TodoPriority.HIGH
                        },
                        new Todo {
                            Done = true,
                            DueDateUtc = DateTime.Parse("2020-11-12T00:00:00"),
                            Id = Guid.NewGuid(),
                            Name = "MyTodo2",
                            Priority = TodoPriority.MEDIUM
                        },
                        new Todo {
                            Done = false,
                            DueDateUtc = DateTime.Parse("2020-11-12T00:00:00"),
                            Id = Guid.NewGuid(),
                            Name = "MyTodo3",
                            Priority = TodoPriority.NOT_SET
                        },
                      }
                    },
                    { DateTime.Parse("2020-08-15T00:00:00"),
                      new Todo[] {
                        new Todo {
                            Done = true,
                            DueDateUtc = DateTime.Parse("2020-08-15T00:00:00"),
                            Id = Guid.NewGuid(),
                            Name = "MyTodo4",
                            Priority = TodoPriority.HIGH
                        },
                        new Todo {
                            Done = true,
                            DueDateUtc = DateTime.Parse("2020-08-15T00:00:00"),
                            Id = Guid.NewGuid(),
                            Name = "MyTodo5",
                            Priority = TodoPriority.MEDIUM
                        },
                        new Todo {
                            Done = false,
                            DueDateUtc = DateTime.Parse("2020-08-15T00:00:00"),
                            Id = Guid.NewGuid(),
                            Name = "MyTodo6",
                            Priority = TodoPriority.NOT_SET
                        },
                      }
                    }
                    },
                    ItemsPerPage = 2,
                    PageNum = 1,
                    TotalItems = 500
                }
            };
        }
    }
}