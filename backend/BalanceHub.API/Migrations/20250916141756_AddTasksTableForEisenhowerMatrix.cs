using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalanceHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTasksTableForEisenhowerMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Urgency = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    Importance = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    MatrixType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "do"),
                    CalculatedPriority = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    EstimatedHours = table.Column<double>(type: "float", nullable: false, defaultValue: 1.0),
                    ActualHours = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    EffortLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "medium"),
                    Deadline = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "todo"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TimePressure = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    PriorityDecay = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    RescheduleCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTask",
                columns: table => new
                {
                    DependenciesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DependentTasksId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTask", x => new { x.DependenciesId, x.DependentTasksId });
                    table.ForeignKey(
                        name: "FK_TaskTask_Tasks_DependenciesId",
                        column: x => x.DependenciesId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTask_Tasks_DependentTasksId",
                        column: x => x.DependentTasksId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CalculatedPriority",
                table: "Tasks",
                column: "CalculatedPriority");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Category",
                table: "Tasks",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Deadline",
                table: "Tasks",
                column: "Deadline");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_MatrixType",
                table: "Tasks",
                column: "MatrixType");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status",
                table: "Tasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId",
                table: "Tasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_IsDeleted",
                table: "Tasks",
                columns: new[] { "UserId", "IsDeleted" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_Status",
                table: "Tasks",
                columns: new[] { "UserId", "Status" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTask_DependentTasksId",
                table: "TaskTask",
                column: "DependentTasksId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskTask");

            migrationBuilder.DropTable(
                name: "Tasks");
        }
    }
}
