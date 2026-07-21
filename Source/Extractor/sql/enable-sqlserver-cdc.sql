-- Sample script to enable SQL Server CDC for the Prologue Engine change-capture tool.
-- The extractor normally does this itself at startup (SqlServer[].EnableChangeDataCapture,
-- on by default), so this script is the fallback for when it cannot: the connecting
-- account is not sysadmin, or a DBA prefers to enable CDC deliberately.
--
-- Run against the target database. Requires sysadmin and a running SQL Server Agent
-- (start the container with MSSQL_AGENT_ENABLED=true).
--
-- Prologue Engine only reads CDC metadata (table + changed column names per transaction);
-- it never reads row data.

-- 1. Enable CDC at the database level.
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = DB_NAME() AND is_cdc_enabled = 1)
BEGIN
    EXEC sys.sp_cdc_enable_db;
END
GO

-- 2. Enable CDC on each table to watch. Repeat per table.
--    @role_name = NULL grants change access to all members of the sysadmin/db_owner roles.
IF NOT EXISTS (SELECT 1 FROM cdc.change_tables ct
               JOIN sys.tables t ON ct.source_object_id = t.object_id
               WHERE t.name = 'Customers')
BEGIN
    EXEC sys.sp_cdc_enable_table
        @source_schema = N'dbo',
        @source_name   = N'Customers',
        @role_name     = NULL,
        @supports_net_changes = 0;
END
GO
