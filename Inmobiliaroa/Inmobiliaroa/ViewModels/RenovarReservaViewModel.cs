using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.ViewModels
{
    public class RenovarReservaViewModel
    {
        public int Id { get; set; }

        public string? InmuebleDescripcion { get; set; }

        public string? InquilinoNombre { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFinActual { get; set; }

        public decimal MontoDiario { get; set; }

        [Required(ErrorMessage = "La nueva fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Nueva Fecha de Inicio")]
        public DateTime NuevaFechaInicio { get; set; }

        [Required(ErrorMessage = "La nueva fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Nueva Fecha de Fin")]
        public DateTime NuevaFechaFin { get; set; }

        [Required(ErrorMessage = "El nuevo monto diario es obligatorio.")]
        [Range(0.01, 100000000, ErrorMessage = "El monto diario debe ser mayor a 0.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Nuevo Monto Diario")]
        public decimal NuevoMontoDiario { get; set; }

        public int DiasActuales => (FechaFinActual - FechaInicio).Days;

        public int DiasNuevos => (NuevaFechaFin.Date - NuevaFechaInicio.Date).Days;

        public decimal TotalNuevo => Math.Round(DiasNuevos * NuevoMontoDiario, 2);
    }
}
