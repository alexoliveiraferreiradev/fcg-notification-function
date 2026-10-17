using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fcg.Notification.Function.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_CreatedAt",
                table: "ProcessedMessages",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedMessages");
        }
    }
}
