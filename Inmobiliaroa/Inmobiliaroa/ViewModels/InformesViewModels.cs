using Inmobiliaria.Models;

namespace Inmobiliaria.ViewModels
{
    public class InformeInmuebleItem
    {
        public int Id { get; set; }

        public string Direccion { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Propietario { get; set; } = string.Empty;

        public int Cupo { get; set; }

        public decimal PrecioDia { get; set; }

        public bool Disponible { get; set; }
    }

    public class InformeInmueblesViewModel
    {
        public List<InformeInmuebleItem> Inmuebles { get; set; } = new();

        public List<Propietario> Propietarios { get; set; } = new();

        public bool? Disponible { get; set; }

        public int? PropietarioId { get; set; }
    }

    public class InmuebleReservasItem
    {
        public int Id { get; set; }

        public string Direccion { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Propietario { get; set; } = string.Empty;

        public int CantidadReservas { get; set; }

        public DateTime? UltimaReserva { get; set; }
    }

    public class InformeMasReservadosViewModel
    {
        public List<InmuebleReservasItem> Inmuebles { get; set; } = new();

        public int Dias { get; set; } = 365;
    }

    public class InformeSinReservasViewModel
    {
        public List<InformeInmuebleItem> Inmuebles { get; set; } = new();

        public int Dias { get; set; } = 30;
    }

    public class InformeReservasViewModel
    {
        public List<ReporteReservaItem> Reservas { get; set; } = new();

        public DateTime Fecha { get; set; } = DateTime.Today;

        public int Dias { get; set; } = 30;
    }

    public class InformeLibresViewModel
    {
        public List<InformeInmuebleItem> Inmuebles { get; set; } = new();

        public DateTime Desde { get; set; }

        public DateTime Hasta { get; set; }
    }
}
