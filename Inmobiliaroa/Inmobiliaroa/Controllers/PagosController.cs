using System.Security.Claims;
using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly IRepositorioPago _repo;
        private readonly IRepositorioReserva _repoReservas;

        public PagosController(IRepositorioPago repo, IRepositorioReserva repoReservas)
        {
            _repo = repo;
            _repoReservas = repoReservas;
        }

        // GET: Pagos?reservaId=5
        public IActionResult Index(int reservaId)
        {
            var reserva = _repoReservas.ObtenerPorId(reservaId);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction("Index", "Reservas");
            }

            var model = new PagoReservaViewModel
            {
                Reserva = reserva,
                Pagos = _repo.ObtenerPorReserva(reservaId),
                TotalPagado = _repo.TotalPagado(reservaId)
            };

            return View(model);
        }

        // GET: Pagos/Create?reservaId=5
        public IActionResult Create(int reservaId)
        {
            var reserva = _repoReservas.ObtenerPorId(reservaId);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction("Index", "Reservas");
            }

            return View(new PagoFormViewModel
            {
                ReservaId = reservaId,
                InmuebleDescripcion = reserva.Inmueble?.Direccion
            });
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PagoFormViewModel model)
        {
            var reserva = _repoReservas.ObtenerPorId(model.ReservaId);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction("Index", "Reservas");
            }

            model.InmuebleDescripcion = reserva.Inmueble?.Direccion;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var pago = new Pago
            {
                ReservaId = model.ReservaId,
                Concepto = model.Concepto,
                Importe = model.Importe,
                FechaPago = DateTime.Now,
                UsuarioCreadorId = IdUsuarioActual()
            };

            try
            {
                _repo.Alta(pago);
                TempData["Mensaje"] = $"Pago registrado por ${pago.Importe.ToString("N2")}.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index), new { reservaId = model.ReservaId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al registrar el pago: " + ex.Message);
                return View(model);
            }
        }

        // POST: Pagos/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Anular(int id, int reservaId)
        {
            try
            {
                _repo.Anular(id, IdUsuarioActual());
                TempData["Mensaje"] = "El pago ha sido anulado.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al anular el pago: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index), new { reservaId });
        }

        private int IdUsuarioActual()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }
    }
}
