using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VeilleNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyBriefing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_briefings",
                schema: "containsharp",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    briefing_date = table.Column<DateOnly>(type: "date", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    article_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_briefings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_daily_briefings_unique_date",
                schema: "containsharp",
                table: "daily_briefings",
                column: "briefing_date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_briefings",
                schema: "containsharp");
        }
    }
}
