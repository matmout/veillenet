using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VeilleNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDominantThemes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dominant_themes",
                schema: "containsharp",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    generation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    theme = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    rationale = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dominant_themes", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dominant_themes_generation_date",
                schema: "containsharp",
                table: "dominant_themes",
                column: "generation_date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dominant_themes",
                schema: "containsharp");
        }
    }
}
