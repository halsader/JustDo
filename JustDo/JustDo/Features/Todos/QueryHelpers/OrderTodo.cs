using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Dynamic.Core;

using JustDo.Models;

namespace JustDo.Features.Todos.QueryHelpers {
    public class AscDateComparer : IComparer<DateTime> {

        public int Compare([AllowNull] DateTime x, [AllowNull] DateTime y) => x.CompareTo(y);
    }

    public class DescDateComparer : IComparer<DateTime> {

        public int Compare([AllowNull] DateTime x, [AllowNull] DateTime y) => y.CompareTo(x);
    }

    public static class OrderTodo {

        public static SortedDictionary<DateTime, IReadOnlyCollection<Todo>> GroupAndOrder(IReadOnlyCollection<Todo> todos, Order groupOrder, IReadOnlyCollection<Order> todoOrder) {
            var groupedTodos = todos.GroupBy(x => x.DueDateUtc).ToDictionary(x => x.Key, x => ApplyTodoOrder(x.ToArray(), todoOrder));

            if (groupOrder != default && groupOrder.Field.Equals("DUEDATEUTC", StringComparison.InvariantCultureIgnoreCase)) {
                return groupOrder.Direction switch
                {
                    Order.DirectionEnum.Asc => new SortedDictionary<DateTime, IReadOnlyCollection<Todo>>(groupedTodos, new AscDateComparer()),
                    _ => new SortedDictionary<DateTime, IReadOnlyCollection<Todo>>(groupedTodos, new DescDateComparer())
                };
            }

            return new SortedDictionary<DateTime, IReadOnlyCollection<Todo>>(groupedTodos, new DescDateComparer());
        }

        private static IReadOnlyCollection<Todo> ApplyTodoOrder(IReadOnlyCollection<Todo> todos, IReadOnlyCollection<Order> orderCollection) {
            IReadOnlyCollection<Todo> result = null;

            if (todos != default) {
                if (orderCollection != default && orderCollection.Count > 0) {
                    var orderBuilder = new List<string>();
                    foreach (var order in orderCollection) {
                        var dir = order.Direction == default ?
                            "DESC" :
                            order.Direction == Order.DirectionEnum.Asc ?
                            "ASC" :
                            "DESC";

                        switch (order.Field.ToUpperInvariant()) {
                            case "NAME":
                                orderBuilder.Add($"{nameof(Todo.Name)} {dir}");
                                break;

                            case "DONE":
                                orderBuilder.Add($"{nameof(Todo.Done)} {dir}");
                                break;

                            case "DUEDATEUTC":
                                orderBuilder.Add($"{nameof(Todo.DueDateUtc)} {dir}");
                                break;

                            default:
                                continue;
                        }

                        if (orderBuilder.Count == 0) {
                            result = todos.OrderBy(x => x.Done).ThenBy(x => x.DueDateUtc).ThenBy(x => x.Name).ToArray();
                        } else {
                            var orderStr = string.Join(",", orderBuilder);
                            result = todos.AsQueryable().OrderBy(orderStr).ToArray();
                        }
                    }
                } else {
                    result = todos.OrderBy(x => x.Done).ThenBy(x => x.DueDateUtc).ThenBy(x => x.Name).ToArray();
                }
            }

            return result;
        }
    }
}