using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.ViewModels
{
    public class PagoFormViewModel
    {
        public int ReservaId { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio.")]
        [StringLength(150, ErrorMessage = "El concepto no puede superar los {1} caracteres.")]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = "Seña";

        [Required(ErrorMessage = "El importe es obligatorio.")]
        [Range(0.01, 100000000, ErrorMessage = "El importe debe ser mayor a 0.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        public string? InmuebleDescripcion { get; set; }
    }
}
