using Snacksy.Models;
using Supabase;

namespace Snacksy.Services
{
    internal class ProductoService
    {
        private readonly Supabase.Client _client;
        private bool _initialized = false;

        public ProductoService()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            _client = new Supabase.Client(
                "https://rfmgnhzothlrdytdcvvd.supabase.co",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InJmbWduaHpvdGhscmR5dGRjdnZkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTEwODI5NjgsImV4cCI6MjA2NjY1ODk2OH0.oEVMUE-QKvuXoN_RvQdaqbl_tHNzacNaXT0iE8tku2g",
                options);
        }

        public async Task InitializeAsync()
        {
            if (!_initialized)
            {
                await _client.InitializeAsync();
                _initialized = true;
            }
        }

        public async Task<List<Producto>> ObtenerProducto()
        {
            await InitializeAsync();
            var response = await _client.From<Producto>().Get();
            return response.Models;
        }

        public async Task<bool> AgregarProducto(Producto producto)
        {
            await InitializeAsync();

            if (string.IsNullOrWhiteSpace(producto.Name) || string.IsNullOrWhiteSpace(producto.Stock))
                return false;

            var response = await _client.From<Producto>().Insert(producto);
            return response.Models.Any();
        }

        public async Task<bool> ActualizarProducto(Producto producto)
        {
            await InitializeAsync();

            if (producto == null || producto.Id <= 0)
                return false;

            var response = await _client.From<Producto>().Update(producto);
            return response.ResponseMessage.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarProducto(Producto producto)
        {
            await InitializeAsync();

            if (producto == null || producto.Id <= 0)
                return false;

            var response = await _client.From<Producto>().Delete(producto);
            return response.ResponseMessage.IsSuccessStatusCode;
        }
    }
}
