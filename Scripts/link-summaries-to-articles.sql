-- Script pour lier les résumés IA existants aux articles de news
-- Base de données : Trader
-- Schéma : containsharp

-- 1. Mettre à jour les résumés IA pour les lier aux articles de news par URL
UPDATE containsharp.ai_summaries AS s
SET news_article_id = n.id
FROM containsharp.news_articles AS n
WHERE s.url = n.url
  AND s.news_article_id IS NULL;

-- 2. Vérifier les liens créés
SELECT 
    s.id AS summary_id,
    s.title AS summary_title,
    s.news_article_id,
    n.id AS article_id,
    n.title AS article_title
FROM containsharp.ai_summaries s
LEFT JOIN containsharp.news_articles n ON s.news_article_id = n.id
ORDER BY s.created_at DESC
LIMIT 20;

-- 3. Compter les résumés IA avec et sans lien
SELECT 
    COUNT(*) AS total_summaries,
    COUNT(news_article_id) AS linked_summaries,
    COUNT(*) - COUNT(news_article_id) AS unlinked_summaries
FROM containsharp.ai_summaries;

-- 4. Afficher les résumés sans article correspondant (orphelins)
SELECT 
    s.id,
    s.title,
    s.url,
    s.created_at
FROM containsharp.ai_summaries s
LEFT JOIN containsharp.news_articles n ON s.url = n.url
WHERE n.id IS NULL;
