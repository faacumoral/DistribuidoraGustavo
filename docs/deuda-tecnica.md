# Deuda técnica, riesgos y trampas

[← README](../README.md)

Hallazgos del análisis estático. No son tareas a ejecutar: son cosas a saber antes de modificar el código. Están ordenados por área y, dentro de cada una, por impacto.

## Consistencia de datos (facturas y saldos)

1. **Operaciones no atómicas.** Alta y modificación de factura hacen más de un `SaveChangesAsync` sin transacción explícita: `TransactionService.Create/UpdateInvoiceTransaction` guarda (y, por compartir el `DbContext`, persiste también la factura), y recién después se ajusta el saldo y se guarda de nuevo ([InvoiceService.cs:102](../DistribuidoraGustavo.Core/Services/InvoiceService.cs#L102), [:143-148](../DistribuidoraGustavo.Core/Services/InvoiceService.cs#L143)). Un fallo en el medio deja factura y transacción guardadas con el saldo sin actualizar.

2. **Numeración con condición de carrera.** El siguiente número se calcula leyendo todos los números del cliente y haciendo `Max + 1` en memoria ([InvoiceService.cs:73-83](../DistribuidoraGustavo.Core/Services/InvoiceService.cs#L73)). Dos altas simultáneas para el mismo cliente generan el mismo número; la segunda falla por el índice único. Además `int.Parse` rompe si algún número no respeta el formato `{prefijo}-{n}`.

3. **Editar una factura cambiando el cliente corrompe saldos.** En la modificación, el cliente se toma del modelo recibido y no de la factura guardada ([InvoiceService.cs:63](../DistribuidoraGustavo.Core/Services/InvoiceService.cs#L63)). Si en la pantalla se elige otro cliente al editar, se le resta el total viejo y se le suma el nuevo al cliente equivocado, no se encuentra la transacción a actualizar, y la factura sigue perteneciendo al cliente original. La UI no bloquea el combo de cliente en edición.

4. **Vínculo factura ↔ transacción por texto.** Se busca por `ClientId` + `Description == InvoiceNumber` ([TransactionService.cs:21](../DistribuidoraGustavo.Core/Services/TransactionService.cs#L21)). No hay FK ni columna dedicada.

5. **Saldo desnormalizado sin reconciliación.** `ActualBalance` se ajusta por incrementos; no hay forma de recalcularlo ni de detectar desvíos respecto de `Transactions`. Al borrar una factura se elimina su transacción, así que tampoco queda historial.

6. **El backend confía en los importes del frontend.** `Amount` de cada ítem no se valida contra `Quantity × UnitPrice`, ni `UnitPrice` contra la lista de precios. La pantalla permite editar el importe del ítem a mano.

7. **Límite de `DECIMAL(10,2)`.** Importes, precios y saldos admiten hasta 99.999.999,99; superar eso produce error de SQL al guardar.

8. **Al editar una factura se pierde la lista de precios original en pantalla.** `LoadInvoice` llama a `ClientChange`, que selecciona la lista por defecto del cliente en lugar de `invoice.PriceList` ([AddOrEditInvoice.razor.cs:72-86](../DistribuidoraGustavo.App/Pages/AddOrEditInvoice.razor.cs#L72)). Los ítems existentes conservan su precio, pero los que se agreguen usan la otra lista.

9. **El PDF no es histórico.** Fecha y saldos se calculan al descargar ([PdfService.cs:46-49](../DistribuidoraGustavo.Core/Services/PdfService.cs#L46)); reimprimir una factura vieja muestra el saldo de hoy.

## Importación de Excel

1. **No se saltea el encabezado.** Si la planilla tiene fila de títulos, se crea un producto con esos textos como código y nombre ([PriceListService.cs:81-110](../DistribuidoraGustavo.Core/Services/PriceListService.cs#L81)).
2. **Precio inválido pisa el precio base con 0** sin recalcular las listas, dejando `BasePrice` y `Products_PriceLists` inconsistentes.
3. **Parseo dependiente de la cultura del servidor** (`decimal.TryParse` sin `CultureInfo`): el separador decimal puede interpretarse distinto en local y en producción.
4. **Los errores por fila no se muestran al usuario**; la página solo los escribe en la consola del navegador.
5. La limpieza de archivos viejos en `SaveImportedFile` marca el borrado después de su propio `SaveChanges` ([:57-61](../DistribuidoraGustavo.Core/Services/PriceListService.cs#L57)); solo se persiste porque `ProcessFile` guarda más adelante.
6. `accept="xls,xlsx"` en [HiddenFileInput.razor:4](../DistribuidoraGustavo.App/Shared/HiddenFileInput.razor#L4) no es un valor válido (debería ser `.xls,.xlsx`), así que el diálogo no filtra por tipo.
7. A verificar en ejecución: ExcelDataReader sobre .NET Core suele requerir registrar `System.Text.Encoding.CodePages` para leer `.xls` antiguos; el código no lo hace.

## Seguridad

1. **Secretos versionados.** [appsettings.Development.json](../DistribuidoraGustavo.API/appsettings.Development.json) contiene `JWT_SecretKey` y `EncryptKey`. Si producción usa los mismos valores, deben rotarse.
2. **Contraseñas con encriptación reversible**, no hash ([PasswordEncryptor.cs](../DistribuidoraGustavo.Core/Security/PasswordEncryptor.cs)). Quien tenga la `EncryptKey` y acceso a la base puede recuperarlas.
3. **CORS abierto** a cualquier origen ([Configure.cs:59](../DistribuidoraGustavo.API/Configure.cs#L59)), y `UseCors` está después de `UseAuthorization` ([Program.cs:45-47](../DistribuidoraGustavo.API/Program.cs#L45)); el orden recomendado es antes de autenticación y autorización.
4. **Swagger expuesto en todos los entornos** ([Program.cs:38](../DistribuidoraGustavo.API/Program.cs#L38)).
5. **Las excepciones se devuelven al cliente** (`Result.Error(ex)` incluye el detalle en `FullException`).
6. **Descarga con `eval`** construyendo JavaScript por interpolación ([Invoices.razor.cs:89](../DistribuidoraGustavo.App/Pages/Invoices.razor.cs#L89)). Hoy los valores son controlados, pero conviene una función en `site.js`.
7. Sin autorización por roles: cualquier usuario activo puede hacer todo.

## Sesión y manejo de errores

1. **`Log.Error` no hace nada** ([Log.cs:6](../DistribuidoraGustavo.Interfaces/Shared/Log.cs#L6)). Sumado a que los controllers atrapan todo, los errores de producción son invisibles del lado servidor.
2. **Token vencido no redirige al login.** Pasados los 180 minutos la API responde 401 y `ApiClient` lo devuelve como error genérico con mensaje vacío ([ApiClient.cs:64-76](../DistribuidoraGustavo.App/Http/ApiClient.cs#L64)); las pantallas quedan vacías o muestran una alerta sin texto.
3. **Sin logout.**
4. En `DownloadInvoice`, si falla la obtención del token no hay `return`: igual se abre la pestaña de descarga con token vacío ([Invoices.razor.cs:82-89](../DistribuidoraGustavo.App/Pages/Invoices.razor.cs#L82)).
5. `ClientService.GetById` y `ProductService.GetById` no controlan `null`: un id inexistente termina en `NullReferenceException` (que llega al cliente como error).
6. Los filtros de búsqueda se concatenan a la URL sin escapar; caracteres como `&`, `#` o `+` rompen la búsqueda.
7. Métodos `async void` (`SearchTransactions`, `UploadFile`, `SearchProducts` de la factura): las excepciones dentro de ellos no se pueden capturar desde quien los llama.

## Arquitectura

1. **`Interfaces` depende de `Data`**, y `App` de `Interfaces`: el frontend incluye las entidades y EF Core en su bundle. Incluso hay `using DistribuidoraGustavo.Data.EfModels` y `using Microsoft.Data.SqlClient` sin uso en páginas como [Balances.razor.cs:3-8](../DistribuidoraGustavo.App/Pages/Balances.razor.cs#L3). Separarlo implicaría mover `CastEfToModel` a `Core` y sacar `Product`/`PriceList` de la firma de `IPriceListService.CalculatePrices`.
2. **`JWT_SecretKey` debe tener al menos 32 caracteres.** `System.IdentityModel.Tokens.Jwt` 6.36.0 rechaza claves más cortas para HS256 (error `IDX10720` al generar el token, es decir, en el login). Vale para todos los entornos.
3. **`try/catch` repetido** en cada acción; candidato a un filtro o middleware.
4. Paquetes sin uso en `Core`: `EPPlus` y `NPOI` (solo aparecen en `using`). Ojo con las licencias si se empiezan a usar: EPPlus ≥ 5 no es libre para uso comercial, e iText 7 (en uso) es AGPL o licencia comercial.
5. **.NET 6 está fuera de soporte** (finalizó en noviembre de 2024); EF Core 7 también.
6. `eTransactionType` vive en `Core`, por lo que el frontend compara contra strings si necesita distinguir tipos.
7. Sin tests, sin CI, sin seed de datos.

## Funcionalidad incompleta

De [todo.txt](../todo.txt) y de lo observado:

- Logs.
- ABM de listas de precios.
- Buscador que separe el filtro en palabras.
- Baja de productos.
- Edición de precio al hacer clic en el input.
- `QuantityPerBox` no editable desde la pantalla de producto.
- `Invoice.Description` existe en modelo y base pero ninguna pantalla lo carga (en cada edición se pisa con `null`).
- La pantalla de precios asume que cada producto tiene precio en todas las listas; si falta alguno, las columnas se desalinean.
- Listados con tope fijo y sin paginado: 50 facturas, 200 movimientos, 100/300 productos (estos últimos sin `OrderBy`, por lo que el recorte no es determinístico).
- El cambio de `ApiUrl` entre local y producción es manual; es fácil publicar apuntando a `localhost`.
- [AddOrEditProduct.razor](../DistribuidoraGustavo.App/Pages/AddOrEditProduct.razor) tiene `class="rowform-row"` (falta un espacio) y varias páginas usan la etiqueta `<diva>` en lugar de `<div>`.
