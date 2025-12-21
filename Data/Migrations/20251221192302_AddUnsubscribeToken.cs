using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeilleNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnsubscribeToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM information_schema.columns 
                        WHERE table_schema = 'containsharp' 
                        AND table_name = 'newsletter_subscribers' 
                        AND column_name = 'unsubscribe_token'
                    ) THEN
                        ALTER TABLE containsharp.newsletter_subscribers
                        ADD COLUMN unsubscribe_token VARCHAR(128);
                    END IF;
                    
                    IF NOT EXISTS (
                        SELECT FROM information_schema.columns 
                        WHERE table_schema = 'containsharp' 
                        AND table_name = 'newsletter_subscribers' 
                        AND column_name = 'unsubscribe_token_expires_at'
                    ) THEN
                        ALTER TABLE containsharp.newsletter_subscribers
                        ADD COLUMN unsubscribe_token_expires_at TIMESTAMP WITH TIME ZONE;
                    END IF;
                END $$;
            ");

            // Create index if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_newsletter_subscribers_unsubscribe_token 
                ON containsharp.newsletter_subscribers(unsubscribe_token);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_newsletter_subscribers_unsubscribe_token",
                schema: "containsharp",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "unsubscribe_token",
                schema: "containsharp",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "unsubscribe_token_expires_at",
                schema: "containsharp",
                table: "newsletter_subscribers");
        }
    }
}
