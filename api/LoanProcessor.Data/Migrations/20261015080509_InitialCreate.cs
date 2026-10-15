using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanProcessor.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoanOwnershipPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanOwnershipPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanPeriodBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanOwnershipPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Rpb = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPeriodBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanPeriodBalances_LoanOwnershipPeriods_LoanOwnershipPeriodId",
                        column: x => x.LoanOwnershipPeriodId,
                        principalTable: "LoanOwnershipPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanOwnershipPeriods_LoanId_Owner_StartDate",
                table: "LoanOwnershipPeriods",
                columns: new[] { "LoanId", "Owner", "StartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanPeriodBalances_LoanOwnershipPeriodId_PeriodDate",
                table: "LoanPeriodBalances",
                columns: new[] { "LoanOwnershipPeriodId", "PeriodDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanPeriodBalances");

            migrationBuilder.DropTable(
                name: "LoanOwnershipPeriods");
        }
    }
}
