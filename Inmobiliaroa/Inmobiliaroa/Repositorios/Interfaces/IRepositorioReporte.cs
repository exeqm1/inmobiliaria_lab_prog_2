using Inmobiliaria.ViewModels;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioReporte
    {
        DashboardViewModel ObtenerDashboard();

        ReporteReservasViewModel ObtenerReporteReservas(DateTime desde, DateTime hasta);

        List<InformeInmuebleItem> ObtenerInformeInmuebles(bool? disponible, int? propietarioId);

        List<InmuebleReservasItem> ObtenerMasReservados(int dias);

        List<InformeInmuebleItem> ObtenerInmueblesSinReservas(int dias);

        List<ReporteReservaItem> ObtenerReservasVigentes(DateTime fecha);

        List<ReporteReservaItem> ObtenerReservasPorTerminar(DateTime fecha, int dias);

        List<InformeInmuebleItem> ObtenerInmueblesLibres(DateTime desde, DateTime hasta);
    }
}
