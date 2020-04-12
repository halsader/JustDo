using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JustDo.Infrastructure.Db.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "todos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DueDateUtc = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Done = table.Column<bool>(nullable: false),
                    Priority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_todos_Done",
                table: "todos",
                column: "Done");

            migrationBuilder.CreateIndex(
                name: "IX_todos_DueDateUtc",
                table: "todos",
                column: "DueDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_todos_Name",
                table: "todos",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "todos");
        }
    }
}
