# Distribuidora Gustavo

Sistema de gestión interno para una distribuidora: productos, listas de precios, clientes, facturas (con PDF) y cuenta corriente (saldos y pagos).

Es una SPA **Blazor WebAssembly** (PWA) que consume una **API REST ASP.NET Core**, con **SQL Server** como base de datos.

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
| Hosting (inferido) | Azure App Service (`api-distribuidoragustavo.azurewebsites.net`) |

No hay tests ni CI.

## Estructura de la solución

```
DistribuidoraGustavo.sln
├── DistribuidoraGustavo.App         Frontend Blazor WASM (páginas, componentes, ApiClient)
├── DistribuidoraGustavo.API         Web API: Program.cs, controllers, config de JWT/CORS
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
3. **API**:
   ```bash
   dotnet run --project DistribuidoraGustavo.API
   ```
   Escucha en `https://localhost:7109` (Swagger en `/swagger`). La configuración de desarrollo está en [appsettings.Development.json](DistribuidoraGustavo.API/appsettings.Development.json).
4. **App**:
   ```bash
   dotnet run --project DistribuidoraGustavo.App
   ```
   Escucha en `https://localhost:7105`. La URL de la API se toma de [wwwroot/appsettings.json](DistribuidoraGustavo.App/wwwroot/appsettings.json) (`ApiUrl`).

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
