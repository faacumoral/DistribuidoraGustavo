# Base de datos

[← README](../README.md)

SQL Server. El esquema se maneja **code first**: la fuente de verdad son las entidades y el `DbContext` de [DistribuidoraGustavo.Data/EfModels](../DistribuidoraGustavo.Data/EfModels/), y los cambios se aplican con migraciones de EF Core ([Migrations/](../DistribuidoraGustavo.Data/Migrations/)). No hay vistas, stored procedures ni datos semilla.

## Diagrama

```mermaid
erDiagram
    PriceLists ||--o{ Clients : "DefaultPriceListID"
    PriceLists ||--o{ Invoices : "PriceListID"
    PriceLists ||--o{ Products_PriceLists : ""
    Products   ||--o{ Products_PriceLists : ""
    Families   ||--o{ Products_Families : ""
    Products   ||--o| Products_Families : ""
    Products   ||--o{ Invoices_Products : ""
    Invoices   ||--o{ Invoices_Products : ""
    Clients    ||--o{ Invoices : ""
    Clients    ||--o{ Transactions : ""
    Users      ||--o{ UserTokens : ""

    Clients { int ClientID PK
              varchar Name
              varchar InvoicePrefix
              int DefaultPriceListID FK
              decimal ActualBalance }
    PriceLists { int PriceListId PK
                 varchar Name
                 decimal Percent }
    Products { int ProductID PK
               varchar Code
               varchar Name
               varchar Description
               bit Active
               decimal BasePrice
               int QuantityPerBox }
    Families { int FamilyID PK
               nvarchar Name UK
               varchar CodeSuffix UK }
    Products_Families { int ProductFamilyID PK
                        int ProductID FK,UK
                        int FamilyID FK }
    Products_PriceLists { int ProductPriceListID PK
                          int ProductID FK
                          int PriceListID FK
                          decimal Price }
    Invoices { int InvoiceID PK
               varchar InvoiceNumber UK
               int ClientID FK
               datetime CreatedDate
               bit Active
               int PriceListID FK
               nvarchar Description }
    Invoices_Products { int InvoicesProductsID PK
                        int ProductID FK
                        int InvoiceID FK
                        decimal UnitPrice
                        int Quantity
                        decimal Amount }
    Transactions { int TransactionID PK
                   decimal Amount
                   datetime Date
                   int ClientID FK
                   nvarchar Description
                   nvarchar Type }
    Users { int UserID PK
            varchar Username
            varchar Name
            varchar Password
            bit Active }
    UserTokens { int UserTokenID PK
                 int UserID FK
                 datetime ExpireTime
                 nvarchar Token
                 nvarchar Data }
    ImportedFiles { int ImportedFileID PK
                    nvarchar FileName
                    datetime DateTime
                    nvarchar FileContent }
```

## Tablas

| Tabla | Propósito | Detalles relevantes |
|---|---|---|
| `Users` | Usuarios del sistema | `Password` guarda el texto encriptado (reversible). `Username` sin restricción de unicidad. |
| `UserTokens` | Tokens de un solo uso | Hoy solo para descarga de PDF: `Data` = id de factura. |
| `Clients` | Clientes | `InvoicePrefix` identifica las facturas del cliente (unicidad validada solo en código). `ActualBalance` es el saldo de cuenta corriente desnormalizado. |
| `PriceLists` | Listas de precios | `Name` máx. 10 caracteres. `Percent` es el recargo sobre el precio base. |
| `Products` | Productos | `Code` sin índice único (validado en código). `Active` para baja lógica. |
| `Families` | Familias de productos | `Name` (máx. 100) y `CodeSuffix` (máx. 20) únicos. El subfijo es el prefijo con el que arrancan los códigos de los productos nuevos. |
| `Products_Families` | Familia de cada producto | Tabla aparte a propósito: hoy un producto tiene **una** familia (índice único en `ProductID`); para permitir varias basta con quitar ese índice. Un producto sin fila no tiene familia. |
| `Products_PriceLists` | Precio de cada producto en cada lista | Valor **calculado y persistido**. Sin unicidad sobre (`ProductID`, `PriceListID`). |
| `Invoices` | Cabecera de factura | `InvoiceNumber` único global, formato `{prefijo}-{n}`. `Active` para baja lógica (default `0` en la base; el código siempre inserta `true`). |
| `Invoices_Products` | Ítems de factura | Guarda `UnitPrice`, `Quantity` y `Amount` al momento de facturar. |
| `Transactions` | Movimientos de cuenta corriente | `Type` = `"Invoice"` o `"Payment"` (nombre del enum). Las de factura se vinculan por `Description = InvoiceNumber`, sin FK. Pagos con importe negativo. |
| `ImportedFiles` | Respaldo de los Excel importados | `FileContent` en base64. Se conservan los últimos ~5. |

Todos los importes son `DECIMAL(10,2)`: el máximo representable es 99.999.999,99.

Fechas en UTC (`GETUTCDATE()` como default en `Invoices.CreatedDate` y `Transactions.Date`).

## Capa EF Core

- Las entidades de [EfModels/](../DistribuidoraGustavo.Data/EfModels/) y el mapeo en [DistribuidoraGustavoContext.cs](../DistribuidoraGustavo.Data/EfModels/DistribuidoraGustavoContext.cs) se **mantienen a mano**. Nacieron de un scaffold (de ahí el estilo y las clases `partial`), pero ya no se regeneran.
- Nombres de entidades: `Invoices_Products` → `InvoicesProduct`, `Products_PriceLists` → `ProductsPriceList`, `Products_Families` → `ProductsFamily`.
- Los nombres de columna terminados en `ID` se fijan con `HasColumnName` para respetar el esquema original.
- No hay lazy loading: las navegaciones se cargan con `Include` explícito.
- Las FKs están configuradas con `DeleteBehavior.ClientSetNull` (sin borrado en cascada).

## Herramientas

`dotnet-ef` está declarado como herramienta local en [dotnet-tools.json](../dotnet-tools.json). Después de clonar:

```bash
dotnet tool restore
```

Los comandos se ejecutan desde la raíz del repo, con `Data` como proyecto de migraciones y `API` como proyecto de inicio (de ahí sale la connection string, en entorno `Development`).

## Crear la base local desde cero

```bash
dotnet ef database update --project DistribuidoraGustavo.Data --startup-project DistribuidoraGustavo.API
```

Crea la base si no existe y aplica todas las migraciones. Después hay que cargar a mano al menos una lista de precios y un usuario (ver [README](../README.md#cómo-levantarlo-en-local)).

## Cómo cambiar el esquema

1. Editar la entidad en `EfModels/` y, si hace falta, su configuración en `OnModelCreating` (tipo de columna, longitud, default, nombre).
2. Generar la migración:
   ```bash
   dotnet ef migrations add NombreDelCambio --project DistribuidoraGustavo.Data --startup-project DistribuidoraGustavo.API
   ```
3. **Revisar el archivo generado** en `Migrations/`: que haga solo lo esperado y que no pierda datos (un rename mal detectado se genera como borrar y crear la columna).
4. Aplicarla en local con `dotnet ef database update` (mismo comando de arriba).
5. Actualizar `CastEfToModel`, los `*Model` y los servicios que usen el campo nuevo.

Una migración ya aplicada en alguna base no se edita: se corrige con una migración nueva. Si todavía no se aplicó en ningún lado, se puede descartar con `dotnet ef migrations remove`.

## Desplegar cambios

La API aplica las migraciones pendientes al arrancar (`Database.Migrate()` en `Program.cs`), así que desplegar la API alcanza: ver [ci-cd.md](ci-cd.md). El usuario de la connection string necesita permisos de DDL.

Si hace falta aplicarlas a mano, generar un script idempotente (se puede correr contra cualquier base, aplica solo lo que falte):

```bash
dotnet ef migrations script --idempotent --project DistribuidoraGustavo.Data --startup-project DistribuidoraGustavo.API -o migracion.sql
```

## Adoptar una base que ya existe

Una base creada con el proyecto SSDT anterior (producción, o una local vieja) ya tiene las tablas, así que la migración inicial no debe ejecutarse: hay que marcarla como aplicada. Para eso está [sql/adoptar-base-existente.sql](sql/adoptar-base-existente.sql), que se corre **una sola vez por base**:

- crea `__EFMigrationsHistory` e inserta la fila de `InitialCreate`;
- crea los índices sobre claves foráneas, que la migración inicial incluye y el esquema anterior no tenía.

Diferencias que quedan entre una base adoptada y una creada desde cero, a tener en cuenta si una migración futura toca esos objetos:

- **Nombres de claves primarias.** El modelo asume `PK_Clients`, `PK_Invoices`, etc.; en una base adoptada tienen nombres autogenerados (`PK__Clients__...`).
- **Unicidad de `Invoices.InvoiceNumber`.** El modelo asume un índice único llamado `UQ_Invoices_InvoiceNumber`; en una base adoptada es una restricción `UNIQUE` con nombre autogenerado.

- **Texto de los defaults.** Son equivalentes pero no idénticos: por ejemplo `((0.0))` y `(CONVERT([bit],(1)))` en una base nueva contra `((0))` y `((1))` en una adoptada.

Mientras ninguna migración modifique una clave primaria, esa unicidad o esos defaults, la diferencia no tiene efecto.

Esto se verificó en SQL Server 2022: una base creada con los scripts del proyecto SSDT y adoptada con el script queda igual a una creada por la migración en columnas, tipos, nulabilidad, claves foráneas e índices, y `dotnet ef database update` no intenta aplicarle nada. No se verificó contra la base de producción real, que puede tener diferencias propias: conviene compararla antes del primer despliegue.
