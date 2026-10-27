using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.ViewModels
{
    public class RescindirViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de fin efectiva es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin Efectiva")]
        public DateTime FechaFinEfectiva { get; set; }

        // Datos de solo lectura para contexto.
        public string? InmuebleDescripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoDiario { get; set; }
        public decimal MultaEstimada { get; set; }
    }
}
