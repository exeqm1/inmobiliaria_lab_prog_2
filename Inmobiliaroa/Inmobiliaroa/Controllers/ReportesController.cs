using Inmobiliaria.Repositorios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IRepositorioReporte _repo;

        public ReportesController(IRepositorioReporte repo)
        {
            _repo = repo;
        }

        // GET: Reportes
        public IActionResult Index()
        {
            return View(_repo.ObtenerDashboard());
        }

        // GET: Reportes/Reservas?desde=2026-01-01&hasta=2026-12-31
        public IActionResult Reservas(DateTime? desde, DateTime? hasta)
        {
            var fechaDesde = desde ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fechaHasta = hasta ?? DateTime.Today;

            if (fechaDesde > fechaHasta)
            {
                (fechaDesde, fechaHasta) = (fechaHasta, fechaDesde);
            }

            return View(_repo.ObtenerReporteReservas(fechaDesde, fechaHasta));
        }
    }
}
