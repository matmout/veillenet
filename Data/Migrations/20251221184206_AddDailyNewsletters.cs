using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VeilleNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyNewsletters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if daily_newsletters table exists before creating
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'containsharp' 
                        AND table_name = 'daily_newsletters'
                    ) THEN
                        CREATE TABLE containsharp.daily_newsletters (
                            id SERIAL PRIMARY KEY,
                            newsletter_date DATE NOT NULL,
                            subject VARCHAR(500) NOT NULL,
                            html_content TEXT NOT NULL,
                            text_content TEXT NOT NULL,
                            summary_count INTEGER NOT NULL,
                            recipient_count INTEGER NOT NULL,
                            sent_at TIMESTAMP WITH TIME ZONE,
                            is_sent BOOLEAN NOT NULL,
                            created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );
                    END IF;
                END $$;
            ");

            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS idx_daily_newsletter_unique_date 
                ON containsharp.daily_newsletters(newsletter_date);
                
                CREATE INDEX IF NOT EXISTS IX_daily_newsletters_is_sent 
                ON containsharp.daily_newsletters(is_sent);
                
                CREATE INDEX IF NOT EXISTS IX_daily_newsletters_sent_at 
                ON containsharp.daily_newsletters(sent_at);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_newsletters",
                schema: "containsharp");
        }
    }
}
