using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IRepositorioReporte _repo;
        private readonly IRepositorioPropietario _repoPropietarios;

        public ReportesController(IRepositorioReporte repo, IRepositorioPropietario repoPropietarios)
        {
            _repo = repo;
            _repoPropietarios = repoPropietarios;
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

        // GET: Reportes/Inmuebles?disponible=true&propietarioId=3
        public IActionResult Inmuebles(bool? disponible, int? propietarioId)
        {
            return View(new InformeInmueblesViewModel
            {
                Inmuebles = _repo.ObtenerInformeInmuebles(disponible, propietarioId),
                Propietarios = _repoPropietarios.ObtenerTodos(),
                Disponible = disponible,
                PropietarioId = propietarioId
            });
        }

        // GET: Reportes/MasReservados?dias=365
        public IActionResult MasReservados(int dias = 365)
        {
            dias = Math.Clamp(dias, 1, 3650);
            return View(new InformeMasReservadosViewModel
            {
                Inmuebles = _repo.ObtenerMasReservados(dias),
                Dias = dias
            });
        }

        // GET: Reportes/SinReservas?dias=30
        public IActionResult SinReservas(int dias = 30)
        {
            dias = Math.Clamp(dias, 1, 3650);
            return View(new InformeSinReservasViewModel
            {
                Inmuebles = _repo.ObtenerInmueblesSinReservas(dias),
                Dias = dias
            });
        }

        // GET: Reportes/Vigentes?fecha=2026-10-29
        public IActionResult Vigentes(DateTime? fecha)
        {
            var referencia = (fecha ?? DateTime.Today).Date;
            return View(new InformeReservasViewModel
            {
                Reservas = _repo.ObtenerReservasVigentes(referencia),
                Fecha = referencia
            });
        }

        // GET: Reportes/PorTerminar?dias=30
        public IActionResult PorTerminar(int dias = 30, DateTime? fecha = null)
        {
            dias = Math.Clamp(dias, 1, 365);
            var referencia = (fecha ?? DateTime.Today).Date;
            return View(new InformeReservasViewModel
            {
                Reservas = _repo.ObtenerReservasPorTerminar(referencia, dias),
                Fecha = referencia,
                Dias = dias
            });
        }

        // GET: Reportes/Libres?desde=2026-11-01&hasta=2026-11-15
        public IActionResult Libres(DateTime? desde, DateTime? hasta)
        {
            var fechaDesde = (desde ?? DateTime.Today).Date;
            var fechaHasta = (hasta ?? DateTime.Today.AddDays(30)).Date;
            if (fechaDesde > fechaHasta)
            {
                (fechaDesde, fechaHasta) = (fechaHasta, fechaDesde);
            }

            return View(new InformeLibresViewModel
            {
                Inmuebles = _repo.ObtenerInmueblesLibres(fechaDesde, fechaHasta),
                Desde = fechaDesde,
                Hasta = fechaHasta
            });
        }
    }
}
