using Snacksy.Models;
using Snacksy.Services;
using System.Text;
using System.Text.Json; // para JsonSerializer


namespace Snacksy.Views;

public partial class ProductosPage : ContentPage
{
    private List<ItemCarrito> carrito = new();
    private readonly ProductoService _productoService;

    private class ItemCarrito
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
    }

    public ProductosPage()
    {
        InitializeComponent();
        _productoService = new ProductoService();
        CargarProductos();
    }

    private async void CargarProductos()
    {
        var productos = await _productoService.ObtenerProducto();
        productosCollection.ItemsSource = productos;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var formulario = new ProductosFormPage();
        formulario.ProductoGuardado += (_, _) => CargarProductos();
        await Navigation.PushModalAsync(formulario);
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var producto = button?.BindingContext as Producto;
        if (producto == null) return;

        var formulario = new ProductosFormPage(producto);
        formulario.ProductoGuardado += (_, _) => CargarProductos();
        await Navigation.PushModalAsync(formulario);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var producto = button?.BindingContext as Producto;
        if (producto == null) return;

        bool confirm = await DisplayAlert("Eliminar", $"¿Deseas eliminar '{producto.Name}'?", "Sí", "No");
        if (!confirm) return;

        bool exito = await _productoService.EliminarProducto(producto);

        if (exito)
        {
            await DisplayAlert("Éxito", "Producto eliminado", "OK");
            CargarProductos();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo eliminar el producto", "OK");
        }
    }
    private bool linternaEncendida = false;

    private async void OnToggleFlashlightClicked(object sender, EventArgs e)
    {
        try
        {
            if (linternaEncendida)
            {
                await Flashlight.Default.TurnOffAsync(); // método correcto
                linternaEncendida = false;
            }
            else
            {
                await Flashlight.Default.TurnOnAsync(); // método correcto
                linternaEncendida = true;
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Error", "La linterna no es compatible con este dispositivo.", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Error", "No tienes permiso para usar la linterna.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
        }
    }
    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var producto = button?.BindingContext as Producto;

        if (producto == null) return;

        string cantidadTexto = await DisplayPromptAsync("Agregar al carrito", $"¿Cuántas unidades de '{producto.Name}' deseas agregar?", initialValue: "1", keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(cantidadTexto))
            return;

        if (!int.TryParse(cantidadTexto, out int cantidadSolicitada) || cantidadSolicitada <= 0)
        {
            await DisplayAlert("Entrada inválida", "Ingresa un número entero positivo.", "OK");
            return;
        }

        if (!int.TryParse(producto.Stock, out int stockDisponible))
        {
            await DisplayAlert("Error", "El stock del producto no es válido.", "OK");
            return;
        }

        if (cantidadSolicitada > stockDisponible)
        {
            await DisplayAlert("Stock insuficiente", $"Solo hay {stockDisponible} unidades disponibles.", "OK");
            return;
        }

        // Ver si ya está en el carrito
        var existente = carrito.FirstOrDefault(c => c.Producto.Id == producto.Id);
        if (existente != null)
        {
            existente.Cantidad += cantidadSolicitada;
        }
        else
        {
            carrito.Add(new ItemCarrito
            {
                Producto = producto,
                Cantidad = cantidadSolicitada
            });
        }

        await DisplayAlert("Agregado", $"{producto.Name} × {cantidadSolicitada} agregado al carrito.", "OK");
    }


    private async void OnPagarClicked(object sender, EventArgs e)
    {
        if (carrito.Count == 0)
        {
            await DisplayAlert("Carrito vacío", "No hay productos en el carrito.", "OK");
            return;
        }

        // Construir resumen y total
        StringBuilder resumenBuilder = new();
        double total = 0;

        foreach (var item in carrito)
        {
            if (!double.TryParse(item.Producto.Price, out double precio))
                continue;

            double subtotal = precio * item.Cantidad;
            resumenBuilder.AppendLine($"{item.Producto.Name} × {item.Cantidad} = ${subtotal:F2}");
            total += subtotal;
        }

        string resumen = resumenBuilder.ToString();

        // Mostrar resumen con un DisplayAlert
        bool confirmarPago = await DisplayAlert("Resumen de compra",
            $"{resumen}\nTOTAL: ${total:F2}\n\n¿Deseas proceder al pago?",
            "Sí", "Cancelar");

        if (!confirmarPago)
        {
            // Si cancela, no hacemos nada
            return;
        }

        // Opciones de pago
        string opcion = await DisplayActionSheet("Selecciona método de pago", "Cancelar", null, "Efectivo", "Tarjeta");

        if (opcion == "Efectivo")
        {
            // Descuenta el stock localmente y actualiza en Supabase
            foreach (var item in carrito)
            {
                if (int.TryParse(item.Producto.Stock, out int stockActual))
                {
                    int nuevoStock = stockActual - item.Cantidad;
                    item.Producto.Stock = nuevoStock.ToString();
                    await _productoService.ActualizarProducto(item.Producto);
                }
            }

            carrito.Clear();
            await DisplayAlert("Pago exitoso", "Gracias por tu compra", "OK");
            CargarProductos();
        }
        else if (opcion == "Tarjeta")
        {
            bool exito = await ProcesarPagoStripe(total);
            if (exito)
            {
                await ActualizarStockYLimpiarCarrito();
                await DisplayAlert("Pago exitoso", "Pago con tarjeta completado", "OK");
            }
        }
        else
        {
            // Si cancela, no hacemos nada
            return;
        }
    }


    private async Task<bool> ProcesarPagoStripe(double total)
    {
        try
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using var httpClient = new HttpClient(handler);

            var url = "https://192.168.0.63:7235/api/pago";

            var body = new
            {
                Monto = (int)(total * 100),
                Descripcion = "Compra desde Snacksy",
                Token = "tok_visa"
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);
            var mensaje = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                await DisplayAlert("Error al pagar",
                    $"Código: {(int)response.StatusCode} - {response.ReasonPhrase}\nDetalle: {mensaje}", "OK");
                return false;
            }
        }
        catch (HttpRequestException httpEx)
        {
            await DisplayAlert("Error HTTP", httpEx.Message, "OK");
            return false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error inesperado", ex.ToString(), "OK");
            return false;
        }
    }

    private async Task ActualizarStockYLimpiarCarrito()
    {
        foreach (var item in carrito)
        {
            if (int.TryParse(item.Producto.Stock, out int stockActual))
            {
                int nuevoStock = stockActual - item.Cantidad;
                item.Producto.Stock = nuevoStock.ToString();
                await _productoService.ActualizarProducto(item.Producto);
            }
        }

        carrito.Clear();
        CargarProductos();
    }

}
