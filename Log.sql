USE Policlinnic;
GO

-- 1. Добавляем колонку "В архиве" (если её нет)
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsDeleted' AND Object_ID = Object_ID(N'Пользователь'))
BEGIN
    ALTER TABLE Пользователь ADD IsDeleted BIT DEFAULT 0 NOT NULL;
END
GO

-- 2. СОЗДАЕМ ПРОЦЕДУРУ CreateLog (Генератор логов)
CREATE OR ALTER PROCEDURE CreateLog @TableName VARCHAR(256)
AS
BEGIN
    DECLARE @LogTableName VARCHAR(260) = @TableName + 'Log'
    DECLARE @TriggerName VARCHAR(260) = 'trg_' + @TableName + '_Audit'
    DECLARE @sql NVARCHAR(MAX)

    -- Удаляем старые версии, если есть
    IF OBJECT_ID(@LogTableName, 'U') IS NOT NULL 
    BEGIN
        SET @sql = 'DROP TABLE ' + @LogTableName
        EXEC (@sql)
    END
    
    IF OBJECT_ID(@TriggerName, 'TR') IS NOT NULL
    BEGIN
        SET @sql = 'DROP TRIGGER ' + @TriggerName
        EXEC (@sql)
    END

    -- Собираем колонки
    DECLARE @Columns NVARCHAR(MAX) = ''
    
    SELECT @Columns = @Columns + ', ' + c.name + ' ' + t.name + 
            CASE WHEN t.name IN ('char', 'varchar', 'nchar', 'nvarchar') 
                THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR) END + ')' 
                ELSE '' END + ' NULL'
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID(@TableName)
    ORDER BY c.column_id

    -- Создаем таблицу логов
    SET @sql = 'CREATE TABLE ' + @LogTableName + ' ('
             + 'idLog INT IDENTITY(1,1) PRIMARY KEY, '
             + 'typeLog CHAR(1), '
             + 'dateLog DATETIME DEFAULT GETDATE(), '
             + 'userLog VARCHAR(100) DEFAULT SYSTEM_USER, '
             + 'hostLog VARCHAR(100) DEFAULT HOST_NAME()'
             + @Columns
             + ')'
    
    PRINT 'Creating Table: ' + @LogTableName
    EXEC (@sql)

    -- Собираем список колонок для вставки
    DECLARE @ColList NVARCHAR(MAX) = ''
    SELECT @ColList = @ColList + ', ' + name 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(@TableName) 
    ORDER BY column_id

    SET @ColList = SUBSTRING(@ColList, 3, LEN(@ColList))

    -- Создаем триггер
    SET @sql = 'CREATE TRIGGER ' + @TriggerName + ' ON ' + @TableName + '
    AFTER INSERT, UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        DECLARE @Type CHAR(1);
        
        IF EXISTS(SELECT * FROM inserted) AND EXISTS(SELECT * FROM deleted)
            SET @Type = ''U'' -- Update
        ELSE IF EXISTS(SELECT * FROM inserted)
            SET @Type = ''I'' -- Insert
        ELSE
            SET @Type = ''D'' -- Delete

        IF @Type IN (''I'', ''U'')
        BEGIN
            INSERT INTO ' + @LogTableName + ' (typeLog, ' + @ColList + ')
            SELECT @Type, ' + @ColList + ' FROM inserted
        END

        IF @Type = ''D''
        BEGIN
            INSERT INTO ' + @LogTableName + ' (typeLog, ' + @ColList + ')
            SELECT @Type, ' + @ColList + ' FROM deleted
        END
    END'

    PRINT 'Creating Trigger: ' + @TriggerName
    EXEC (@sql)
END
GO

-- 3. ЗАПУСКАЕМ ГЕНЕРАТОР ДЛЯ ПОЛЬЗОВАТЕЛЕЙ
EXEC CreateLog 'Пользователь';
GO

-- 4. СОЗДАЕМ VIEW ДЛЯ УДОБНОГО ЧТЕНИЯ (Исправлено)
CREATE OR ALTER VIEW ViewUserLogs AS
SELECT 
    l.idLog,
    CASE l.typeLog 
        WHEN 'I' THEN 'Создание'
        WHEN 'U' THEN 'Изменение' -- Сюда попадет и архивация (Soft Delete)
        WHEN 'D' THEN 'Удаление'
    END AS Operation,
    l.dateLog AS Date,
    l.userLog AS SystemUser,
    l.Логин,
    l.IsDeleted -- Показываем статус архива
FROM ПользовательLog l
GO