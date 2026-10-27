using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria.Models
{
    public class FotoInmueble
    {
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Inmueble")]
        public int InmuebleId { get; set; }

        [ForeignKey(nameof(InmuebleId))]
        public Inmueble? Inmueble { get; set; }

        [Required]
        [Display(Name = "Ruta de la Foto")]
        public string UrlFoto { get; set; } = string.Empty;
    }
}

