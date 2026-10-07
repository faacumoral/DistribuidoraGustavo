# Guía de cambios

[← README](../README.md)

Recetas para los cambios más frecuentes, siguiendo las convenciones existentes. Antes de tocar facturas, saldos o importación, leer [deuda-tecnica.md](deuda-tecnica.md).

## Agregar un campo a una entidad existente

Ejemplo: agregar `Phone` a clientes.

1. **SQL**: agregar la columna en [Tables/Clients.sql](../DistribuidoraGustavo.SQL/Tables/Clients.sql) (nullable o con default, porque ya hay datos).
2. **EF**: publicar y re-scaffoldear ([base-de-datos.md](base-de-datos.md#cómo-cambiar-el-esquema)), o editar a mano `Client.cs` y `OnModelCreating` respetando el estilo generado.
3. **Modelo**: agregar la propiedad en [ClientModel.cs](../DistribuidoraGustavo.Interfaces/Models/ClientModel.cs).
4. **Mapper**: agregarla en `ToModel(this Client)` de [CastEfToModel.cs](../DistribuidoraGustavo.Interfaces/Parsers/CastEfToModel.cs).
5. **Servicio**: asignarla en `InsertClient` y `UpdateClient` de [ClientService.cs](../DistribuidoraGustavo.Core/Services/ClientService.cs) (el mapeo modelo → entidad es manual).
6. **Controller**: sumar validación si corresponde en [ClientsController.UpsertClient](../DistribuidoraGustavo.API/Controllers/ClientsController.cs).
7. **UI**: agregar el input en [AddOrEditClient.razor](../DistribuidoraGustavo.App/Pages/AddOrEditClient.razor) y la columna en [Clients.razor](../DistribuidoraGustavo.App/Pages/Clients.razor).

Es fácil olvidar los pasos 4 y 5: como no hay mapeo automático, un campo que falte ahí simplemente no viaja o no se guarda, sin error.

## Agregar un endpoint a un controller existente

1. Declarar el método en la interfaz `I{X}Service` ([Interfaces/Services](../DistribuidoraGustavo.Interfaces/Services/)).
2. Implementarlo en `{X}Service` ([Core/Services](../DistribuidoraGustavo.Core/Services/)).
3. Agregar la acción en el controller con el esqueleto estándar:

   ```csharp
   [HttpGet("ruta")]
   public async Task<ListResult<FooModel>> GetFoo([FromQuery] int? clientId)
   {
       try
       {
           var items = await _fooService.GetFoo(clientId);
           return ListResult<FooModel>.Ok(items.ToList());
       }
       catch (Exception ex)
       {
           Log.Error(ex);
           return ListResult<FooModel>.Error(ex);
       }
   }
   ```

4. Consumirlo desde la página con `ApiClient.Send<ListResult<FooModel>>(ApiRequest.BuildGet("X/ruta?clientId=..."))`.

Si se necesita un verbo que `ApiRequest` no tiene (PUT, PATCH), agregar el `Build*` correspondiente en [ApiRequest.cs](../DistribuidoraGustavo.App/Http/ApiRequest.cs).

## Agregar una entidad completa (ABM nuevo)

Ejemplo: el ABM de listas de precios pendiente.

1. **SQL** + **EF** (si la tabla es nueva; `PriceLists` ya existe).
2. **Modelo** en `Interfaces/Models` (heredar de `BaseModel` si va a viajar dentro de un `DTOResult<T>`; `PriceListModel` hoy no hereda).
3. **Mapper** `ToModel` en `CastEfToModel`.
4. **Interfaz + servicio**; registrar el servicio en [Program.cs](../DistribuidoraGustavo.API/Program.cs#L16) con `AddTransient` si es nuevo.
5. **Controller** heredando de `BaseController`.
6. **Frontend**: seguir la receta de página nueva (listado + `AddOrEditX`).

Para listas de precios en particular: al cambiar `Percent` o crear una lista hay que **recalcular `Products_PriceLists` para todos los productos** llamando a `CalculatePrices`; hoy eso solo ocurre por producto.

## Agregar una página

1. Agregar el valor en el enum [Views](../DistribuidoraGustavo.App/Utils/Views.cs).
2. Crear `Pages/X.razor` con `@page "/x"` (igual al nombre del enum, en minúsculas) y `@inherits XBase`.
3. Crear `Pages/X.razor.cs` con `XState` y `XBase : ComponentBase` (ver plantilla en [frontend.md](frontend.md#patrón-de-página)).
4. Agregar el `NavLink` en [NavMenu.razor](../DistribuidoraGustavo.App/Shared/NavMenu.razor) con un ícono de Bootstrap Icons (`bi bi-...`).
5. Para alta/edición, copiar el esquema de doble ruta: `@page "/addoreditx/{XId:int}"` y `@page "/addoreditx"`, con `[Parameter] public int XId { get; set; }` (0 = alta).

## Modificar el PDF de la factura

- Diseño: editar [InvoiceTemplate.html](../DistribuidoraGustavo.Core/HTML/InvoiceTemplate.html). Es HTML + CSS embebido, convertido por iText pdfHTML (soporta CSS básico, no JavaScript).
- Datos nuevos: agregar un token `{{MiToken}}` en el template y su `.ReplaceToken("MiToken", valor)` en [PdfService.GenerateInvoicePdf](../DistribuidoraGustavo.Core/Services/PdfService.cs). Un token sin reemplazo queda impreso literalmente.
- Columnas de la tabla de ítems: cambiar a la vez el `<th>` del template, el `<tr>` generado en `PdfService` y el `colspan` de la fila de total.

## Cambiar el formato del Excel de importación

- Columnas: [ExcelService.ProcessFile](../DistribuidoraGustavo.Core/Services/ExcelService.cs) (lectura por índice) y [ExcelProductModel](../DistribuidoraGustavo.Interfaces/Models/ExcelProductModel.cs).
- Qué se hace con cada fila: [PriceListService.ProcessFile](../DistribuidoraGustavo.Core/Services/PriceListService.cs#L64).

## Cambiar reglas de precios

Todo pasa por [CalculatePrices](../DistribuidoraGustavo.Core/Services/PriceListService.cs#L127). Como el precio está persistido, cualquier cambio de fórmula requiere además recalcular los datos existentes (reimportar el Excel o un script).

## Tocar facturas o saldos

Cualquier operación que cambie el total de una factura debe mantener sincronizadas **tres cosas**: los ítems (`Invoices_Products`), la transacción asociada (`Transactions`, buscada por `Description = InvoiceNumber`) y `Clients.ActualBalance`. Revisar `UpsertInvoice`, `DeleteInvoice` y `TransactionService` en conjunto.

## Agregar configuración

- **API**: clave en `appsettings.Development.json` (y en el hosting de producción). Leerla con `builder.Configuration["Clave"]`; si la necesita `Core`, exponerla vía [ApplicationSettings](../DistribuidoraGustavo.Interfaces/Settings/ApplicationSettings.cs) e inicializarla en `Configure.InitConfig`.
- **App**: clave en `wwwroot/appsettings.json`; leerla con `IConfiguration` inyectado. Todo lo que esté ahí es público.

## Verificación

No hay tests automatizados. Para validar un cambio:

```bash
dotnet build DistribuidoraGustavo.sln
```

(El proyecto `.sqlproj` requiere MSBuild de Visual Studio con SSDT; con `dotnet build` conviene compilar los `.csproj` de API y App por separado.) Después, probar a mano el flujo afectado con API y App levantadas, o los endpoints desde Swagger en `https://localhost:7109/swagger`.
