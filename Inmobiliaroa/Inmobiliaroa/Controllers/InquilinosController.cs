using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino _repo;

        public InquilinosController(IRepositorioInquilino repo)
        {
            _repo = repo;
        }

        // GET: Inquilinos
        public IActionResult Index(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            var resultado = _repo.ObtenerPaginado(pagina, tamanio, busqueda);
            return View(resultado);
        }

        // GET: Inquilinos/Details/5
        public IActionResult Details(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null)
            {
                TempData["Mensaje"] = "El inquilino solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View(new Inquilino());
        }

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inquilino inquilino)
        {
            if (!ModelState.IsValid)
            {
                return View(inquilino);
            }

            // Validar unicidad de DNI
            var existenteDni = _repo.ObtenerPorDni(inquilino.Dni);
            if (existenteDni != null)
            {
                ModelState.AddModelError(nameof(Inquilino.Dni), "Ya existe un inquilino registrado con ese DNI.");
                return View(inquilino);
            }

            // Validar unicidad de Email
            var existenteEmail = _repo.ObtenerPorEmail(inquilino.Email);
            if (existenteEmail != null)
            {
                ModelState.AddModelError(nameof(Inquilino.Email), "Ya existe un inquilino registrado con ese correo electrónico.");
                return View(inquilino);
            }

            try
            {
                _repo.Alta(inquilino);
                TempData["Mensaje"] = $"Inquilino {inquilino.NombreCompleto} registrado con éxito.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al guardar el inquilino: " + ex.Message);
                return View(inquilino);
            }
        }

        // GET: Inquilinos/Edit/5
        public IActionResult Edit(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null)
            {
                TempData["Mensaje"] = "El inquilino solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inquilino inquilino)
        {
            if (id != inquilino.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(inquilino);
            }

            // Validar unicidad de DNI excluyendo el actual
            var existenteDni = _repo.ObtenerPorDni(inquilino.Dni);
            if (existenteDni != null && existenteDni.Id != id)
            {
                ModelState.AddModelError(nameof(Inquilino.Dni), "El DNI ingresado ya pertenece a otro inquilino.");
                return View(inquilino);
            }

            // Validar unicidad de Email excluyendo el actual
            var existenteEmail = _repo.ObtenerPorEmail(inquilino.Email);
            if (existenteEmail != null && existenteEmail.Id != id)
            {
                ModelState.AddModelError(nameof(Inquilino.Email), "El correo electrónico ya pertenece a otro inquilino.");
                return View(inquilino);
            }

            try
            {
                _repo.Modificacion(inquilino);
                TempData["Mensaje"] = "Datos del inquilino actualizados correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al modificar: " + ex.Message);
                return View(inquilino);
            }
        }

        // GET: Inquilinos/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var inquilino = _repo.ObtenerPorId(id);
            if (inquilino == null)
            {
                TempData["Mensaje"] = "El inquilino no fue encontrado.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repo.BajaLogica(id);
                TempData["Mensaje"] = "El inquilino ha sido dado de baja lógicamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al dar de baja al inquilino: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Inquilinos/Buscar?q=texto
        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<OpcionSelect>());
            }

            var lista = _repo.Buscar(q)
                .Select(i => new OpcionSelect { Id = i.Id, Text = $"{i.NombreCompleto} (DNI: {i.Dni})" });

            return Json(lista);
        }
    }
}

