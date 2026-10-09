-- Datos de prueba para el entorno local de docker compose. Idempotente.
-- SOLO desarrollo: no ejecutar en producción.

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM [PriceLists])
BEGIN
    INSERT INTO [PriceLists] ([Name], [Percent]) VALUES ('Lista 1', 0), ('Lista 2', 10), ('Lista 3', 25);
END;

-- Usuario: admin / Contraseña: admin
-- La contraseña está encriptada con el EncryptKey de appsettings.Development.json
-- (PasswordEncryptor.Encrypt). Si se cambia esa clave, hay que regenerar este valor.
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [Users] ([Username], [Name], [Password], [Active])
    VALUES ('admin', 'Administrador', 'sos9BvlABwlGw6n9ElgLoQ==', 1);
END;

SELECT
    (SELECT COUNT(*) FROM [PriceLists]) AS PriceLists,
    (SELECT COUNT(*) FROM [Users]) AS Users;
