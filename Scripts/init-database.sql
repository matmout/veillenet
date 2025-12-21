-- Script d'initialisation PostgreSQL pour VeilleNet
-- Base de données : Trader
-- Schéma : containsharp
-- 
-- IMPORTANT: Ce fichier est pour PostgreSQL, pas SQL Server !
-- Visual Studio peut afficher des erreurs car il utilise un analyseur T-SQL.
-- Ces erreurs peuvent être ignorées - le script fonctionne correctement avec PostgreSQL.

-- Note: Quand ce script est exécuté via docker-entrypoint-initdb.d,
-- la base de données "Trader" est déjà créée via POSTGRES_DB

-- 1. Créer la base de données (si exécuté manuellement)
-- CREATE DATABASE "Trader";

-- 2. Se connecter à la base de données Trader (si en mode interactif)
-- \c Trader

-- 3. Créer le schéma containsharp
-- PostgreSQL syntax (not T-SQL!)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'containsharp') THEN
        CREATE SCHEMA containsharp;
        RAISE NOTICE 'Schema containsharp created successfully!';
    ELSE
        RAISE NOTICE 'Schema containsharp already exists.';
    END IF;
END
$$;

-- 4. Donner les permissions sur le schéma
-- PostgreSQL syntax
DO $$
BEGIN
    EXECUTE 'GRANT ALL ON SCHEMA containsharp TO postgres';
    RAISE NOTICE 'Permissions granted on schema containsharp.';
END
$$;

-- Note: Les tables seront créées automatiquement par Entity Framework 
-- lors du premier démarrage de l'application ou via 'dotnet ef database update'

-- Pour vérifier les migrations appliquées après le démarrage de l'app :
-- SELECT * FROM containsharp."__EFMigrationsHistory";
