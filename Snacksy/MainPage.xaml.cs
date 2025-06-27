using Snacksy.Services;

namespace Snacksy
{
    public partial class MainPage : ContentPage
    {
        private ProductoService _servicio = new();

        public MainPage()
        {
            InitializeComponent();
            CargarProductos();
        }

        private async void CargarProductos()
        {
            var productos = await _servicio.ObtenerProductosAsync();
            ProductosView.ItemsSource = productos;
        }
    }

}
