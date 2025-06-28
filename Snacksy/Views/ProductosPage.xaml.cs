using Snacksy.Models;
using Snacksy.Services;

namespace Snacksy.Views;

public partial class ProductosPage : ContentPage
{
    private readonly ProductoService _productoService;

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
        string nombre = await DisplayPromptAsync("Nuevo Producto", "Nombre del producto:");
        if (string.IsNullOrWhiteSpace(nombre))
            return;

        string precio = await DisplayPromptAsync("Nuevo Producto", "Precio:");
        if (string.IsNullOrWhiteSpace(precio))
            return;

        string stock = await DisplayPromptAsync("Nuevo Producto", "Stock:");
        if (string.IsNullOrWhiteSpace(stock))
            return;

        var producto = new Producto
        {
            Name = nombre,
            Price = precio,
            Stock = stock
        };

        bool exito = await _productoService.AgregarProducto(producto);

        if (exito)
        {
            await DisplayAlert("Éxito", "Producto agregado correctamente", "OK");
            CargarProductos();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo agregar el producto", "OK");
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var producto = button?.BindingContext as Producto;
        if (producto == null) return;

        string nuevoNombre = await DisplayPromptAsync("Editar Producto", "Nuevo nombre:", initialValue: producto.Name);
        if (string.IsNullOrWhiteSpace(nuevoNombre)) return;

        string nuevoPrecio = await DisplayPromptAsync("Editar Producto", "Nuevo precio:", initialValue: producto.Price);
        if (string.IsNullOrWhiteSpace(nuevoPrecio)) return;

        string nuevoStock = await DisplayPromptAsync("Editar Producto", "Nuevo stock:", initialValue: producto.Stock);
        if (string.IsNullOrWhiteSpace(nuevoStock)) return;

        producto.Name = nuevoNombre;
        producto.Price = nuevoPrecio;
        producto.Stock = nuevoStock;

        bool exito = await _productoService.ActualizarProducto(producto);

        if (exito)
        {
            await DisplayAlert("Éxito", "Producto actualizado correctamente", "OK");
            CargarProductos();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo actualizar el producto", "OK");
        }
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
}
