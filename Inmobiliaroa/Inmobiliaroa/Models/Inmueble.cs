using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inmobiliaria.Models
{
    public class Inmueble
    {
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cupo es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El cupo debe ser mayor a 0")]
        [Display(Name = "Cupo")]
        public int Cupo { get; set; }

        [Required(ErrorMessage = "El tipo de inmueble es obligatorio")]
        [Display(Name = "Tipo de Inmueble")]
        public int TipoId { get; set; }

        [ForeignKey(nameof(TipoId))]
        public TipoInmueble? Tipo { get; set; }

        [Display(Name = "Coordenadas")]
        public string? Coordenadas { get; set; }

        [Required(ErrorMessage = "El precio por día es obligatorio")]
        [Display(Name = "Precio por Día")]
        [DataType(DataType.Currency)]
        public decimal PrecioDia { get; set; }

        [Required(ErrorMessage = "El porcentaje de reserva es obligatorio")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        [Display(Name = "Porcentaje de Reserva (%)")]
        public int PorcentajeReserva { get; set; }

        [Display(Name = "Portada")]
        public string? Portada { get; set; }

        [Display(Name = "¿Disponible?")]
        public bool Disponible { get; set; } = true;

        [Required(ErrorMessage = "El propietario es obligatorio")]
        [Display(Name = "Propietario")]
        public int PropietarioId { get; set; }

        [ForeignKey(nameof(PropietarioId))]
        public Propietario? Propietario { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        public List<FotoInmueble> Fotos { get; set; } = new();
    }
}

