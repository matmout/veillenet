-- Script de vérification des newsletters quotidiennes
-- Base de données : Trader
-- Schéma : containsharp

-- 1. Vérifier la contrainte unique sur newsletter_date
SELECT 
    conname AS constraint_name,
    contype AS constraint_type,
    pg_get_constraintdef(oid) AS constraint_definition
FROM pg_constraint
WHERE conrelid = 'containsharp.daily_newsletters'::regclass
  AND contype = 'u'; -- Contraintes UNIQUE

-- 2. Compter les newsletters
SELECT 
    COUNT(*) AS total_newsletters,
    COUNT(*) FILTER (WHERE is_sent = TRUE) AS sent_newsletters,
    COUNT(*) FILTER (WHERE is_sent = FALSE) AS pending_newsletters,
    MIN(newsletter_date) AS first_newsletter_date,
    MAX(newsletter_date) AS last_newsletter_date
FROM containsharp.daily_newsletters;

-- 3. Voir les 10 dernières newsletters
SELECT 
    newsletter_date,
    subject,
    summary_count,
    recipient_count,
    CASE WHEN is_sent THEN '? Envoyée' ELSE '? En attente' END AS status,
    sent_at AT TIME ZONE 'Europe/Paris' AS sent_at_paris,
    created_at AT TIME ZONE 'Europe/Paris' AS created_at_paris
FROM containsharp.daily_newsletters
ORDER BY newsletter_date DESC
LIMIT 10;

-- 4. Statistiques par mois
SELECT 
    DATE_TRUNC('month', newsletter_date) AS month,
    COUNT(*) AS newsletters_count,
    COUNT(*) FILTER (WHERE is_sent = TRUE) AS sent_count,
    SUM(summary_count) AS total_summaries,
    SUM(recipient_count) AS total_recipients,
    ROUND(AVG(recipient_count), 0) AS avg_recipients
FROM containsharp.daily_newsletters
WHERE is_sent = TRUE
GROUP BY DATE_TRUNC('month', newsletter_date)
ORDER BY month DESC;

-- 5. Vérifier qu'il n'y a pas de doublons (devrait retourner 0 lignes)
SELECT 
    newsletter_date,
    COUNT(*) AS count
FROM containsharp.daily_newsletters
GROUP BY newsletter_date
HAVING COUNT(*) > 1;

-- 6. Newsletters avec le plus de destinataires
SELECT 
    newsletter_date,
    subject,
    recipient_count,
    summary_count,
    sent_at AT TIME ZONE 'Europe/Paris' AS sent_at_paris
FROM containsharp.daily_newsletters
WHERE is_sent = TRUE
ORDER BY recipient_count DESC
LIMIT 5;

-- 7. Taille du contenu HTML/Text
SELECT 
    newsletter_date,
    LENGTH(html_content) AS html_size_bytes,
    LENGTH(text_content) AS text_size_bytes,
    ROUND(LENGTH(html_content) / 1024.0, 2) AS html_size_kb,
    ROUND(LENGTH(text_content) / 1024.0, 2) AS text_size_kb
FROM containsharp.daily_newsletters
ORDER BY newsletter_date DESC
LIMIT 5;

-- 8. Newsletter d'aujourd'hui (heure de Paris)
SELECT 
    newsletter_date,
    subject,
    summary_count,
    is_sent,
    CASE 
        WHEN is_sent THEN sent_at AT TIME ZONE 'Europe/Paris'
        ELSE NULL
    END AS sent_at_paris
FROM containsharp.daily_newsletters
WHERE newsletter_date = CURRENT_DATE AT TIME ZONE 'Europe/Paris';
