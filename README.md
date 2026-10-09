# Distribuidora Gustavo

Sistema de gestión interno para una distribuidora: productos, listas de precios, clientes, facturas (con PDF) y cuenta corriente (saldos y pagos).

Es una SPA **Blazor WebAssembly** (PWA) que consume una **API REST ASP.NET Core**, con **SQL Server** como base de datos. La API sirve también el frontend: se ejecuta y se despliega como **una sola aplicación**.

> Esta documentación se generó por análisis estático del código (sin compilar ni ejecutar). Donde algo es una inferencia y no un hecho leído del código, está marcado como tal.

## Stack

| Capa | Tecnología |
|---|---|
| Frontend | Blazor WebAssembly (.NET 10), PWA con service worker, Bootstrap 5 + Bootstrap Icons (CDN) |
| Backend | ASP.NET Core Web API (.NET 10), controllers MVC, Swagger (Swashbuckle) |
| Autenticación | JWT Bearer (clave simétrica, 180 min), emitido con `JwtSecurityTokenHandler` (`System.IdentityModel.Tokens.Jwt`) |
| Acceso a datos | Entity Framework Core 10, *code first* con migraciones |
| Base de datos | SQL Server / Azure SQL |
| PDF | iText 7 + pdfHTML (HTML → PDF) |
| Excel | ExcelDataReader (importación de lista de precios) |
| Hosting (inferido) | Azure App Service; un solo sitio para API y frontend |

No hay tests ni CI.

## Estructura de la solución

```
DistribuidoraGustavo.sln
├── DistribuidoraGustavo.App         Frontend Blazor WASM (páginas, componentes, ApiClient)
├── DistribuidoraGustavo.API         Host: Web API (rutas /api/...) + sirve el frontend; controllers, config de JWT
├── DistribuidoraGustavo.Core        Lógica de negocio: implementación de los servicios, template del PDF
├── DistribuidoraGustavo.Interfaces  Contratos compartidos: modelos (DTOs), interfaces de servicios, mappers EF→modelo
├── DistribuidoraGustavo.Data        DbContext, entidades EF y migraciones (fuente de verdad del esquema)
├── dotnet-tools.json                Herramientas locales (dotnet-ef)
└── todo.txt                         Pendientes anotados por el autor
```

## Cómo levantarlo en local

Requisitos: SDK de .NET 10 y SQL Server Express con instancia `.\sqlexpress01`.

1. **Base de datos**: crearla con las migraciones de EF Core.
   ```bash
   dotnet tool restore
   ```
   ```bash
   dotnet ef database update --project DistribuidoraGustavo.Data --startup-project DistribuidoraGustavo.API
   ```
   Si ya tenés una base local creada con el proyecto SQL anterior, no corras el segundo comando todavía: ver [Adoptar una base que ya existe](docs/base-de-datos.md#adoptar-una-base-que-ya-existe).
2. **Datos iniciales** (no hay seed en el repo): hay que insertar a mano al menos una fila en `PriceLists` y un usuario en `Users`. La contraseña se guarda encriptada con `PasswordEncryptor.Encrypt(password, EncryptKey)` ([PasswordEncryptor.cs](DistribuidoraGustavo.Core/Security/PasswordEncryptor.cs)); ver [docs/backend.md](docs/backend.md#autenticación).
3. **Ejecutar** (API y frontend juntos):
   ```bash
   dotnet run --project DistribuidoraGustavo.API
   ```
   La app queda en `https://localhost:7109`, la API en `https://localhost:7109/api/...` y Swagger en `/swagger`. La configuración de desarrollo está en [appsettings.Development.json](DistribuidoraGustavo.API/appsettings.Development.json).

## Levantarlo con Docker

Alternativa que no requiere instalar SQL Server ni el SDK de .NET: solo Docker.

```bash
docker compose up --build
```

Levanta SQL Server, aplica las migraciones, carga datos de prueba y arranca la aplicación:

| Qué | Dónde |
|---|---|
| App | `http://localhost:5080` |
| Swagger | `http://localhost:5080/swagger` |
| SQL Server | `localhost,14333`, usuario `sa` (contraseña en [docker-compose.yml](docker-compose.yml)) |

El usuario de prueba y las listas de precios iniciales están en [docker/seed.sql](docker/seed.sql). Los datos persisten entre reinicios en un volumen; para empezar de cero:

```bash
docker compose down -v
```

Los puertos se pueden cambiar con las variables `APP_PORT` y `SQL_PORT`. Después de cambiar código hay que volver a correr `docker compose up --build`. Es un entorno solo para desarrollo: usa las claves de `appsettings.Development.json`.

## Publicar

```bash
dotnet publish DistribuidoraGustavo.API -c Release -o publish
```

La carpeta resultante contiene todo: la API, el frontend compilado (en `wwwroot/`) y el template del PDF. Es lo único que se despliega.

## Documentación

| Documento | Contenido |
|---|---|
| [docs/arquitectura.md](docs/arquitectura.md) | Capas, dependencias entre proyectos, flujo de un request, patrones y convenciones |
| [docs/backend.md](docs/backend.md) | Arranque de la API, autenticación, endpoints, servicios, manejo de errores, configuración |
| [docs/frontend.md](docs/frontend.md) | Estructura de la app Blazor, ruteo, páginas, componentes compartidos, `ApiClient`, sesión |
| [docs/base-de-datos.md](docs/base-de-datos.md) | Tablas, relaciones, y cómo cambiar el esquema con migraciones de EF Core |
| [docs/dominio.md](docs/dominio.md) | Reglas de negocio: precios, facturas, numeración, saldos, importación de Excel, PDF |
| [docs/guia-de-cambios.md](docs/guia-de-cambios.md) | Recetas paso a paso para los cambios más comunes |
| [docs/deuda-tecnica.md](docs/deuda-tecnica.md) | Bugs latentes, riesgos y trampas a tener en cuenta antes de tocar el código |
