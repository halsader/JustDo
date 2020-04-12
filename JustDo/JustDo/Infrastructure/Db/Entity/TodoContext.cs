using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

namespace JustDo.Infrastructure.Db.Entity {
    public class TodoContext : DbContext {

        public TodoContext(DbContextOptions<TodoContext> options) : base(options) {
        }

        public DbSet<DbTodo> Todos { get; set; }

        protected override void OnModelCreating([NotNull] ModelBuilder mb) {
            mb.Entity<DbTodo>().ToTable("todos");

            mb.Entity<DbTodo>().HasKey(k => k.Id);

            mb.Entity<DbTodo>().HasIndex(i => i.Name);
            mb.Entity<DbTodo>().HasIndex(i => i.DueDateUtc);
            mb.Entity<DbTodo>().HasIndex(i => i.Done);
            mb.Entity<DbTodo>().HasIndex(i => i.Name);

            mb.Entity<DbTodo>().Property(p => p.DueDateUtc).HasConversion(
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );
        }
    }
}