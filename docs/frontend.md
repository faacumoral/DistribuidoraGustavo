# Frontend (Blazor WebAssembly)

[← README](../README.md)

Proyecto: [DistribuidoraGustavo.App](../DistribuidoraGustavo.App/). Blazor WASM *standalone* (no hosteado por la API), .NET 6, con soporte PWA.

## Estructura

```
DistribuidoraGustavo.App/
├── Program.cs            DI: ApiClient (HttpClient tipado), FileReader, SessionStorage
├── App.razor             Router; layout por defecto MainLayout
├── _Imports.razor        usings globales
├── Http/
│   ├── ApiClient.cs      Único acceso a la API
│   └── ApiRequest.cs     Descriptor de request (BuildGet / BuildPost / BuildDelete)
├── Pages/                Una página = X.razor + X.razor.cs (clase XBase)
├── Shared/               Layouts y componentes reutilizables
├── UIModels/             Modelos solo de UI (UIProductModel)
├── Utils/
│   ├── Views.cs          Enum de vistas (ruteo)
│   └── Extensions.cs     Helpers de sessionStorage y navegación
└── wwwroot/
    ├── appsettings.json  ApiUrl
    ├── index.html        Carga Bootstrap local, Bootstrap Icons por CDN, site.js
    ├── site.js           FocusElement(id), ClickElement(id)
    ├── css/app.css       Estilos globales (font-size 13px)
    └── service-worker*.js  PWA (offline solo en publish)
```

Paquetes: `Blazored.SessionStorage` 2.3.0, `Tewr.Blazor.FileReader` 3.3.1, `Microsoft.Extensions.Http` 6.0.0.

## Configuración

[wwwroot/appsettings.json](../DistribuidoraGustavo.App/wwwroot/appsettings.json) tiene una sola clave, `ApiUrl` (con barra final). El cambio entre local y producción se hace **comentando y descomentando a mano** la línea correspondiente; no hay `appsettings.{Environment}.json`.

## Acceso a la API

[ApiClient](../DistribuidoraGustavo.App/Http/ApiClient.cs) se registra con `AddHttpClient<ApiClient>` y se inyecta en cada página.

```csharp
var result = await ApiClient.Send<ListResult<ClientModel>>(ApiRequest.BuildGet("Clients"));
if (result.Success) Clients = result.ResultOk;
else Alert.ShowError(result.ResultError.FriendlyErrorMessage);
```

Comportamiento de `Send<TResult>`:

1. Lee el JWT de `sessionStorage`. Si no hay y el request tiene `CheckAuth = true` (default), navega a `/Login` y devuelve un result con `ResultOperation.Unauthorized`.
2. Arma la URL concatenando `BaseAddress + request.Url` (la URL relativa va sin barra inicial).
3. Si `Parameter != null`, lo serializa como JSON en el body.
4. Respuesta 2xx: deserializa a `TResult`. Cualquier otro status: devuelve un result de error con el body como mensaje. **Un 401 por token vencido no redirige al login.**

`SendFormData<TResult>(url, MultipartFormDataContent)` hace lo mismo para subir archivos (siempre POST, siempre autenticado).

Los parámetros de query se arman a mano en el string de la URL, sin `Uri.EscapeDataString`.

## Sesión y autenticación

- `sessionStorage` (se pierde al cerrar la pestaña), con claves definidas en [Utils/Extensions.cs](../DistribuidoraGustavo.App/Utils/Extensions.cs#L10):
  - `DistGustavo_JWT`: el token.
  - `DistDigus_UserLog`: el `UserModel` serializado.
- Helpers: `SetJwt/GetJwt/ExistsJwt`, `SetUser/GetUser`.
- [Login](../DistribuidoraGustavo.App/Pages/Login.razor.cs) guarda ambos y navega a `Home`.
- Protección de rutas: solo [Index](../DistribuidoraGustavo.App/Pages/Index.razor.cs) verifica explícitamente que exista el JWT. El resto de las páginas queda protegido indirectamente porque su primera llamada a `ApiClient.Send` redirige si no hay token.
- No hay logout ni validación de expiración del lado cliente.

## Ruteo

[Views.cs](../DistribuidoraGustavo.App/Utils/Views.cs) y las rutas `@page`:

| `Views` | Ruta | Página | Descripción |
|---|---|---|---|
| `Login` | `/login` | [Login](../DistribuidoraGustavo.App/Pages/Login.razor) | Usa `LoginLayout` (sin menú). |
| `Home` | `/`, `/home` | [Index](../DistribuidoraGustavo.App/Pages/Index.razor) | Bienvenida; redirige a login si no hay JWT. |
| `Invoices` | `/invoices` | [Invoices](../DistribuidoraGustavo.App/Pages/Invoices.razor) | Listado con filtro por cliente; editar, descargar PDF, eliminar. |
| `AddOrEditInvoice` | `/addoreditinvoice[/{invoiceId:int}]` | [AddOrEditInvoice](../DistribuidoraGustavo.App/Pages/AddOrEditInvoice.razor) | Alta y edición de factura. |
| `Clients` | `/clients` | [Clients](../DistribuidoraGustavo.App/Pages/Clients.razor) | Listado. |
| `AddOrEditClient` | `/addoreditclient[/{ClientId:int}]` | [AddOrEditClient](../DistribuidoraGustavo.App/Pages/AddOrEditClient.razor) | Alta y edición. |
| `Prices` | `/prices` | [Prices](../DistribuidoraGustavo.App/Pages/Prices.razor) | Productos con precio por lista; importación de Excel. |
| `Products` | `/products` | [Products](../DistribuidoraGustavo.App/Pages/Products.razor) | Listado y búsqueda. |
| `AddOrEditProduct` | `/addoreditproduct[/{ProductId:int}]` | [AddOrEditProduct](../DistribuidoraGustavo.App/Pages/AddOrEditProduct.razor) | Alta y edición. |
| `Balances` | `/balances` | [Balances](../DistribuidoraGustavo.App/Pages/Balances.razor) | Movimientos, saldo del cliente y registro de pagos. |

La navegación usa `Views.X.ToString()` como URL relativa; funciona porque el ruteo de Blazor no distingue mayúsculas. El menú lateral está en [NavMenu.razor](../DistribuidoraGustavo.App/Shared/NavMenu.razor).

## Patrón de página

```csharp
// X.razor.cs
public class XState { public bool Processing { get; set; } /* filtros, selección */ }

public class XBase : ComponentBase
{
    [Inject] ApiClient ApiClient { get; set; }
    [Inject] NavigationManager NavigationManager { get; set; }

    public Alerts Alert { get; set; }              // se asigna con @ref
    protected XState State { get; set; } = new();
    protected IList<TModel> Items { get; set; } = new List<TModel>();

    protected override async Task OnInitializedAsync() { /* cargar datos */ }
}
```

```razor
@* X.razor *@
@page "/x"
@inherits XBase
<Alerts @ref="Alert"></Alerts>
<Spinner Show="@State.Processing"></Spinner>
```

Convenciones observadas:

- Flags `Processing` / `Searching` / `Saving` para deshabilitar controles y mostrar el spinner.
- Los `<select>` se resuelven con `@onchange` + `int.Parse(e.Value.ToString())` y un `if/else` para marcar la opción `selected` (no se usa `@bind`).
- Errores: `Alert.ShowError(result.ResultError.FriendlyErrorMessage)`; el detalle técnico va a `Console.WriteLine`.
- Varias páginas inyectan `IJSRuntime` e `IConfiguration` aunque no los usen.

## Componentes compartidos

| Componente | Parámetros / API | Notas |
|---|---|---|
| [Alerts](../DistribuidoraGustavo.App/Shared/Alerts.razor) | `ShowSuccess(msg, hideAfterTime = true)`, `ShowError(...)` | Se usa por `@ref`. Se oculta solo a los 3 s. |
| [Spinner](../DistribuidoraGustavo.App/Shared/Spinner.razor) | `Show` | Spinner de Bootstrap. |
| [NumberInput](../DistribuidoraGustavo.App/Shared/NumberInput.razor) | `Value` (int), `Disabled`, `HasChanged` (`Action<int>`) | Callback sincrónico, no `EventCallback`. Valor inválido → 0. |
| [DecimalInput](../DistribuidoraGustavo.App/Shared/DecimalInput.razor) | `Value` (decimal), `Disabled`, `HasChanged` (`Action<decimal>`) | Ídem. Parsea con la cultura del navegador/proceso. |
| [SearchInput](../DistribuidoraGustavo.App/Shared/SearchInput.razor) | `SearchValue`, `Disabled`, `OnValueChanged`, `OnSearch`; método `Focus()` | *Debounce* de 2 s; Enter busca de inmediato. El `id` del input es fijo (`txtSearchInput`), así que solo puede haber uno por página. |
| [HiddenFileInput](../DistribuidoraGustavo.App/Shared/HiddenFileInput.razor) | `OnChange` (`EventCallback<MultipartFormDataContent>`); método `Open()` | Input de archivo oculto; `Open()` lo dispara por JS. Arma el multipart con el campo `file`. |
| [MainLayout](../DistribuidoraGustavo.App/Shared/MainLayout.razor) | — | Sidebar + barra superior con el nombre del usuario logueado. |
| [LoginLayout](../DistribuidoraGustavo.App/Shared/LoginLayout.razor) | — | Layout sin menú. |

## Interop con JavaScript

- [site.js](../DistribuidoraGustavo.App/wwwroot/site.js): `FocusElement(id)` y `ClickElement(id)`.
- La descarga del PDF abre una pestaña nueva con `JSRuntime.InvokeVoidAsync("eval", "... open(url, '_blank')")` en [Invoices.razor.cs:89](../DistribuidoraGustavo.App/Pages/Invoices.razor.cs#L89).

## Pantalla de factura (la más compleja)

[AddOrEditInvoice.razor.cs](../DistribuidoraGustavo.App/Pages/AddOrEditInvoice.razor.cs):

- Carga clientes y listas de precios; si viene `InvoiceId`, carga la factura.
- Al elegir cliente se selecciona automáticamente su lista de precios por defecto y se dispara la búsqueda de productos.
- La búsqueda de productos tiene su propio *debounce* de 2 s (no usa `SearchInput`) y llama a `GET /Products?filter=&priceListId=`, que devuelve cada producto con el `UnitPrice` de la lista elegida.
- Panel izquierdo: resultados con checkbox. Panel derecho: ítems de la factura.
- Dos botones para agregar los productos tildados: por **unidad** (cantidad 1) o por **bulto** (cantidad = `QuantityPerBox`). Si el producto ya está en la factura, suma cantidad e importe.
- En cada ítem se pueden editar cantidad y precio unitario (recalculan `Amount = Quantity × UnitPrice`) y también el **importe total del ítem directamente**, sin recalcular lo demás.
- Guardar: `POST /Invoices` con cliente, lista, productos e `InvoiceId`; al terminar vuelve al listado.

## PWA

- [service-worker.js](../DistribuidoraGustavo.App/wwwroot/service-worker.js) (desarrollo) no cachea nada.
- [service-worker.published.js](../DistribuidoraGustavo.App/wwwroot/service-worker.published.js) (publish) cachea los assets del manifest y sirve `index.html` offline. Tener en cuenta al desplegar: los usuarios pueden seguir viendo la versión anterior hasta que el service worker se actualice.
