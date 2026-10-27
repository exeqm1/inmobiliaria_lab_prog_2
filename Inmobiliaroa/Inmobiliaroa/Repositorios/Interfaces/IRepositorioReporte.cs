using Inmobiliaria.ViewModels;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioReporte
    {
        DashboardViewModel ObtenerDashboard();

        ReporteReservasViewModel ObtenerReporteReservas(DateTime desde, DateTime hasta);
    }
}
