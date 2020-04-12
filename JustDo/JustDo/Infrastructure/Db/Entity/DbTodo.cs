using System;

using JustDo.Models;

namespace JustDo.Infrastructure.Db.Entity {
    public class DbTodo {
        public Guid Id { get; set; }
        public DateTime DueDateUtc { get; set; }
        public string Name { get; set; }
        public bool Done { get; set; }
        public TodoPriority Priority { get; set; }
    }
}