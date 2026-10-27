using System.ComponentModel.DataAnnotations;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Http;

namespace Inmobiliaria.ViewModels
{
    public class InmuebleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un tipo de inmueble.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de inmueble.")]
        [Display(Name = "Tipo de Inmueble")]
        public int TipoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un propietario.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un propietario.")]
        [Display(Name = "Propietario")]
        public int PropietarioId { get; set; }

        [Required(ErrorMessage = "El cupo es obligatorio.")]
        [Range(1, 100, ErrorMessage = "El cupo debe estar entre 1 y 100.")]
        [Display(Name = "Cupo")]
        public int Cupo { get; set; } = 1;

        [Required(ErrorMessage = "El precio por día es obligatorio.")]
        [Range(0.01, 100000000, ErrorMessage = "Ingrese un precio válido.")]
        [Display(Name = "Precio por Día")]
        [DataType(DataType.Currency)]
        public decimal PrecioDia { get; set; }

        [Required(ErrorMessage = "El porcentaje de reserva es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
        [Display(Name = "Porcentaje de Reserva (%)")]
        public int PorcentajeReserva { get; set; } = 30;

        [Display(Name = "Coordenadas")]
        public string? Coordenadas { get; set; }

        [Display(Name = "¿Disponible?")]
        public bool Disponible { get; set; } = true;

        [Display(Name = "Portada actual")]
        public string? PortadaActual { get; set; }

        [Display(Name = "Portada")]
        public IFormFile? PortadaArchivo { get; set; }

        [Display(Name = "Fotos adicionales")]
        public List<IFormFile>? FotosArchivos { get; set; }

        public List<FotoInmueble> FotosExistentes { get; set; } = new();

        public List<int> FotosAEliminar { get; set; } = new();
    }
}
