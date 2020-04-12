using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation;

using JustDo.Infrastructure.Db.Entity;
using JustDo.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SmartAnalyzers.CSharpExtensions.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace JustDo.Features.Todos {
    public static class Create {
        public class Handler : IRequestHandler<Command, Guid> {
            private readonly TodoContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(TodoContext context, ILogger<Handler> logger) {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<Guid> Handle(Command c, CancellationToken ct) {
                if (c is null) {
                    throw new ArgumentNullException(nameof(c));
                }

                var strategy = _context.Database.CreateExecutionStrategy();

                var id = await strategy.ExecuteAsync(async (ct) => {
                    var newTodoId = Guid.NewGuid();

                    var newTodo = new DbTodo {
                        Done = false,
                        DueDateUtc = c.DueDate,
                        Id = newTodoId,
                        Name = c.Name,
                        Priority = c.Priority.Value
                    };

                    _context.Add(newTodo);

                    await _context.SaveChangesAsync(ct).ConfigureAwait(false);

                    return newTodoId;
                }, ct).ConfigureAwait(false);

                return id;
            }
        }

        [InitOnly]
        [DataContract]
        public class Command : IRequest<Guid> {

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "dueDate")]
            public DateTime DueDate { get; set; }

            [DataMember(Name = "priority")]
            public TodoPriority? Priority { get; set; } = TodoPriority.NOT_SET;
        }

        public class CommandValidator : AbstractValidator<Command> {

            public CommandValidator() {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.DueDate).LessThan(DateTime.MaxValue).GreaterThan(DateTime.MinValue).Must(x=>x.Kind == DateTimeKind.Utc);
            }
        }

        public class ModelExample : IExamplesProvider<Command> {

            public Command GetExamples() => new Command {
                DueDate = DateTime.Parse("2020-08-11T15:48:13Z"),
                Name = "MyToDo",
                Priority = TodoPriority.MEDIUM
            };
        }
    }
}