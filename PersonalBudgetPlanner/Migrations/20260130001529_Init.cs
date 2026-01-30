using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PersonalBudgetPlanner.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    AnnualIncome = table.Column<decimal>(type: "TEXT", nullable: false),
                    AnnualWorkHours = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRecurring = table.Column<bool>(type: "INTEGER", nullable: false),
                    Recurrence = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Absences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    HoursMissed = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Absences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Absences_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Mat", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111112"), "Hus & drift", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111113"), "Transport", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111114"), "Fritid", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111115"), "Barn", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111116"), "Streaming-tjänster", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111117"), "SaaS-produkter", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111118"), "Hälsa & Sjukvård", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111119"), "Skulder & Räntor", 1 },
                    { new Guid("11111111-1111-1111-1111-111111111120"), "Övrigt (Utgift)", 1 },
                    { new Guid("22222222-2222-2222-2222-222222222221"), "Lön", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Bidrag", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222223"), "Hobbyverksamhet", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222224"), "Återbäring/Bonus", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222225"), "Gåvor", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222226"), "Övrigt (Inkomst)", 0 }
                });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "Id", "AnnualIncome", "AnnualWorkHours", "Name" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), 0m, 0.0, "Standard User" });

            migrationBuilder.CreateIndex(
                name: "IX_Absences_UserId",
                table: "Absences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Absences");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
