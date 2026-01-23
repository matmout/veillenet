using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeilleNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterConfirmationLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "confirmation_token",
                schema: "containsharp",
                table: "newsletter_subscribers",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "confirmation_token_expires_at",
                schema: "containsharp",
                table: "newsletter_subscribers",
                type: "timestamp with time zone",
                nullable: true);

            // Ensure existing subscribers are preserved and treated as confirmed (no token needed)
            // Existing accounts remain active (or inactive if unsubscribed) without needing new confirmation.
            migrationBuilder.Sql("UPDATE \"containsharp\".\"newsletter_subscribers\" SET \"confirmation_token\" = NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confirmation_token",
                schema: "containsharp",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "confirmation_token_expires_at",
                schema: "containsharp",
                table: "newsletter_subscribers");
        }
    }
}
