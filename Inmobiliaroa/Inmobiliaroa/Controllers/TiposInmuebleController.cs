using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class TiposInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble _repo;

        public TiposInmuebleController(IRepositorioTipoInmueble repo)
        {
            _repo = repo;
        }

        // GET: TiposInmueble
        public IActionResult Index(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            var resultado = _repo.ObtenerPaginado(pagina, tamanio, busqueda);
            return View(resultado);
        }

        // GET: TiposInmueble/Details/5
        public IActionResult Details(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                TempData["Mensaje"] = "El tipo de inmueble solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // GET: TiposInmueble/Create
        public IActionResult Create()
        {
            return View(new TipoInmueble());
        }

        // POST: TiposInmueble/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TipoInmueble tipo)
        {
            if (!ModelState.IsValid)
            {
                return View(tipo);
            }

            try
            {
                _repo.Alta(tipo);
                TempData["Mensaje"] = $"Tipo de inmueble \"{tipo.Descripcion}\" registrado con éxito.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al guardar el tipo de inmueble: " + ex.Message);
                return View(tipo);
            }
        }

        // GET: TiposInmueble/Edit/5
        public IActionResult Edit(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                TempData["Mensaje"] = "El tipo de inmueble solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // POST: TiposInmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TipoInmueble tipo)
        {
            if (id != tipo.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(tipo);
            }

            try
            {
                _repo.Modificacion(tipo);
                TempData["Mensaje"] = "Tipo de inmueble actualizado correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al modificar: " + ex.Message);
                return View(tipo);
            }
        }

        // GET: TiposInmueble/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null)
            {
                TempData["Mensaje"] = "El tipo de inmueble no fue encontrado.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // POST: TiposInmueble/Delete/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repo.BajaLogica(id);
                TempData["Mensaje"] = "El tipo de inmueble ha sido dado de baja lógicamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al dar de baja el tipo de inmueble: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: TiposInmueble/Buscar?q=texto
        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<OpcionSelect>());
            }

            var lista = _repo.Buscar(q)
                .Select(t => new OpcionSelect { Id = t.Id, Text = t.Descripcion });

            return Json(lista);
        }
    }
}
