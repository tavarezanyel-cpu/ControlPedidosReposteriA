// ============================================================================
// Control de Pedidos de Repostería - Tarea 2 (base del proyecto)
// Datos en memoria: una lista de diccionarios por entidad (productos, clientes
// y pedidos) y un diccionario con los productos de cada pedido.
// Sintaxis moderna: instrucciones de nivel superior, new(), expresiones de
// colección, cadenas literales sin formato, inicializadores de diccionario y
// coincidencia de patrones.
// ============================================================================
using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

const string Pendiente = "Pendiente";
const string Entregado = "Entregado";

// Listas individuales: cada elemento es un diccionario campo -> valor
List<Dictionary<string, string>> productos = new();
List<Dictionary<string, string>> clientes = new();
List<Dictionary<string, string>> pedidos = new();

// IdPedido -> (IdProducto -> Cantidad)
Dictionary<int, Dictionary<int, int>> itemsPorPedido = new();

string[] categorias = ["Pastel", "Galleta", "Cupcake", "Postre", "Otro"];
int proximoIdProducto = 1, proximoIdCliente = 1, proximoIdPedido = 1;

// ---------------------------------------------------------------------------
// Ciclo principal: solo termina cuando el usuario elige 0 y confirma con "S"
// ---------------------------------------------------------------------------
bool salir = false;
while (!salir)
{
    try
    {
        MostrarMenuPrincipal();
        switch (LeerEntero("Seleccione una opción: ", 0, 3))
        {
            case 1: MenuProductos(); break;
            case 2: MenuClientes(); break;
            case 3: MenuPedidos(); break;
            case 0: salir = Confirmar("¿Está seguro de que desea salir?"); break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
        Pausa();
    }
}

Console.WriteLine("¡Hasta pronto!");

// ===========================================================================
// MENÚS
// ===========================================================================
void MostrarMenuPrincipal()
{
    Console.Clear();
    Console.WriteLine("""
        ===== CONTROL DE PEDIDOS - REPOSTERÍA =====
        1. Productos
        2. Clientes
        3. Pedidos
        0. Salir
        """);
}

// Limpia la pantalla y muestra el título de la opción elegida
void Titulo(string texto)
{
    Console.Clear();
    Console.WriteLine(texto);
}

void MostrarSubmenu(string titulo, bool conEntrega = false)
{
    Console.Clear();
    Console.WriteLine($"""
        ===== {titulo} =====
        1. Agregar
        2. Buscar
        3. Modificar
        4. Eliminar
        5. Listar
        """);
    if (conEntrega) Console.WriteLine("6. Marcar como entregado");
    Console.WriteLine("0. Volver");
}

void MenuProductos()
{
    bool volver = false;
    while (!volver)
    {
        MostrarSubmenu("PRODUCTOS");
        switch (LeerEntero("Seleccione una opción: ", 0, 5))
        {
            case 1: AgregarProducto(); break;
            case 2: BuscarProductos(); break;
            case 3: ModificarProducto(); break;
            case 4: EliminarProducto(); break;
            case 5: ListarProductos(); break;
            case 0: volver = true; break;
        }
    }
}

void MenuClientes()
{
    bool volver = false;
    while (!volver)
    {
        MostrarSubmenu("CLIENTES");
        switch (LeerEntero("Seleccione una opción: ", 0, 5))
        {
            case 1: AgregarCliente(); break;
            case 2: BuscarClientes(); break;
            case 3: ModificarCliente(); break;
            case 4: EliminarCliente(); break;
            case 5: ListarClientes(); break;
            case 0: volver = true; break;
        }
    }
}

void MenuPedidos()
{
    bool volver = false;
    while (!volver)
    {
        MostrarSubmenu("PEDIDOS", conEntrega: true);
        switch (LeerEntero("Seleccione una opción: ", 0, 6))
        {
            case 1: AgregarPedido(); break;
            case 2: BuscarPedidos(); break;
            case 3: ModificarPedido(); break;
            case 4: EliminarPedido(); break;
            case 5: ListarPedidos(); break;
            case 6: MarcarEntregado(); break;
            case 0: volver = true; break;
        }
    }
}

// ===========================================================================
// PRODUCTOS  (ID, NOMBRE, CATEGORIA, PRECIO)
// ===========================================================================
void AgregarProducto()
{
    Titulo("--- AGREGAR PRODUCTO ---");
    Dictionary<string, string> producto = new()
    {
        ["ID"] = proximoIdProducto.ToString(),
        ["NOMBRE"] = LeerTexto("Nombre: "),
        ["CATEGORIA"] = ElegirCategoria(),
        ["PRECIO"] = LeerDecimal("Precio: ").ToString(CultureInfo.InvariantCulture)
    };

    productos.Add(producto);
    Console.WriteLine($"Producto agregado con ID {proximoIdProducto}.");
    proximoIdProducto++;
    Pausa();
}

void BuscarProductos()
{
    Titulo("--- BUSCAR PRODUCTOS ---");
    Console.WriteLine("1. Por ID\n2. Por nombre\n3. Por categoría\n4. Por rango de precio");

    List<Dictionary<string, string>> resultados;
    switch (LeerEntero("Criterio: ", 1, 4))
    {
        case 1:
            {
                string id = LeerEntero("ID: ").ToString();
                resultados = productos.Where(p => p["ID"] == id).ToList();
                break;
            }
        case 2:
            {
                string texto = LeerTexto("Nombre (o parte): ");
                resultados = productos
                    .Where(p => p["NOMBRE"].Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                break;
            }
        case 3:
            {
                string categoria = ElegirCategoria();
                resultados = productos.Where(p => p["CATEGORIA"] == categoria).ToList();
                break;
            }
        default:
            {
                decimal min = LeerDecimal("Precio mínimo: ");
                decimal max = LeerDecimal("Precio máximo: ");
                if (min > max) (min, max) = (max, min);
                resultados = productos.Where(p => PrecioDe(p) >= min && PrecioDe(p) <= max).ToList();
                break;
            }
    }

    MostrarProductos(resultados);
    Pausa();
}

void ModificarProducto()
{
    Titulo("--- MODIFICAR PRODUCTO ---");
    Dictionary<string, string>? producto = BuscarPorId(productos, LeerEntero("ID del producto: "));
    if (producto is null)
    {
        Console.WriteLine("Producto no encontrado.");
        Pausa();
        return;
    }

    Console.WriteLine("(Presione Enter para mantener el valor actual)");
    producto["NOMBRE"] = LeerTextoOpcional($"Nombre [{producto["NOMBRE"]}]: ", producto["NOMBRE"]);

    if (Confirmar($"¿Cambiar la categoría? (actual: {producto["CATEGORIA"]})"))
        producto["CATEGORIA"] = ElegirCategoria();

    Console.Write($"Precio [{PrecioDe(producto):N2}]: ");
    string? entrada = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(entrada))
    {
        if (decimal.TryParse(entrada, out decimal nuevoPrecio) && nuevoPrecio > 0)
            producto["PRECIO"] = nuevoPrecio.ToString(CultureInfo.InvariantCulture);
        else
            Console.WriteLine("Precio no válido; se mantiene el anterior.");
    }

    Console.WriteLine("Producto modificado.");
    Pausa();
}

void EliminarProducto()
{
    Titulo("--- ELIMINAR PRODUCTO ---");
    int id = LeerEntero("ID del producto: ");
    Dictionary<string, string>? producto = BuscarPorId(productos, id);

    if (producto is null)
    {
        Console.WriteLine("Producto no encontrado.");
    }
    else if (itemsPorPedido.Values.Any(items => items.ContainsKey(id)))
    {
        Console.WriteLine("No se puede eliminar: el producto está en uno o más pedidos.");
    }
    else if (Confirmar($"¿Eliminar '{producto["NOMBRE"]}'?"))
    {
        productos.Remove(producto);
        Console.WriteLine("Producto eliminado.");
    }
    Pausa();
}

void ListarProductos()
{
    Titulo("--- LISTA DE PRODUCTOS ---");
    MostrarProductos(productos);
    Pausa();
}

void MostrarProductos(List<Dictionary<string, string>> lista)
{
    if (lista.Count == 0)
    {
        Console.WriteLine("No hay productos para mostrar.");
        return;
    }

    Console.WriteLine($"{"ID",-5}{"Nombre",-25}{"Categoría",-12}{"Precio",12}");
    foreach (var p in lista)
        Console.WriteLine($"{p["ID"],-5}{p["NOMBRE"],-25}{p["CATEGORIA"],-12}{PrecioDe(p),12:N2}");
}

string ElegirCategoria()
{
    for (int i = 0; i < categorias.Length; i++)
        Console.WriteLine($"  {i + 1}. {categorias[i]}");
    int opcion = LeerEntero("Categoría: ", 1, categorias.Length);
    return categorias[opcion - 1];
}

decimal PrecioDe(Dictionary<string, string> producto) =>
    decimal.Parse(producto["PRECIO"], CultureInfo.InvariantCulture);

decimal PrecioProducto(int idProducto)
{
    Dictionary<string, string>? producto = BuscarPorId(productos, idProducto);
    return producto is null ? 0m : PrecioDe(producto);
}

// ===========================================================================
// CLIENTES  (ID, NOMBRE, TELEFONO, DIRECCION)
// ===========================================================================
void AgregarCliente()
{
    Titulo("--- AGREGAR CLIENTE ---");
    Dictionary<string, string> cliente = new()
    {
        ["ID"] = proximoIdCliente.ToString(),
        ["NOMBRE"] = LeerTexto("Nombre: "),
        ["TELEFONO"] = LeerTexto("Teléfono: "),
        ["DIRECCION"] = LeerTexto("Dirección: ")
    };

    clientes.Add(cliente);
    Console.WriteLine($"Cliente agregado con ID {proximoIdCliente}.");
    proximoIdCliente++;
    Pausa();
}

void BuscarClientes()
{
    Titulo("--- BUSCAR CLIENTES ---");
    Console.WriteLine("1. Por ID\n2. Por nombre\n3. Por teléfono");

    List<Dictionary<string, string>> resultados;
    switch (LeerEntero("Criterio: ", 1, 3))
    {
        case 1:
            {
                string id = LeerEntero("ID: ").ToString();
                resultados = clientes.Where(c => c["ID"] == id).ToList();
                break;
            }
        case 2:
            {
                string texto = LeerTexto("Nombre (o parte): ");
                resultados = clientes
                    .Where(c => c["NOMBRE"].Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                break;
            }
        default:
            {
                string telefono = LeerTexto("Teléfono (o parte): ");
                resultados = clientes.Where(c => c["TELEFONO"].Contains(telefono)).ToList();
                break;
            }
    }

    MostrarClientes(resultados);
    Pausa();
}

void ModificarCliente()
{
    Titulo("--- MODIFICAR CLIENTE ---");
    Dictionary<string, string>? cliente = BuscarPorId(clientes, LeerEntero("ID del cliente: "));
    if (cliente is null)
    {
        Console.WriteLine("Cliente no encontrado.");
        Pausa();
        return;
    }

    Console.WriteLine("(Presione Enter para mantener el valor actual)");
    cliente["NOMBRE"] = LeerTextoOpcional($"Nombre [{cliente["NOMBRE"]}]: ", cliente["NOMBRE"]);
    cliente["TELEFONO"] = LeerTextoOpcional($"Teléfono [{cliente["TELEFONO"]}]: ", cliente["TELEFONO"]);
    cliente["DIRECCION"] = LeerTextoOpcional($"Dirección [{cliente["DIRECCION"]}]: ", cliente["DIRECCION"]);

    Console.WriteLine("Cliente modificado.");
    Pausa();
}

void EliminarCliente()
{
    Titulo("--- ELIMINAR CLIENTE ---");
    Dictionary<string, string>? cliente = BuscarPorId(clientes, LeerEntero("ID del cliente: "));

    if (cliente is null)
    {
        Console.WriteLine("Cliente no encontrado.");
    }
    else if (pedidos.Any(p => p["CLIENTE_ID"] == cliente["ID"]))
    {
        Console.WriteLine("No se puede eliminar: el cliente tiene pedidos registrados.");
    }
    else if (Confirmar($"¿Eliminar a '{cliente["NOMBRE"]}'?"))
    {
        clientes.Remove(cliente);
        Console.WriteLine("Cliente eliminado.");
    }
    Pausa();
}

void ListarClientes()
{
    Titulo("--- LISTA DE CLIENTES ---");
    MostrarClientes(clientes);
    Pausa();
}

void MostrarClientes(List<Dictionary<string, string>> lista)
{
    if (lista.Count == 0)
    {
        Console.WriteLine("No hay clientes para mostrar.");
        return;
    }

    Console.WriteLine($"{"ID",-5}{"Nombre",-25}{"Teléfono",-15}Dirección");
    foreach (var c in lista)
        Console.WriteLine($"{c["ID"],-5}{c["NOMBRE"],-25}{c["TELEFONO"],-15}{c["DIRECCION"]}");
}

string NombreCliente(string idCliente) =>
    clientes.Find(c => c["ID"] == idCliente)?["NOMBRE"] ?? "(cliente eliminado)";

// ===========================================================================
// PEDIDOS  (ID, CLIENTE_ID, FECHA, ESTADO) + itemsPorPedido
// ===========================================================================
void AgregarPedido()
{
    Titulo("--- CREAR PEDIDO ---");
    if (clientes.Count == 0 || productos.Count == 0)
    {
        Console.WriteLine("Debe registrar al menos un cliente y un producto antes de crear un pedido.");
        Pausa();
        return;
    }

    MostrarClientes(clientes);
    Dictionary<string, string>? cliente = BuscarPorId(clientes, LeerEntero("ID del cliente: "));
    if (cliente is null)
    {
        Console.WriteLine("Cliente no encontrado.");
        Pausa();
        return;
    }

    Dictionary<int, int> items = new();
    MostrarProductos(productos);
    AgregarItems(items);

    if (items.Count == 0)
    {
        Console.WriteLine("El pedido no tiene productos; no fue creado.");
        Pausa();
        return;
    }

    Dictionary<string, string> pedido = new()
    {
        ["ID"] = proximoIdPedido.ToString(),
        ["CLIENTE_ID"] = cliente["ID"],
        ["FECHA"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
        ["ESTADO"] = Pendiente
    };

    pedidos.Add(pedido);
    itemsPorPedido[proximoIdPedido] = items;
    Console.WriteLine($"Pedido #{proximoIdPedido} creado. Total: {TotalPedido(items):N2}");
    proximoIdPedido++;
    Pausa();
}

void AgregarItems(Dictionary<int, int> items)
{
    while (true)
    {
        int idProducto = LeerEntero("ID del producto (0 para terminar): ", 0);
        if (idProducto == 0) break;

        if (BuscarPorId(productos, idProducto) is null)
        {
            Console.WriteLine("Producto no encontrado.");
            continue;
        }

        int cantidad = LeerEntero("Cantidad: ", 1, 1000);
        items[idProducto] = items.GetValueOrDefault(idProducto) + cantidad;
    }
}

void BuscarPedidos()
{
    Titulo("--- BUSCAR PEDIDOS ---");
    Console.WriteLine("1. Por ID\n2. Por nombre del cliente\n3. Por estado");

    List<Dictionary<string, string>> resultados;
    switch (LeerEntero("Criterio: ", 1, 3))
    {
        case 1:
            {
                string id = LeerEntero("ID: ").ToString();
                resultados = pedidos.Where(p => p["ID"] == id).ToList();
                break;
            }
        case 2:
            {
                string texto = LeerTexto("Nombre del cliente (o parte): ");
                resultados = pedidos
                    .Where(p => NombreCliente(p["CLIENTE_ID"]).Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                break;
            }
        default:
            {
                int opcion = LeerEntero("1. Pendiente  2. Entregado: ", 1, 2);
                string estado = opcion == 1 ? Pendiente : Entregado;
                resultados = pedidos.Where(p => p["ESTADO"] == estado).ToList();
                break;
            }
    }

    MostrarPedidos(resultados);
    Pausa();
}

void ModificarPedido()
{
    Titulo("--- MODIFICAR PEDIDO ---");
    int idPedido = LeerEntero("ID del pedido: ");
    Dictionary<string, string>? pedido = BuscarPorId(pedidos, idPedido);
    if (pedido is null)
    {
        Console.WriteLine("Pedido no encontrado.");
        Pausa();
        return;
    }

    if (pedido["ESTADO"] == Entregado)
    {
        Console.WriteLine("Un pedido entregado no se puede modificar.");
        Pausa();
        return;
    }

    Dictionary<int, int> items = itemsPorPedido[idPedido];

    bool terminado = false;
    while (!terminado)
    {
        MostrarPedidos([pedido]);
        Console.WriteLine("""

            1. Cambiar la cantidad de un producto
            2. Agregar productos
            3. Quitar un producto
            0. Terminar
            """);

        switch (LeerEntero("Opción: ", 0, 3))
        {
            case 1:
                {
                    int idProducto = LeerEntero("ID del producto: ");
                    if (!items.ContainsKey(idProducto))
                    {
                        Console.WriteLine("Ese producto no está en el pedido.");
                        break;
                    }
                    items[idProducto] = LeerEntero("Nueva cantidad: ", 1, 1000);
                    Console.WriteLine("Cantidad actualizada.");
                    break;
                }
            case 2:
                MostrarProductos(productos);
                AgregarItems(items);
                break;
            case 3:
                {
                    int idProducto = LeerEntero("ID del producto a quitar: ");
                    if (!items.ContainsKey(idProducto))
                        Console.WriteLine("Ese producto no está en el pedido.");
                    else if (items.Count == 1)
                        Console.WriteLine("El pedido debe tener al menos un producto. Para quitarlo todo, elimine el pedido.");
                    else
                    {
                        items.Remove(idProducto);
                        Console.WriteLine("Producto quitado del pedido.");
                    }
                    break;
                }
            case 0:
                terminado = true;
                break;
        }
    }
}

void EliminarPedido()
{
    Titulo("--- ELIMINAR PEDIDO ---");
    int idPedido = LeerEntero("ID del pedido: ");
    Dictionary<string, string>? pedido = BuscarPorId(pedidos, idPedido);

    if (pedido is null)
        Console.WriteLine("Pedido no encontrado.");
    else if (Confirmar($"¿Eliminar el pedido #{idPedido}?"))
    {
        pedidos.Remove(pedido);
        itemsPorPedido.Remove(idPedido);
        Console.WriteLine("Pedido eliminado.");
    }
    Pausa();
}

void ListarPedidos()
{
    Titulo("--- LISTA DE PEDIDOS ---");
    MostrarPedidos(pedidos);
    Pausa();
}

void MarcarEntregado()
{
    Titulo("--- MARCAR PEDIDO COMO ENTREGADO ---");
    int idPedido = LeerEntero("ID del pedido: ");
    Dictionary<string, string>? pedido = BuscarPorId(pedidos, idPedido);

    if (pedido is null)
        Console.WriteLine("Pedido no encontrado.");
    else if (pedido["ESTADO"] == Entregado)
        Console.WriteLine("Ese pedido ya estaba marcado como entregado.");
    else if (Confirmar($"¿Marcar el pedido #{idPedido} como entregado?"))
    {
        pedido["ESTADO"] = Entregado;
        Console.WriteLine("Pedido marcado como entregado.");
    }
    Pausa();
}

void MostrarPedidos(List<Dictionary<string, string>> lista)
{
    if (lista.Count == 0)
    {
        Console.WriteLine("No hay pedidos para mostrar.");
        return;
    }

    foreach (var pedido in lista)
    {
        Dictionary<int, int> items = itemsPorPedido[int.Parse(pedido["ID"])];

        Console.WriteLine($"\nPedido #{pedido["ID"]} | Cliente: {NombreCliente(pedido["CLIENTE_ID"])} | {pedido["FECHA"]} | {pedido["ESTADO"]}");
        foreach (var (idProducto, cantidad) in items)
        {
            string nombre = BuscarPorId(productos, idProducto)?["NOMBRE"] ?? "(producto eliminado)";
            decimal subtotal = PrecioProducto(idProducto) * cantidad;
            Console.WriteLine($"   [{idProducto}] {nombre} x{cantidad} = {subtotal:N2}");
        }
        Console.WriteLine($"   TOTAL: {TotalPedido(items):N2}");
    }
}

decimal TotalPedido(Dictionary<int, int> items) =>
    items.Sum(item => PrecioProducto(item.Key) * item.Value);

// ===========================================================================
// BÚSQUEDA GENÉRICA
// ===========================================================================
Dictionary<string, string>? BuscarPorId(List<Dictionary<string, string>> lista, int id) =>
    lista.Find(registro => registro["ID"] == id.ToString());

// ===========================================================================
// ENTRADA DE DATOS CON VALIDACIÓN
// ===========================================================================
int LeerEntero(string mensaje, int min = int.MinValue, int max = int.MaxValue)
{
    while (true)
    {
        Console.Write(mensaje);
        if (int.TryParse(Console.ReadLine(), out int valor) && valor >= min && valor <= max)
            return valor;
        Console.WriteLine("Entrada no válida. Intente de nuevo.");
    }
}

decimal LeerDecimal(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);
        if (decimal.TryParse(Console.ReadLine(), out decimal valor) && valor > 0)
            return valor;
        Console.WriteLine("Ingrese un número mayor que cero.");
    }
}

string LeerTexto(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);
        string? texto = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(texto))
            return texto.Trim();
        Console.WriteLine("Este campo no puede estar vacío.");
    }
}

string LeerTextoOpcional(string mensaje, string valorActual)
{
    Console.Write(mensaje);
    string? texto = Console.ReadLine();
    return string.IsNullOrWhiteSpace(texto) ? valorActual : texto.Trim();
}

bool Confirmar(string mensaje)
{
    while (true)
    {
        Console.Write($"{mensaje} (S/N): ");
        string respuesta = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
        if (respuesta is "S" or "SI" or "SÍ") return true;
        if (respuesta is "N" or "NO") return false;
        Console.WriteLine("Responda S o N.");
    }
}

void Pausa()
{
    Console.Write("\nPresione Enter para continuar...");
    Console.ReadLine();
}