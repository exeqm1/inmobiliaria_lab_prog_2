using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.ViewModels
{
    public class ReservaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un inmueble.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un inmueble.")]
        [Display(Name = "Inmueble")]
        public int InmuebleId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un inquilino.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un inquilino.")]
        [Display(Name = "Inquilino")]
        public int InquilinoId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; } = DateTime.Today.AddDays(1);

        // Solo para mostrar en la edición (el monto diario queda congelado al crear la reserva).
        public decimal MontoDiario { get; set; }

        public string? InmuebleDescripcion { get; set; }
    }
}
