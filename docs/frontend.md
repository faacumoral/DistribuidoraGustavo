# Frontend (Blazor WebAssembly)

[← README](../README.md)

Proyecto: [DistribuidoraGustavo.App](../DistribuidoraGustavo.App/). Blazor WASM hosteado por el proyecto `API` (no tiene servidor propio ni se ejecuta solo), .NET 10, con soporte PWA.

## Estructura

```
DistribuidoraGustavo.App/
├── Program.cs            DI: ApiClient (HttpClient tipado), SessionStorage
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
    ├── index.html        Carga Bootstrap local, Bootstrap Icons por CDN, site.js
    ├── site.js           FocusElement(id), ClickElement(id)
    ├── css/app.css       Estilos globales (font-size 13px)
    └── service-worker*.js  PWA (offline solo en publish)
```

Paquetes: `Microsoft.AspNetCore.Components.WebAssembly` 10.0.12, `Microsoft.Extensions.Http` 10.0.12, `Blazored.SessionStorage` 2.3.0.

## Configuración

No tiene configuración propia. La API está en el mismo origen que la app, así que la dirección base se arma en [Program.cs](../DistribuidoraGustavo.App/Program.cs) con `HostEnvironment.BaseAddress + "api/"` y no hay nada que cambiar entre local y producción.

Para ejecutarlo se levanta el proyecto `API`, que lo sirve (ver [README](../README.md#cómo-levantarlo-en-local)).

## Acceso a la API

[ApiClient](../DistribuidoraGustavo.App/Http/ApiClient.cs) se registra con `AddHttpClient<ApiClient>` y se inyecta en cada página.

```csharp
var result = await ApiClient.Send<ListResult<ClientModel>>(ApiRequest.BuildGet("Clients"));
if (result.Success) Clients = result.ResultOk;
else Alert.ShowError(result.ResultError.FriendlyErrorMessage);
```

Comportamiento de `Send<TResult>`:

1. Lee el JWT de `sessionStorage`. Si no hay y el request tiene `CheckAuth = true` (default), navega a `/Login` y devuelve un result con `ResultOperation.Unauthorized`.
2. Arma la URL concatenando `BaseAddress + request.Url`. `BaseAddress` es el origen de la app más `api/`, así que la URL relativa va sin barra inicial y sin el prefijo (`"Clients"`, no `"/api/Clients"`).
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
| `Families` | `/families` | [Families](../DistribuidoraGustavo.App/Pages/Families.razor) | Listado y búsqueda de familias. |
| `AddOrEditFamily` | `/addoreditfamily[/{FamilyId:int}]` | [AddOrEditFamily](../DistribuidoraGustavo.App/Pages/AddOrEditFamily.razor) | Alta, edición y baja (bloqueada si tiene productos). |
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

## Pantallas simplificadas (`simple-ui`)

El usuario principal es una persona mayor con poco contacto con la tecnología. Las pantallas se van pasando de a una a un estilo de letra y controles grandes, definido una sola vez en [app.css](../DistribuidoraGustavo.App/wwwroot/css/app.css) bajo la clase `simple-ui`. Ya migradas: `Products`, `AddOrEditProduct`, `Families` y `AddOrEditFamily`; el resto sigue con letra de 14 px.

El aspecto general (tipografía del sistema, superficies planas, bordes finos de 0,5 px, botones de contorno y verde claro para la acción principal, colores de aviso suaves) se define con variables CSS al principio de la sección de tema de [app.css](../DistribuidoraGustavo.App/wwwroot/css/app.css) y se aplica a todo el sitio sobreescribiendo Bootstrap; la barra lateral está en [NavMenu.razor.css](../DistribuidoraGustavo.App/Shared/NavMenu.razor.css) y [MainLayout.razor.css](../DistribuidoraGustavo.App/Shared/MainLayout.razor.css). `simple-ui` solo agrega los tamaños grandes.

Para migrar una pantalla se envuelve su contenido en `<div class="simple-ui">` y se usan estas clases:

| Clase | Uso |
|---|---|
| `page-header` + `<h1>` | Título de la pantalla; en los listados lleva al lado el buscador y la acción principal. En edición el `<h1>` es el nombre de lo que se edita, con `page-kicker` arriba ("Editar producto") |
| `back-link` | "Volver a ..." arriba del título |
| `form-layout` | Formulario en dos columnas: campos a la izquierda, `summary-card` a la derecha |
| `field-row` (`short-first`) | Dos campos por renglón; `short-first` cuando el primero es corto (código + nombre) |
| `field`, `field-label`, `field-help`, `field-error` | Un campo: etiqueta arriba, ayuda corta, error debajo |
| `readonly-value` | Dato que no se puede editar, mostrado como texto |
| `input-with-prefix` | Campo con `$` adelante |
| `summary-card`, `summary-title`, `summary-line`, `summary-value` | Recuadro fijo a la derecha con el resultado en vivo y los botones de guardar |
| `form-actions` | Fila de botones |
| `danger-zone`, `confirm-box` | Acción destructiva separada del resto, con confirmación en la misma pantalla |
| `search-row` | Buscador con botón "Buscar" |
| `data-table` (`num`, `strong`, `actions`, `clickable`) | Listado en tabla: `num` alinea a la derecha, `strong` resalta, `actions` es la columna de botones. `clickable` en el `<tr>` cuando toda la fila abre el detalle (el `@onclick` va en la fila; el botón queda como indicación visual) |

El uso principal es en escritorio, así que el diseño aprovecha el ancho. Por debajo de 992 px el formulario pasa a una sola columna.

Criterios que siguen estas pantallas:

- Título visible y forma de volver sin guardar.
- Textos en lenguaje llano y en voseo ("Escribí el nombre del producto").
- Los errores de validación se muestran junto al campo y **no desaparecen solos** (`ShowError(msg, false)`); el cartel general se oculta cuando se corrigen los campos.
- Después de guardar o dar de baja se vuelve al listado con un aviso de confirmación (parámetro `?done=` que lee `ProductsBase`).
- Las acciones destructivas piden confirmación explicando la consecuencia.
- Importes: se parsean con [Numbers.TryParseAmount](../DistribuidoraGustavo.App/Utils/Numbers.cs), que acepta `1500`, `1500,50`, `1.500` y `1.500,50`, y se muestran con `ToMoney()` (`$ 1.500,50`), sin depender de la cultura del navegador.

## Componentes compartidos

| Componente | Parámetros / API | Notas |
|---|---|---|
| [Alerts](../DistribuidoraGustavo.App/Shared/Alerts.razor) | `Seconds` (default 3); `ShowSuccess(msg, hideAfterTime = true)`, `ShowError(...)`, `Hide()` | Se usa por `@ref`. Se oculta solo a los `Seconds` segundos, salvo que se pase `hideAfterTime = false`. |
| [Spinner](../DistribuidoraGustavo.App/Shared/Spinner.razor) | `Show` | Spinner de Bootstrap. |
| [NumberInput](../DistribuidoraGustavo.App/Shared/NumberInput.razor) | `Value` (int), `Disabled`, `HasChanged` (`Action<int>`) | Callback sincrónico, no `EventCallback`. Valor inválido → 0. |
| [DecimalInput](../DistribuidoraGustavo.App/Shared/DecimalInput.razor) | `Value` (decimal), `Disabled`, `HasChanged` (`Action<decimal>`) | Ídem. Parsea con la cultura del navegador/proceso. |
| [SearchInput](../DistribuidoraGustavo.App/Shared/SearchInput.razor) | `SearchValue`, `Placeholder`, `Disabled`, `OnValueChanged`, `OnSearch`; método `Focus()` | *Debounce* de 2 s; Enter busca de inmediato. El `id` del input es fijo (`txtSearchInput`), así que solo puede haber uno por página. |
| [HiddenFileInput](../DistribuidoraGustavo.App/Shared/HiddenFileInput.razor) | `OnChange` (`EventCallback<MultipartFormDataContent>`); método `Open()` | `InputFile` de Blazor oculto; `Open()` lo dispara por JS. Arma el multipart con el campo `file`. Tamaño máximo: 20 MB. |
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
- El service worker publicado **no** responde con `index.html` las navegaciones a `/api/...` ni a `/swagger`: las deja pasar al servidor. Es necesario para la descarga del PDF, que se abre en una pestaña nueva.
