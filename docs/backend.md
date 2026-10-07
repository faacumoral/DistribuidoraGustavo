# Backend (API + Core)

[← README](../README.md)

## Arranque

[Program.cs](../DistribuidoraGustavo.API/Program.cs) usa *minimal hosting* con controllers:

1. `AddControllers`, `AddEndpointsApiExplorer`, `AddSwaggerGen`.
2. Registro de los 8 servicios como `Transient`.
3. Extensiones de [Configure.cs](../DistribuidoraGustavo.API/Configure.cs):
   - `InitConfig()`: carga `EncryptKey` en el singleton estático `ApplicationSettings.Config` (solo puede inicializarse una vez).
   - `AddJwtManager()`: crea un [JwtManager](../DistribuidoraGustavo.Core/Services/JwtManager.cs) como singleton `IJwtManager` y configura `JwtBearer` validando issuer, audience, lifetime y firma.
   - `AddCors()`: política `"Origins"` que permite cualquier origen, método y header.
4. `AddDbContext<DistribuidoraGustavoContext>` con SQL Server y la connection string `DistribuidoraGustavo`.

Pipeline: `UseSwagger` → `UseSwaggerUI` → `UseAuthentication` → `UseHttpsRedirection` → `UseAuthorization` → `UseCors("Origins")` → `MapControllers`. Swagger está activo en todos los entornos. (El orden de CORS está comentado en [deuda-tecnica.md](deuda-tecnica.md#seguridad).)

## Configuración

| Clave | Uso | Dónde está |
|---|---|---|
| `ConnectionStrings:DistribuidoraGustavo` | SQL Server | `appsettings.Development.json` (local: `.\sqlexpress01`) |
| `EncryptKey` | Clave para encriptar contraseñas de usuarios | ídem |
| `JWT_SecretKey`, `JWT_Issuer`, `JWT_Audience` | Firma y validación del JWT. La clave debe tener **32 caracteres o más**. | ídem |

[appsettings.json](../DistribuidoraGustavo.API/appsettings.json) no tiene ninguna de estas claves: en producción deben venir de variables de entorno / App Settings del hosting (inferido; no hay nada en el repo que lo defina). El perfil de publicación de producción del proyecto SQL y la carpeta `PublishProfiles/` están en `.gitignore`.

Puertos locales: `https://localhost:7109` y `http://localhost:5109` ([launchSettings.json](../DistribuidoraGustavo.API/Properties/launchSettings.json)).

## Autenticación

- `POST /Users/login` es el único endpoint de usuarios. [UserService.TryLogin](../DistribuidoraGustavo.Core/Services/UserService.cs#L27) encripta la contraseña recibida con `PasswordEncryptor.Encrypt(password, EncryptKey)` y busca un `User` con ese `Username` y `Password` exactos. Verifica `Active`.
- Devuelve `LoginResponse { Jwt, User }`. El token lo genera `IJwtManager.GenerateToken(userId)` y vence a los 180 minutos. El id de usuario viaja en el claim `IdUsuario`.
- No hay roles, refresh token, alta de usuarios ni cambio de contraseña. Los usuarios se crean directamente en la base.
- Todo lo demás requiere JWT salvo `GET /Invoices/download`, que se protege con un token de descarga de un solo uso (ver [dominio.md](dominio.md#descarga-del-pdf)).

## Endpoints

Todas las rutas son relativas a la raíz (sin `/api`). Salvo indicación, requieren `Authorization: Bearer {jwt}` y responden HTTP 200 con un *result object*.

### Users — [UsersController](../DistribuidoraGustavo.API/Controllers/UsersController.cs)

| Método | Ruta | Body | Respuesta | Notas |
|---|---|---|---|---|
| POST | `/Users/login` | `LoginRequest { Username, Password }` | `DTOResult<LoginResponse>` | Anónimo. Sin `try/catch`. |

### Clients — [ClientsController](../DistribuidoraGustavo.API/Controllers/ClientsController.cs)

| Método | Ruta | Body / Query | Respuesta | Notas |
|---|---|---|---|---|
| GET | `/Clients` | — | `ListResult<ClientModel>` | Incluye `DefaultPriceList`. Sin paginado ni orden. |
| GET | `/Clients/{clientId}` | — | `DTOResult<ClientModel>` | |
| POST | `/Clients` | `ClientModel` | `DTOResult<ClientModel>` | Upsert. Valida `Name` y `DefaultPriceList`. En alta valida prefijo único; en modificación solo cambia `Name` y lista por defecto (el prefijo es inmutable). |

### Products — [ProductsController](../DistribuidoraGustavo.API/Controllers/ProductsController.cs)

| Método | Ruta | Body / Query | Respuesta | Notas |
|---|---|---|---|---|
| GET | `/Products` | `filter`, `priceListId` | `ListResult<ProductModel>` | Máx. 100 activos. Si `priceListId` existe, completa `UnitPrice` con el precio de esa lista. |
| GET | `/Products/prices` | `filter`, `priceListId` | `ListResult<PricedProductModel>` | Máx. 300. `priceListId` 0 o ausente trae precios de todas las listas. |
| GET | `/Products/{ProductId}` | — | `DTOResult<ProductModel>` | |
| POST | `/Products` | `ProductModel` | `DTOResult<ProductModel>` | Upsert. Recalcula los precios de todas las listas. |

### PriceLists — [PriceListsController](../DistribuidoraGustavo.API/Controllers/PriceListsController.cs)

| Método | Ruta | Body / Query | Respuesta | Notas |
|---|---|---|---|---|
| GET | `/PriceLists` | — | `ListResult<PriceListModel>` | |
| POST | `/PriceLists/uploadFile` | `multipart/form-data`, campo `file` | `ListResult<string>` | Importa productos y precios desde Excel. La lista devuelta son los errores por fila. |

No hay ABM de listas de precios (figura como pendiente en `todo.txt`); se administran en la base.

### Invoices — [InvoicesController](../DistribuidoraGustavo.API/Controllers/InvoicesController.cs)

| Método | Ruta | Body / Query | Respuesta | Notas |
|---|---|---|---|---|
| GET | `/Invoices` | `ClientID` (0 = todos) | `ListResult<InvoiceModel>` | Últimas 50 activas, por fecha descendente. |
| GET | `/Invoices/{invoiceId}` | — | `DTOResult<InvoiceModel>` | No filtra por `Active`. |
| POST | `/Invoices` | `InvoiceModel` | `DTOResult<InvoiceModel>` | Upsert. Actualiza saldo del cliente y su transacción. |
| DELETE | `/Invoices/{invoiceId}` | — | `BoolResult` | Baja lógica (`Active = false`). |
| GET | `/Invoices/{invoiceID}/downloadtoken` | — | `StringResult` | Token GUID de un solo uso, válido 1 minuto. |
| GET | `/Invoices/download` | `token` | Archivo `application/pdf` | Anónimo. HTTP 500 si el token es inválido. |

### Transactions — [TransactionsController](../DistribuidoraGustavo.API/Controllers/TransactionsController.cs)

| Método | Ruta | Body / Query | Respuesta | Notas |
|---|---|---|---|---|
| GET | `/Transactions` | `clientId` (opcional) | `ListResult<TransactionModel>` | Últimas 200 por fecha descendente. |
| POST | `/Transactions/payment` | `TransactionModel` (`Amount`, `Client.ClientId`) | `DTOResult<TransactionModel>` | Registra un pago y descuenta el saldo. |

## Servicios

| Servicio | Depende de | Responsabilidad |
|---|---|---|
| [UserService](../DistribuidoraGustavo.Core/Services/UserService.cs) | `DbContext`, `IJwtManager` | Login. |
| [ClientService](../DistribuidoraGustavo.Core/Services/ClientService.cs) | `DbContext` | Listado, alta y modificación de clientes. |
| [ProductService](../DistribuidoraGustavo.Core/Services/ProductService.cs) | `DbContext`, `IPriceListService` | Búsqueda, precios por lista, upsert. |
| [PriceListService](../DistribuidoraGustavo.Core/Services/PriceListService.cs) | `DbContext`, `IExcelService` | Listado, importación de Excel, `CalculatePrices`. |
| [ExcelService](../DistribuidoraGustavo.Core/Services/ExcelService.cs) | — | Lee la primera hoja y devuelve `ExcelProductModel` (todo como string). |
| [InvoiceService](../DistribuidoraGustavo.Core/Services/InvoiceService.cs) | `DbContext`, `IPdfService`, `ITransactionService` | Facturas, numeración, tokens de descarga. |
| [TransactionService](../DistribuidoraGustavo.Core/Services/TransactionService.cs) | `DbContext` | Movimientos de cuenta corriente y pagos. |
| [PdfService](../DistribuidoraGustavo.Core/Services/PdfService.cs) | — | Rellena [InvoiceTemplate.html](../DistribuidoraGustavo.Core/HTML/InvoiceTemplate.html) y lo convierte a PDF. |

`BaseService` e `IBaseService` están vacíos; no todos los servicios los usan.

Las reglas de negocio de cada uno están en [dominio.md](dominio.md).

## Manejo de errores y logging

- Los controllers atrapan toda excepción y devuelven `Result.Error(ex)` con HTTP 200.
- [Log.Error](../DistribuidoraGustavo.Interfaces/Shared/Log.cs#L6) **está vacío** (`#warning TBD`): hoy las excepciones no se registran en ningún lado. "agregar logs" es el primer ítem de `todo.txt`.
- No hay middleware global de excepciones ni `ProblemDetails`.

## Código de infraestructura propio

Hasta octubre de 2026 estas piezas venían de las DLL `FMCW.*`; ahora viven en el repo.

| Pieza | Ubicación | Notas |
|---|---|---|
| Tipos de resultado | [Interfaces/Results](../DistribuidoraGustavo.Interfaces/Results/) | `BaseResult<T>` (base), `DTOResult<T>`, `ListResult<T>`, `BoolResult`, `StringResult`, `ErrorResult`, `IBaseErrorResult`, `ResultOperation`. El JSON es el contrato con el frontend: no renombrar propiedades ni cambiar los valores numéricos de `ResultOperation` (`Ok = 0`, `Unauthorized = 1`, `Error = 3`). |
| JWT | [IJwtManager](../DistribuidoraGustavo.Interfaces/Services/IJwtManager.cs), [JwtConfiguration](../DistribuidoraGustavo.Interfaces/Settings/JwtConfiguration.cs), [JwtManager](../DistribuidoraGustavo.Core/Services/JwtManager.cs) | Genera el token con `SecurityTokenDescriptor` + `JwtSecurityTokenHandler` (HS256). Claims: `jti` e `IdUsuario` (constante `JwtManager.USER_ID_CLAIM`, que también usa `BaseController`). |
| Encriptado de contraseñas | [PasswordEncryptor](../DistribuidoraGustavo.Core/Security/PasswordEncryptor.cs) | AES-256-CBC con clave derivada por `PasswordDeriveBytes` (SHA1, 2 iteraciones) y salt/IV fijos. Las constantes definen el formato de las contraseñas ya guardadas: cambiarlas invalida todos los usuarios. |

## Paquetes NuGet

| Proyecto | Paquetes |
|---|---|
| API | `Microsoft.AspNetCore.Authentication.JwtBearer` 6.0.16, `Swashbuckle.AspNetCore` 6.2.3 |
| Core | `System.IdentityModel.Tokens.Jwt` 6.36.0, `itext7` 8.0.0, `itext7.bouncy-castle-adapter` 8.0.0, `itext7.pdfhtml` 5.0.0, `ExcelDataReader` + `.DataSet` 3.6.0, `EPPlus` 6.2.6 y `NPOI` 2.6.0 (estos dos últimos referenciados pero sin uso real) |
| Data | `Microsoft.EntityFrameworkCore` / `.SqlServer` / `.Tools` 7.0.5 |
