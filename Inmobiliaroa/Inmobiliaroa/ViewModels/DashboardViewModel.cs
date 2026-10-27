using Inmobiliaria.Models;

namespace Inmobiliaria.ViewModels
{
    public class DashboardViewModel
    {
        public int PropietariosActivos { get; set; }

        public int InquilinosActivos { get; set; }

        public int InmueblesActivos { get; set; }

        public int InmueblesDisponibles { get; set; }

        public int ReservasVigentes { get; set; }

        public int ReservasFinalizadas { get; set; }

        public int ReservasCanceladas { get; set; }

        public int PagosRegistrados { get; set; }

        public decimal IngresosTotales { get; set; }

        public List<Reserva> ProximasReservas { get; set; } = new();

        public List<IngresoPorTipoItem> IngresosPorTipo { get; set; } = new();
    }
}
