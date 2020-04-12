using System;
using System.Collections.Generic;
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

using Swashbuckle.AspNetCore.Filters;

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
                }
            };
        }

        public class ResponseExample : IExamplesProvider<TodoListEnvelope> {

            public TodoListEnvelope GetExamples() => new TodoListEnvelope {
                TodoList = new SortedDictionary<DateTime, IReadOnlyCollection<Todo>> {
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
                }
            };
        }
    }
}