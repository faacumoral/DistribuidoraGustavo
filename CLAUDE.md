# Distribuidora Gustavo

App de gestión (productos, listas de precios, clientes, facturas, cuenta corriente). Blazor WASM (`DistribuidoraGustavo.App`) + ASP.NET Core Web API (`API` → `Core` → `Data`), servidos juntos por el proyecto `API` (un solo sitio; la API bajo `/api`), .NET 10, EF Core 10 code first, SQL Server.

Antes de modificar código, leer [README.md](README.md) y el documento de `docs/` que corresponda:

- [docs/arquitectura.md](docs/arquitectura.md): capas, patrones y convenciones.
- [docs/backend.md](docs/backend.md): endpoints, servicios, auth, config.
- [docs/frontend.md](docs/frontend.md): páginas, componentes, `ApiClient`.
- [docs/base-de-datos.md](docs/base-de-datos.md): esquema y migraciones de EF Core.
- [docs/dominio.md](docs/dominio.md): reglas de negocio.
- [docs/guia-de-cambios.md](docs/guia-de-cambios.md): recetas para cambios comunes.
- [docs/deuda-tecnica.md](docs/deuda-tecnica.md): bugs latentes y trampas.

## Reglas rápidas

- Respuestas de la API: siempre `DTOResult<T>` / `ListResult<T>` / `BoolResult` / `StringResult` (`Interfaces/Results`), con HTTP 200 incluso en error. El frontend chequea `.Success`.
- Controllers heredan de `BaseController` (ruta `api/[controller]`); cada acción con `try/catch` + `Log.Error(ex)` + `Result.Error(ex)`.
- Escrituras por POST como upsert: id `0` = alta.
- Mapeo entidad → modelo en `CastEfToModel.ToModel()`; modelo → entidad a mano en el servicio. Un campo nuevo hay que agregarlo en ambos lados.
- Base de datos code first: el esquema se cambia editando las entidades de `DistribuidoraGustavo.Data/EfModels` y generando una migración (`dotnet ef migrations add X --project DistribuidoraGustavo.Data --startup-project DistribuidoraGustavo.API`). Revisar siempre la migración generada.
- Páginas Blazor: `X.razor` (`@inherits XBase`) + `X.razor.cs` (`XBase : ComponentBase`, estado en `XState`). Vista nueva = valor en el enum `Views` + `@page` + entrada en `NavMenu`.
- Cualquier cambio en el total de una factura debe mantener sincronizados `Invoices_Products`, la `Transaction` asociada (por `Description = InvoiceNumber`) y `Clients.ActualBalance`.
- Código en inglés; textos de UI y mensajes de error en español.
- No hay tests: validar con `dotnet build` de los `.csproj` y prueba manual.
- Si un cambio altera algo descrito en `docs/`, actualizar el documento en el mismo cambio.
