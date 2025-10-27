-- PostgreSQL Setup Script for Reacoda Molemi Database
-- Run this script in pgAdmin or psql command line

-- Create the main database
CREATE DATABASE "Reacoda_MolemiDB"
    WITH 
    OWNER = postgree
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_United States.1252'
    LC_CTYPE = 'English_United States.1252'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Connect to the database
\c "Reacoda_MolemiDB";

-- Grant necessary permissions to postgree user
GRANT ALL PRIVILEGES ON DATABASE "Reacoda_MolemiDB" TO postgree;
GRANT ALL PRIVILEGES ON SCHEMA public TO postgree;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgree;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgree;

-- Set default privileges for future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO postgree;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO postgree;

-- Verify database creation
SELECT datname FROM pg_database WHERE datname = 'Reacoda_MolemiDB';

-- Display connection information
SELECT 
    'Database: Reacoda_MolemiDB' as info,
    'Host: localhost' as host,
    'Port: 5432' as port,
    'User: postgree' as username,
    'Password: 12345' as password;
