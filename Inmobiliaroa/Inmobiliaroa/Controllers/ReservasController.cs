using System.Security.Claims;
using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva _repo;
        private readonly IRepositorioInmueble _repoInmuebles;
        private readonly IRepositorioInquilino _repoInquilinos;

        public ReservasController(
            IRepositorioReserva repo,
            IRepositorioInmueble repoInmuebles,
            IRepositorioInquilino repoInquilinos)
        {
            _repo = repo;
            _repoInmuebles = repoInmuebles;
            _repoInquilinos = repoInquilinos;
        }

        // GET: Reservas
        public IActionResult Index(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            var resultado = _repo.ObtenerPaginado(pagina, tamanio, busqueda);
            return View(resultado);
        }

        // GET: Reservas/Details/5
        public IActionResult Details(int id)
        {
            var reserva = _repo.ObtenerPorId(id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            CargarCombos();
            return View(new ReservaFormViewModel());
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ReservaFormViewModel model)
        {
            ValidarFechas(model);

            var inmueble = model.InmuebleId > 0 ? _repoInmuebles.ObtenerPorId(model.InmuebleId) : null;
            if (model.InmuebleId > 0 && inmueble == null)
            {
                ModelState.AddModelError(nameof(model.InmuebleId), "El inmueble seleccionado no existe.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(model);
                return View(model);
            }

            if (_repo.ExisteSuperposicion(model.InmuebleId, model.FechaInicio, model.FechaFin))
            {
                ModelState.AddModelError(string.Empty, "El inmueble ya tiene una reserva activa que se superpone con las fechas indicadas.");
                CargarCombos(model);
                return View(model);
            }

            var reserva = new Reserva
            {
                InmuebleId = model.InmuebleId,
                InquilinoId = model.InquilinoId,
                FechaInicio = model.FechaInicio.Date,
                FechaFin = model.FechaFin.Date,
                MontoDiario = inmueble!.PrecioDia,
                UsuarioCreadorId = IdUsuarioActual()
            };

            try
            {
                _repo.Alta(reserva);
                TempData["Mensaje"] = "Reserva registrada con éxito.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al guardar la reserva: " + ex.Message);
                CargarCombos(model);
                return View(model);
            }
        }

        // GET: Reservas/Edit/5
        public IActionResult Edit(int id)
        {
            var reserva = _repo.ObtenerPorId(id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var model = new ReservaFormViewModel
            {
                Id = reserva.Id,
                InmuebleId = reserva.InmuebleId,
                InquilinoId = reserva.InquilinoId,
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin,
                MontoDiario = reserva.MontoDiario,
                InmuebleDescripcion = reserva.Inmueble?.Direccion
            };

            CargarCombos(model);
            return View(model);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ReservaFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ValidarFechas(model);

            if (!ModelState.IsValid)
            {
                CargarCombos(model);
                return View(model);
            }

            var reserva = _repo.ObtenerPorId(id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            if (_repo.ExisteSuperposicion(reserva.InmuebleId, model.FechaInicio, model.FechaFin, id))
            {
                ModelState.AddModelError(string.Empty, "El inmueble ya tiene otra reserva activa que se superpone con las fechas indicadas.");
                model.InmuebleDescripcion = reserva.Inmueble?.Direccion;
                CargarCombos(model);
                return View(model);
            }

            reserva.InquilinoId = model.InquilinoId;
            reserva.FechaInicio = model.FechaInicio.Date;
            reserva.FechaFin = model.FechaFin.Date;

            try
            {
                _repo.Modificacion(reserva);
                TempData["Mensaje"] = "Reserva actualizada correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al modificar la reserva: " + ex.Message);
                model.InmuebleDescripcion = reserva.Inmueble?.Direccion;
                CargarCombos(model);
                return View(model);
            }
        }

        // GET: Reservas/Delete/5
        public IActionResult Delete(int id)
        {
            var reserva = _repo.ObtenerPorId(id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva no fue encontrada.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repo.BajaLogica(id);
                TempData["Mensaje"] = "La reserva ha sido dada de baja lógicamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al dar de baja la reserva: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Reservas/Rescindir/5
        public IActionResult Rescindir(int id)
        {
            var reserva = _repo.ObtenerPorId(id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            if (reserva.FechaFinEfectiva.HasValue)
            {
                TempData["Mensaje"] = "La reserva ya fue rescindida anticipadamente.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Details), new { id });
            }

            var model = new RescindirViewModel
            {
                Id = reserva.Id,
                InmuebleDescripcion = reserva.Inmueble?.Direccion,
                FechaInicio = reserva.FechaInicio,
                FechaFin = reserva.FechaFin,
                MontoDiario = reserva.MontoDiario,
                FechaFinEfectiva = FechaRescisionPorDefecto(reserva)
            };
            model.MultaEstimada = CalcularMulta(reserva, model.FechaFinEfectiva);

            return View(model);
        }

        // POST: Reservas/Rescindir/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Rescindir(int id, RescindirViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var reserva = _repo.ObtenerPorId(id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            if (reserva.FechaFinEfectiva.HasValue)
            {
                TempData["Mensaje"] = "La reserva ya fue rescindida anticipadamente.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (model.FechaFinEfectiva.Date <= reserva.FechaInicio.Date || model.FechaFinEfectiva.Date >= reserva.FechaFin.Date)
            {
                ModelState.AddModelError(nameof(model.FechaFinEfectiva), "La fecha de fin efectiva debe ser posterior al inicio y anterior al fin original.");
            }

            model.InmuebleDescripcion = reserva.Inmueble?.Direccion;
            model.FechaInicio = reserva.FechaInicio;
            model.FechaFin = reserva.FechaFin;
            model.MontoDiario = reserva.MontoDiario;
            model.MultaEstimada = CalcularMulta(reserva, model.FechaFinEfectiva);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _repo.Rescindir(reserva.Id, model.FechaFinEfectiva, model.MultaEstimada, IdUsuarioActual());
                TempData["Mensaje"] = $"Reserva rescindida. Multa aplicada: ${model.MultaEstimada.ToString("N2")}.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Details), new { id = reserva.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al rescindir la reserva: " + ex.Message);
                return View(model);
            }
        }

        // GET: Reservas/Renovar/5
        public IActionResult Renovar(int id)
        {
            var reserva = _repo.ObtenerPorId(id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            if (reserva.FechaFinEfectiva.HasValue)
            {
                TempData["Mensaje"] = "No se puede renovar una reserva que ya fue rescindida.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(new RenovarReservaViewModel
            {
                Id = reserva.Id,
                InmuebleDescripcion = reserva.Inmueble?.Direccion,
                InquilinoNombre = reserva.Inquilino?.NombreCompleto,
                FechaInicio = reserva.FechaInicio,
                FechaFinActual = reserva.FechaFin,
                MontoDiario = reserva.MontoDiario,
                NuevaFechaFin = reserva.FechaFin.AddDays(7)
            });
        }

        // POST: Reservas/Renovar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Renovar(int id, RenovarReservaViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var reserva = _repo.ObtenerPorId(id);
            if (reserva == null)
            {
                TempData["Mensaje"] = "La reserva solicitada no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            if (reserva.FechaFinEfectiva.HasValue)
            {
                TempData["Mensaje"] = "No se puede renovar una reserva que ya fue rescindida.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Details), new { id });
            }

            model.InmuebleDescripcion = reserva.Inmueble?.Direccion;
            model.InquilinoNombre = reserva.Inquilino?.NombreCompleto;
            model.FechaInicio = reserva.FechaInicio;
            model.FechaFinActual = reserva.FechaFin;
            model.MontoDiario = reserva.MontoDiario;

            if (model.NuevaFechaFin.Date <= reserva.FechaFin.Date)
            {
                ModelState.AddModelError(nameof(model.NuevaFechaFin), "La nueva fecha de fin debe ser posterior a la fecha de fin actual.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_repo.ExisteSuperposicion(reserva.InmuebleId, reserva.FechaInicio, model.NuevaFechaFin, reserva.Id))
            {
                ModelState.AddModelError(string.Empty, "El inmueble ya tiene otra reserva activa que se superpone con el nuevo rango de fechas.");
                return View(model);
            }

            try
            {
                _repo.Renovar(reserva.Id, model.NuevaFechaFin);
                TempData["Mensaje"] = $"Reserva renovada hasta el {model.NuevaFechaFin:dd/MM/yyyy}.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Details), new { id = reserva.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al renovar la reserva: " + ex.Message);
                return View(model);
            }
        }

        private static DateTime FechaRescisionPorDefecto(Reserva reserva)
        {
            DateTime hoy = DateTime.Today;
            if (hoy > reserva.FechaInicio && hoy < reserva.FechaFin)
            {
                return hoy;
            }
            return reserva.FechaInicio.AddDays(1);
        }

        private static decimal CalcularMulta(Reserva reserva, DateTime fechaFinEfectiva)
        {
            int diasNoUsados = (reserva.FechaFin.Date - fechaFinEfectiva.Date).Days;
            if (diasNoUsados <= 0)
            {
                return 0m;
            }
            return Math.Round(0.5m * diasNoUsados * reserva.MontoDiario, 2);
        }

        private int IdUsuarioActual()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        private void ValidarFechas(ReservaFormViewModel model)
        {
            if (model.FechaFin <= model.FechaInicio)
            {
                ModelState.AddModelError(nameof(model.FechaFin), "La fecha de fin debe ser posterior a la fecha de inicio.");
            }
        }

        private void CargarCombos(ReservaFormViewModel? model = null)
        {
            ViewBag.Inmuebles = new SelectList(_repoInmuebles.ObtenerTodos(), "Id", "Direccion", model?.InmuebleId);
            ViewBag.Inquilinos = new SelectList(_repoInquilinos.ObtenerTodos(), "Id", "NombreCompleto", model?.InquilinoId);
        }
    }
}
