using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria.Models
{
    public class Reserva
    {
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El inmueble es obligatorio")]
        [Display(Name = "Inmueble")]
        public int InmuebleId { get; set; }

        [ForeignKey(nameof(InmuebleId))]
        public Inmueble? Inmueble { get; set; }

        [Required(ErrorMessage = "El inquilino es obligatorio")]
        [Display(Name = "Inquilino")]
        public int InquilinoId { get; set; }

        [ForeignKey(nameof(InquilinoId))]
        public Inquilino? Inquilino { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El monto diario es obligatorio")]
        [DataType(DataType.Currency)]
        [Display(Name = "Monto Diario")]
        public decimal MontoDiario { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin Efectiva")]
        public DateTime? FechaFinEfectiva { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Multa")]
        public decimal? Multa { get; set; }

        [Required(ErrorMessage = "El usuario creador es obligatorio")]
        [Display(Name = "Creado por")]
        public int UsuarioCreadorId { get; set; }

        [ForeignKey(nameof(UsuarioCreadorId))]
        public Usuario? UsuarioCreador { get; set; }

        [Display(Name = "Finalizado por")]
        public int? UsuarioFinalizadorId { get; set; }

        [ForeignKey(nameof(UsuarioFinalizadorId))]
        public Usuario? UsuarioFinalizador { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;
    }
}

