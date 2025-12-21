-- Script de vérification après le premier run de l'application
-- Base de données : Trader
-- Schéma : containsharp

-- 1. Compter les enregistrements
SELECT 
    (SELECT COUNT(*) FROM containsharp.news_articles) AS total_articles,
    (SELECT COUNT(*) FROM containsharp.ai_summaries) AS total_summaries,
    (SELECT COUNT(*) FROM containsharp.ai_summaries WHERE news_article_id IS NOT NULL) AS linked_summaries;

-- 2. Voir les 5 derniers articles
SELECT 
    id,
    title,
    source,
    published_date AT TIME ZONE 'Europe/Paris' AS published_date_paris,
    created_at AT TIME ZONE 'Europe/Paris' AS created_at_paris
FROM containsharp.news_articles
ORDER BY created_at DESC
LIMIT 5;

-- 3. Voir les 5 derniers résumés avec leur lien
SELECT 
    s.id,
    s.title,
    s.news_article_id,
    CASE WHEN s.news_article_id IS NOT NULL THEN '? Linked' ELSE '? Orphan' END AS status,
    s.summary_date AT TIME ZONE 'Europe/Paris' AS summary_date_paris
FROM containsharp.ai_summaries s
ORDER BY s.created_at DESC
LIMIT 5;

-- 4. Jointure pour voir les articles avec leurs résumés
SELECT 
    n.id AS article_id,
    n.title AS article_title,
    n.source,
    s.id AS summary_id,
    CASE WHEN s.id IS NOT NULL THEN '? Has AI Summary' ELSE '? No Summary' END AS has_summary
FROM containsharp.news_articles n
LEFT JOIN containsharp.ai_summaries s ON n.id = s.news_article_id
ORDER BY n.created_at DESC
LIMIT 10;
