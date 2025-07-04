using Snacksy.Models;
using Snacksy.Services;

namespace Snacksy.Views;

public partial class ProductosFormPage : ContentPage
{
    private readonly ProductoService _productoService = new();
    private readonly Producto _productoOriginal;
    private readonly bool _esNuevo;

    public event EventHandler ProductoGuardado;

    public ProductosFormPage(Producto producto = null)
    {
        InitializeComponent();

        _esNuevo = producto == null;
        _productoOriginal = producto ?? new Producto();

        nombreEntry.Text = _productoOriginal.Name;
        precioEntry.Text = _productoOriginal.Price;
        stockEntry.Text = _productoOriginal.Stock;
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nombreEntry.Text) ||
            string.IsNullOrWhiteSpace(precioEntry.Text) ||
            string.IsNullOrWhiteSpace(stockEntry.Text))
        {
            await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
            return;
        }

        _productoOriginal.Name = nombreEntry.Text;
        _productoOriginal.Price = precioEntry.Text;
        _productoOriginal.Stock = stockEntry.Text;

        bool exito = _esNuevo
            ? await _productoService.AgregarProducto(_productoOriginal)
            : await _productoService.ActualizarProducto(_productoOriginal);

        if (exito)
        {
            ProductoGuardado?.Invoke(this, EventArgs.Empty);
            await DisplayAlert("Éxito", _esNuevo ? "Producto agregado" : "Producto actualizado", "OK");
            await Navigation.PopModalAsync();
        }
        else
        {
            await DisplayAlert("Error", "No se pudo guardar el producto", "OK");
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
