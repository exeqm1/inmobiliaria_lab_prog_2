using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.ViewModels
{
    public class PagoEditViewModel
    {
        public int Id { get; set; }

        public int ReservaId { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio.")]
        [StringLength(150, ErrorMessage = "El concepto no puede superar los {1} caracteres.")]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; }

        public string? InmuebleDescripcion { get; set; }
    }
}
