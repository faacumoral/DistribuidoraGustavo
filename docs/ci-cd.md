# CI/CD

Integración y despliegue con GitHub Actions. El destino es **un único Azure App Service** (el del frontend, que conserva el DNS) que sirve la API y el Blazor WASM juntos.

## Flujo

```
Pull request ──► ci.yml      restore + build de la solución (Release)

Push a main ───► deploy.yml  dotnet publish de API ──► zip deploy al App Service
                                                          │
                                                          ▼
                                         la app arranca y aplica las migraciones de EF Core
```

| Workflow | Archivo | Disparador | Qué hace |
|---|---|---|---|
| CI | [ci.yml](../.github/workflows/ci.yml) | Pull request | `dotnet restore` + `dotnet build -c Release` de la solución. No hay tests: el build es la única verificación. |
| Deploy | [deploy.yml](../.github/workflows/deploy.yml) | Push a `main`, o manual (`workflow_dispatch`) | `dotnet publish` del proyecto `API` y deploy del resultado con `azure/webapps-deploy`. |

- `dotnet publish` del proyecto `API` incluye el frontend Blazor en `wwwroot` (ver [arquitectura.md](arquitectura.md)), por eso alcanza con un solo paquete.
- El deploy corre en el environment `production` de GitHub y con `concurrency: deploy` (un deploy a la vez, sin cancelar el que está en curso).
- El `Dockerfile` y el `docker-compose.yml` **no participan** del despliegue: son solo para desarrollo local.

## Migraciones de base de datos

No hay paso de base de datos en el pipeline. La API ejecuta `Database.Migrate()` al arrancar ([Program.cs](../DistribuidoraGustavo.API/Program.cs)), así que cada deploy aplica las migraciones pendientes.

- Si una migración falla, la excepción corta el arranque: el sitio queda caído y el error está en el log de arranque del App Service. No corre sobre un esquema desactualizado.
- El usuario de la connection string de producción necesita permisos de DDL (`db_ddladmin` o `db_owner`).
- Mientras la nueva versión migra, la anterior sigue respondiendo; conviene que las migraciones sean aditivas (agregar columnas/tablas, no renombrar ni borrar en el mismo paso).
- Con más de una instancia, varias podrían migrar a la vez. Hoy hay una sola instancia, así que no aplica.
- La base de producción venía del esquema anterior (SSDT): se adoptó una única vez con [sql/adoptar-base-existente.sql](sql/adoptar-base-existente.sql), que marca `InitialCreate` como aplicada. Ver [base-de-datos.md](base-de-datos.md#adoptar-una-base-que-ya-existe).

## Configuración

### GitHub (Settings → Secrets and variables → Actions)

| Tipo | Nombre | Contenido |
|---|---|---|
| Secret | `AZURE_WEBAPP_PUBLISH_PROFILE` | Contenido completo del archivo `.PublishSettings` del App Service |
| Variable | `APP_SERVICE_NAME` | Nombre del App Service (sin `.azurewebsites.net`) |

Environment `production` (Settings → Environments): permite agregar reviewers para que cada deploy requiera aprobación manual.

### Azure App Service

- Stack de runtime **.NET 10**.
- *SCM Basic Auth Publishing Credentials* habilitado (Configuration → General settings), necesario para el publish profile.
- Application settings:

| Setting | Uso |
|---|---|
| `ConnectionStrings__DistribuidoraGustavo` (o connection string `DistribuidoraGustavo` de tipo SQLAzure) | Azure SQL |
| `JWT_SecretKey` | Firma del JWT. 32 caracteres o más. |
| `JWT_Issuer`, `JWT_Audience` | Validación del JWT |
| `EncryptKey` | Clave AES de las contraseñas de `Users.Password` |

- El firewall del servidor SQL tiene que permitir al App Service (*Allow Azure services* o sus IPs de salida).

## Operaciones comunes

**Desplegar**: mergear o pushear a `main`. Para redesplegar sin cambios: Actions → Deploy → *Run workflow*.

**Rollback**: ejecutar *Run workflow* sobre un commit anterior (o revertir el commit y pushear). Las migraciones **no** se revierten solas: si el commit viejo es anterior a una migración ya aplicada, la base queda con el esquema nuevo. Para deshacer un cambio de esquema hay que escribir una migración nueva.

**Cambiar un setting o secret**: modificar la Application setting en Azure (reinicia el sitio) o el secret en GitHub (aplica al próximo deploy).

**Rotar el publish profile**: en Azure, *Reset publish profile*, descargar el nuevo y reemplazar el secret `AZURE_WEBAPP_PUBLISH_PROFILE`.

**Rotar `JWT_SecretKey`**: es seguro; solo cierra las sesiones abiertas.

**No rotar `EncryptKey` a la ligera**: [PasswordEncryptor](../DistribuidoraGustavo.Core/Security/PasswordEncryptor.cs) encripta las contraseñas con esa clave. Cambiarla deja a todos los usuarios sin poder entrar, salvo que se regeneren las contraseñas guardadas.

**Aplicar migraciones a mano** (sin desplegar): ver [base-de-datos.md](base-de-datos.md#desplegar-cambios).

## Problemas típicos

| Síntoma | Causa probable |
|---|---|
| El paso *Deploy to App Service* falla con `app-name` vacío | Falta la variable `APP_SERVICE_NAME` |
| Falla con error de autenticación o 401 | Publish profile vencido/reseteado, o *SCM Basic Auth* deshabilitado |
| Deploy verde pero el sitio devuelve 500.30 / no arranca | Falló una migración o falta una setting. Ver *Log stream* del App Service. |
| Error de permisos (`CREATE TABLE denied`) en el log | El usuario SQL no tiene DDL |
| Timeout o `Cannot open server` al migrar | El firewall del SQL no deja pasar al App Service |
| Todos los logins fallan tras el deploy | `EncryptKey` distinta a la usada para guardar las contraseñas |
| Los usuarios tienen que volver a loguearse | Esperable si cambió `JWT_SecretKey` |
