-- Триггер на запрет удаления пользователя, если за ним числятся приёмы
CREATE TRIGGER trgCheckUserDeletion ON Пользователь
AFTER DELETE
AS
IF EXISTS (
    SELECT 1 FROM Приём 
    WHERE КодПациента IN (SELECT Код FROM deleted) 
       OR КодВрача IN (SELECT Код FROM deleted)
)
BEGIN
    RAISERROR('Нельзя удалить пользователя, так как он фигурирует в записях на приём.', 16, 1)
    ROLLBACK TRANSACTION
END
GO