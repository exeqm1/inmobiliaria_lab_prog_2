using Microsoft.AspNetCore.Http;

namespace Inmobiliaria.Servicios
{
    public interface IImagenServicio
    {
        // Valida que el archivo sea una imagen con extensión y tamaño permitidos.
        bool EsImagenValida(IFormFile archivo, out string? error);

        // Guarda el archivo en wwwroot/uploads/{subcarpeta} y devuelve la ruta relativa (ej: /uploads/inmuebles/abc.jpg).
        Task<string?> GuardarAsync(IFormFile? archivo, string subcarpeta);

        // Elimina físicamente un archivo a partir de su ruta relativa.
        void Eliminar(string? rutaRelativa);
    }

    public class ImagenServicio : IImagenServicio
    {
        private const long TamanioMaximo = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly IWebHostEnvironment _entorno;

        public ImagenServicio(IWebHostEnvironment entorno)
        {
            _entorno = entorno;
        }

        public bool EsImagenValida(IFormFile archivo, out string? error)
        {
            error = null;

            if (archivo == null || archivo.Length == 0)
            {
                error = "El archivo está vacío.";
                return false;
            }

            if (archivo.Length > TamanioMaximo)
            {
                error = "El archivo supera el tamaño máximo permitido (5 MB).";
                return false;
            }

            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                error = "Formato no permitido. Use JPG, JPEG, PNG o WEBP.";
                return false;
            }

            return true;
        }

        public async Task<string?> GuardarAsync(IFormFile? archivo, string subcarpeta)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return null;
            }

            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            string carpetaFisica = Path.Combine(_entorno.WebRootPath, "uploads", subcarpeta);
            Directory.CreateDirectory(carpetaFisica);

            string nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            string rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/uploads/{subcarpeta}/{nombreArchivo}";
        }

        public void Eliminar(string? rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return;
            }

            string relativa = rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string rutaFisica = Path.Combine(_entorno.WebRootPath, relativa);

            if (File.Exists(rutaFisica))
            {
                File.Delete(rutaFisica);
            }
        }
    }
}
