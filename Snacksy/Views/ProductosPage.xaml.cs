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

        bool confirm = await DisplayAlert("Eliminar", $"�Deseas eliminar '{producto.Name}'?", "S�", "No");
        if (!confirm) return;

        bool exito = await _productoService.EliminarProducto(producto);

        if (exito)
        {
            await DisplayAlert("�xito", "Producto eliminado", "OK");
            CargarProductos();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo eliminar el producto", "OK");
        }
    }
}
