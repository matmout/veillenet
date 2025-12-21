using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VeilleNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterSubscribers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if newsletter_subscribers table exists before creating
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'containsharp' 
                        AND table_name = 'newsletter_subscribers'
                    ) THEN
                        CREATE TABLE containsharp.newsletter_subscribers (
                            id SERIAL PRIMARY KEY,
                            email VARCHAR(255) NOT NULL,
                            subscribed_at TIMESTAMP WITH TIME ZONE NOT NULL,
                            unsubscribed_at TIMESTAMP WITH TIME ZONE,
                            is_active BOOLEAN NOT NULL,
                            source VARCHAR(100) NOT NULL,
                            unsubscribe_reason VARCHAR(500),
                            email_sent_count INTEGER NOT NULL,
                            last_email_sent_at TIMESTAMP WITH TIME ZONE,
                            created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );
                    END IF;
                END $$;
            ");

            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS IX_newsletter_subscribers_email 
                ON containsharp.newsletter_subscribers(email);
                
                CREATE INDEX IF NOT EXISTS IX_newsletter_subscribers_is_active 
                ON containsharp.newsletter_subscribers(is_active);
                
                CREATE INDEX IF NOT EXISTS IX_newsletter_subscribers_subscribed_at 
                ON containsharp.newsletter_subscribers(subscribed_at);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "newsletter_subscribers",
                schema: "containsharp");
        }
    }
}
