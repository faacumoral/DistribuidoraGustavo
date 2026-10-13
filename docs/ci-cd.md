# CI/CD

Dos workflows en `.github/workflows/`:

| Workflow | Cuándo | Qué hace |
|---|---|---|
| `ci.yml` | Pull request | `dotnet build` de la solución en Release (no hay tests) |
| `deploy.yml` | Push a `main` (o manual) | `dotnet publish` del proyecto `API` y deploy del resultado al App Service |

`dotnet publish` del proyecto `API` incluye el frontend Blazor en `wwwroot`, así que un solo App Service sirve todo (ver [arquitectura.md](arquitectura.md)). El `Dockerfile` y el `docker-compose.yml` son solo para desarrollo local.

## Migraciones

La API aplica las migraciones pendientes de EF Core al arrancar (`Database.Migrate()` en [Program.cs](../DistribuidoraGustavo.API/Program.cs)), así que el deploy no tiene un paso de base de datos. Consecuencias:

- El usuario de la connection string de producción necesita permisos de DDL (`db_ddladmin`, o `db_owner`).
- Si una migración falla, la app no arranca y el App Service muestra el error en el log de arranque.
- Con más de una instancia, varias podrían migrar a la vez; con una sola instancia no hay problema.
- Las migraciones deben ser aditivas cuando se pueda: durante el reinicio la versión anterior no se detiene hasta que la nueva termina de migrar.

## Configuración única

**Azure (App Service del frontend, para conservar el DNS)**

1. Stack de runtime **.NET 10** (Linux o Windows, el deploy es el mismo).
2. Application settings con los valores de [backend.md](backend.md#configuración): `JWT_SecretKey`, `JWT_Issuer`, `JWT_Audience`, `EncryptKey`, y la connection string `DistribuidoraGustavo` (sección *Connection strings*, tipo SQLAzure, o la setting `ConnectionStrings__DistribuidoraGustavo`).
3. Habilitar *SCM Basic Auth Publishing Credentials* (Configuration → General settings) y descargar el **publish profile** (Overview → Get publish profile).
4. Si el servidor SQL tiene firewall, tildar *Allow Azure services* o agregar las IPs de salida del App Service.
5. **Primer despliegue sobre la base existente**: correr una sola vez [sql/adoptar-base-existente.sql](sql/adoptar-base-existente.sql) (ver [base-de-datos.md](base-de-datos.md#adoptar-una-base-que-ya-existe)). Marca `InitialCreate` como aplicada; `AddFamilies` y las siguientes las aplica la app al arrancar. Comparar antes el esquema real con el del modelo.

**GitHub**

- Environment `production` (opcional: reviewers para aprobar el deploy a mano).
- Secret `AZURE_WEBAPP_PUBLISH_PROFILE`: contenido completo del archivo `.PublishSettings`.
- Variable `APP_SERVICE_NAME`: nombre del App Service del frontend.

## Notas

- El segundo App Service (el de la API sola) ya no se usa: se puede apagar una vez verificado el sitio unificado.
- Rollback: re-ejecutar el workflow (`workflow_dispatch`) sobre un commit anterior. Las migraciones no se revierten solas.
