# Dominio y reglas de negocio

[← README](../README.md)

## Conceptos

- **Producto**: tiene un código, un **precio base** y una **cantidad por bulto**.
- **Familia**: agrupa productos; tiene un nombre y un **subfijo** para los códigos (por ejemplo "Gaseosas" / `GAS`). Un producto tiene como máximo una familia.
- **Lista de precios**: un nombre y un **porcentaje** de recargo sobre el precio base.
- **Cliente**: tiene una lista de precios por defecto, un **prefijo de factura** y un **saldo** de cuenta corriente.
- **Factura**: pertenece a un cliente, tiene ítems con cantidad, precio unitario e importe.
- **Transacción**: movimiento de cuenta corriente. Puede ser una factura (suma deuda) o un pago (resta deuda).

"Factura" acá es un comprobante interno: no hay integración fiscal (AFIP/ARCA), ni impuestos, ni tipos de comprobante.

## Precios

Regla única, definida en [PriceCalculator](../DistribuidoraGustavo.Interfaces/Shared/PriceCalculator.cs) (compartida por la API y por la vista previa del formulario de producto) y aplicada en [PriceListService.CalculatePrices](../DistribuidoraGustavo.Core/Services/PriceListService.cs):

```
Precio en lista = techo( PrecioBase × (1 + Percent / 100) )
```

- Se redondea **hacia arriba al entero**.
- El resultado se **persiste** en `Products_PriceLists` (una fila por producto y lista).
- Se recalcula para todas las listas cuando:
  - se crea o modifica un producto (`ProductService.Upsert`);
  - se importa el Excel y el precio de la fila es válido.
- **No** se recalcula si se cambia el `Percent` de una lista o se agrega una lista nueva directamente en la base: los precios quedan desactualizados hasta que se toque cada producto o se reimporte el Excel.

## Productos

- Búsqueda ([ProductService.SearchProducts](../DistribuidoraGustavo.Core/Services/ProductService.cs#L24)): solo activos; el filtro busca por *contains* sin distinguir mayúsculas en nombre, código, descripción o **nombre de la familia**, como una sola cadena (escribir "gaseosas" trae también todos los productos de esa familia). Ordenado por código. Límite 100 (listado y facturación) o 300 (pantalla de precios).
- Alta: código y nombre obligatorios; el código no puede repetirse.
- Familia (opcional): al elegirla en el alta, el código se completa con el subfijo (sin guion; se reemplaza solo el subfijo anterior si se cambia de familia) y hay que agregarle el resto a mano. Cambiar el subfijo de una familia no toca los códigos existentes. La importación de Excel no asigna ni modifica familias.
- Familias: nombre y subfijo obligatorios y únicos; no se puede eliminar una familia que tenga productos.
- Modificación: cambia familia, nombre, descripción, precio base y unidades por bulto. El código no se modifica.
- `QuantityPerBox` (unidades por bulto) se carga desde el formulario de producto o por importación de Excel.
- El formulario no tiene campo de descripción: al guardar copia el nombre a `Description`, igual que la importación de Excel.
- Baja: es **lógica** (`Active = false`, [ProductService.Delete](../DistribuidoraGustavo.Core/Services/ProductService.cs)). El producto deja de aparecer en el listado, en la pantalla de precios y al facturar; las facturas que ya lo incluyen no cambian. Sus precios por lista se conservan.
- Un producto dado de baja **vuelve a estar activo** de dos formas: si se da de alta otro con el mismo código (se reutiliza el registro con los datos nuevos) o si el código viene en un Excel importado.

## Importación de Excel

`POST /PriceLists/uploadFile` → [PriceListService.ProcessFile](../DistribuidoraGustavo.Core/Services/PriceListService.cs#L64).

Formato esperado: primera hoja del libro, columnas por posición.

| Columna | Contenido | Uso |
|---|---|---|
| A (0) | Código | Clave para encontrar el producto |
| B (1) | Nombre | Se copia a `Name` y a `Description` |
| C (2) | Tipo de unidad | Se lee pero no se usa |
| D (3) | Cantidad por bulto | `QuantityPerBox` (0 si no es entero) |
| E (4) | Precio | `BasePrice` |

Comportamiento:

1. Guarda una copia del archivo en `ImportedFiles` (reemplaza la del mismo día; conserva unas 5).
2. Por cada fila: si el código existe, actualiza el producto y lo reactiva (`Active = true`); si no, lo crea.
3. Si el precio parsea, recalcula los precios de todas las listas. Si no, agrega un mensaje `'{valor}' no es un precio válido` a la lista de errores que devuelve el endpoint (el frontend solo la escribe en la consola).
4. Un único `SaveChangesAsync` al final.

A tener en cuenta:

- **Se procesan todas las filas, incluida una eventual fila de encabezado** (no se saltea ninguna).
- Una fila con precio inválido igual pisa el `BasePrice` con 0, sin recalcular las listas.
- Los productos que no vienen en el Excel no se tocan (no se dan de baja).
- El parseo de números usa la cultura del servidor.

## Clientes

- Alta: nombre, lista de precios por defecto y prefijo. El prefijo se guarda sin guiones finales y no puede repetirse.
- Modificación: solo nombre y lista por defecto. **El prefijo es inmutable** (la numeración de facturas depende de él).
- No hay baja de clientes.

## Facturas

Lógica en [InvoiceService.UpsertInvoice](../DistribuidoraGustavo.Core/Services/InvoiceService.cs#L56).

### Numeración

`InvoiceNumber = "{InvoicePrefix}-{n}"`, donde `n` es el máximo número existente entre **todas** las facturas del cliente (incluidas las dadas de baja) más 1, empezando en 1. Se calcula en memoria, quitando el prefijo del string y parseando el resto.

### Alta

1. Valida cliente existente y al menos un producto.
2. Crea la factura (`Active = true`, fecha UTC) con sus ítems tal como vienen del frontend (`Quantity`, `UnitPrice`, `Amount`). El backend **no recalcula ni valida importes**.
3. Crea una transacción tipo `Invoice` con `Description = InvoiceNumber` e importe = total.
4. Suma el total al saldo del cliente.

### Modificación

1. Resta del saldo del cliente el total anterior de la factura.
2. Reemplaza los ítems por los nuevos (los que coinciden por producto se actualizan, los que no están se eliminan, los nuevos se agregan).
3. Actualiza el importe de la transacción asociada.
4. Suma el nuevo total al saldo.

No se actualizan ni el cliente ni la lista de precios de la factura.

### Baja

[DeleteInvoice](../DistribuidoraGustavo.Core/Services/InvoiceService.cs#L198): baja lógica (`Active = false`), resta el total del saldo del cliente y **elimina físicamente** la transacción asociada. El número de factura no se reutiliza.

### Total

`InvoiceModel.TotalAmount` es una propiedad calculada: suma de `Amount` de los ítems. No se persiste en `Invoices`.

### Descarga del PDF

Flujo en dos pasos, porque la descarga se abre en una pestaña nueva y no puede enviar el header `Authorization`:

1. `GET /Invoices/{id}/downloadtoken` (autenticado) crea un `UserToken` con un GUID, vencimiento a 1 minuto y el id de factura en `Data`.
2. `GET /Invoices/download?token=...` (anónimo) valida el token, lo elimina (un solo uso) y devuelve el PDF con nombre `{InvoiceNumber}.pdf`.

El PDF se arma en [PdfService](../DistribuidoraGustavo.Core/Services/PdfService.cs) reemplazando tokens `{{Nombre}}` en [InvoiceTemplate.html](../DistribuidoraGustavo.Core/HTML/InvoiceTemplate.html):

| Token | Valor |
|---|---|
| `InvoiceNumber`, `ClientName` | Datos de la factura |
| `Date` | Fecha y hora **de la descarga** (no de la factura) |
| `TotalAmount` | Total de la factura |
| `ActualBalance` | Saldo **actual** del cliente |
| `OldBalance` | `ActualBalance − TotalAmount` |
| `InvoiceProducts` | Filas `<tr>` generadas por ítem (nombre truncado a 35 caracteres) |

Los saldos del PDF se calculan al momento de descargar, por lo que solo son coherentes si la factura es la última operación del cliente. Los totales usan formato `N0` en cultura `es-AR` (sin decimales); precio unitario e importe por ítem salen sin formato.

El template se copia al directorio de salida (`CopyToOutputDirectory=Always`) y se lee desde `AppDomain.CurrentDomain.BaseDirectory/HTML/InvoiceTemplate.html`.

## Cuenta corriente

- `Clients.ActualBalance` es el saldo. **Positivo = el cliente debe**; negativo o cero = saldo a favor (así lo muestra [Balances.razor](../DistribuidoraGustavo.App/Pages/Balances.razor)).
- El saldo es un campo desnormalizado que se va ajustando en cada operación; no se recalcula desde `Transactions`.
- Movimientos ([TransactionService](../DistribuidoraGustavo.Core/Services/TransactionService.cs)):

| Operación | Efecto en `Transactions` | Efecto en saldo |
|---|---|---|
| Alta de factura | Nueva, `Type = "Invoice"`, importe positivo | `+ total` |
| Modificación de factura | Actualiza importe | `− total anterior + total nuevo` |
| Baja de factura | Se elimina | `− total` |
| Pago | Nueva, `Type = "Payment"`, `Description = "Pago"`, importe **negativo** | `− importe` |

- Los pagos se cargan desde la pantalla de Saldos como **enteros positivos** (usa `NumberInput`). No se pueden editar ni anular.
- El listado muestra los últimos 200 movimientos, de todos los clientes o de uno.
