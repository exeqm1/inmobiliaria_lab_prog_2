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

        [Required(ErrorMessage = "La nueva fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Nueva Fecha de Fin")]
        public DateTime NuevaFechaFin { get; set; }

        public int DiasActuales => (FechaFinActual - FechaInicio).Days;

        public int DiasNuevos => (NuevaFechaFin.Date - FechaInicio.Date).Days;
    }
}
