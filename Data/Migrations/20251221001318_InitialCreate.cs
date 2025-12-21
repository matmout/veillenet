using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VeilleNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "containsharp");

            // Check if news_articles table exists before creating
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'containsharp' 
                        AND table_name = 'news_articles'
                    ) THEN
                        CREATE TABLE containsharp.news_articles (
                            id SERIAL PRIMARY KEY,
                            title VARCHAR(500) NOT NULL,
                            url VARCHAR(1000) NOT NULL,
                            summary TEXT NOT NULL,
                            published_date TIMESTAMP WITH TIME ZONE NOT NULL,
                            author VARCHAR(200) NOT NULL,
                            source VARCHAR(100) NOT NULL,
                            category VARCHAR(100) NOT NULL,
                            image VARCHAR(1000) NOT NULL,
                            created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
                        );
                    END IF;
                END $$;
            ");

            // Check if ai_summaries table exists before creating
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'containsharp' 
                        AND table_name = 'ai_summaries'
                    ) THEN
                        CREATE TABLE containsharp.ai_summaries (
                            id SERIAL PRIMARY KEY,
                            title VARCHAR(500) NOT NULL,
                            url VARCHAR(1000) NOT NULL,
                            source VARCHAR(100) NOT NULL,
                            published_date TIMESTAMP WITH TIME ZONE NOT NULL,
                            summary TEXT NOT NULL,
                            ai_generated BOOLEAN NOT NULL,
                            summary_date TIMESTAMP WITH TIME ZONE NOT NULL,
                            created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            news_article_id INTEGER NULL
                        );
                    END IF;
                END $$;
            ");

            // Add foreign key if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE constraint_name = 'FK_ai_summaries_news_articles_news_article_id'
                        AND table_schema = 'containsharp'
                    ) THEN
                        ALTER TABLE containsharp.ai_summaries
                        ADD CONSTRAINT FK_ai_summaries_news_articles_news_article_id
                        FOREIGN KEY (news_article_id)
                        REFERENCES containsharp.news_articles(id)
                        ON DELETE SET NULL;
                    END IF;
                END $$;
            ");

            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_ai_summaries_news_article_id 
                ON containsharp.ai_summaries(news_article_id);
                
                CREATE UNIQUE INDEX IF NOT EXISTS IX_ai_summaries_url 
                ON containsharp.ai_summaries(url);
                
                CREATE INDEX IF NOT EXISTS IX_ai_summaries_published_date 
                ON containsharp.ai_summaries(published_date);
                
                CREATE INDEX IF NOT EXISTS IX_ai_summaries_source 
                ON containsharp.ai_summaries(source);
                
                CREATE INDEX IF NOT EXISTS IX_ai_summaries_summary_date 
                ON containsharp.ai_summaries(summary_date);
                
                CREATE INDEX IF NOT EXISTS IX_news_articles_category 
                ON containsharp.news_articles(category);
                
                CREATE INDEX IF NOT EXISTS IX_news_articles_published_date 
                ON containsharp.news_articles(published_date);
                
                CREATE INDEX IF NOT EXISTS IX_news_articles_source 
                ON containsharp.news_articles(source);
                
                CREATE UNIQUE INDEX IF NOT EXISTS IX_news_articles_url 
                ON containsharp.news_articles(url);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_summaries",
                schema: "containsharp");

            migrationBuilder.DropTable(
                name: "news_articles",
                schema: "containsharp");
        }
    }
}
