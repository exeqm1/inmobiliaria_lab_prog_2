using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Inmobiliaria.ViewModels
{
    public class CambiarAvatarViewModel
    {
        public int Id { get; set; }

        public string? Avatar { get; set; }

        [Display(Name = "Nueva Imagen de Perfil")]
        public IFormFile? Archivo { get; set; }
    }
}
