using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria.Models
{
    public class Pago
    {
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Required(ErrorMessage = "La reserva es obligatoria")]
        [Display(Name = "Reserva")]
        public int ReservaId { get; set; }

        [ForeignKey(nameof(ReservaId))]
        public Reserva? Reserva { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El importe es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }

        [Required(ErrorMessage = "El usuario que registra el pago es obligatorio")]
        [Display(Name = "Registrado por")]
        public int UsuarioCreadorId { get; set; }

        [ForeignKey(nameof(UsuarioCreadorId))]
        public Usuario? UsuarioCreador { get; set; }

        [Display(Name = "Anulado por")]
        public int? UsuarioAnuladorId { get; set; }

        [ForeignKey(nameof(UsuarioAnuladorId))]
        public Usuario? UsuarioAnulador { get; set; }

        [Display(Name = "¿Anulado?")]
        public bool Anulado { get; set; } = false;
    }
}

