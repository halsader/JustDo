using System.Linq;

using JustDo.Infrastructure.Db.Entity;
using JustDo.Models;

using Microsoft.EntityFrameworkCore;

namespace JustDo.Features.Todos.QueryHelpers {
    public static class FilterTodo {

        public static IQueryable<DbTodo> ApplyFilters(IQueryable<DbTodo> todos, TodoFilterCollection filters) {
            if (filters != default) {
                if (filters.DueDate != default) {
                    if (filters.DueDate.From.HasValue) {
                        todos = todos.Where(x => x.DueDateUtc >= filters.DueDate.From.Value.Date);
                    }

                    if (filters.DueDate.To.HasValue) {
                        todos = todos.Where(x => x.DueDateUtc <= filters.DueDate.To.Value.Date);
                    }
                }

                if (!string.IsNullOrEmpty(filters.Name)) {
                    todos = todos.Where(x => EF.Functions.ILike(x.Name, $"%{filters.Name}%"));
                }

                switch (filters.Done) {
                    case TodoDoneOptions.DONE:
                        todos = todos.Where(x => x.Done);
                        break;

                    case TodoDoneOptions.NOT_DONE:
                        todos = todos.Where(x => !x.Done);
                        break;
                }
            }

            return todos;
        }
    }
}