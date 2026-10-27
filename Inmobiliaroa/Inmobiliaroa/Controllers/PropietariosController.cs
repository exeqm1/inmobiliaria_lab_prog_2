using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PropietariosController : Controller
    {
        private readonly IRepositorioPropietario _repo;

        public PropietariosController(IRepositorioPropietario repo)
        {
            _repo = repo;
        }

        // GET: Propietarios
        public IActionResult Index(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            var resultado = _repo.ObtenerPaginado(pagina, tamanio, busqueda);
            return View(resultado);
        }

        // GET: Propietarios/Details/5
        public IActionResult Details(int id)
        {
            var propietario = _repo.ObtenerPorId(id);
            if (propietario == null)
            {
                TempData["Mensaje"] = "El propietario solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: Propietarios/Create
        public IActionResult Create()
        {
            return View(new Propietario());
        }

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Propietario propietario)
        {
            if (!ModelState.IsValid)
            {
                return View(propietario);
            }

            // Validar unicidad de DNI
            var existenteDni = _repo.ObtenerPorDni(propietario.Dni);
            if (existenteDni != null)
            {
                ModelState.AddModelError(nameof(Propietario.Dni), "Ya existe un propietario registrado con ese DNI.");
                return View(propietario);
            }

            // Validar unicidad de Email
            var existenteEmail = _repo.ObtenerPorEmail(propietario.Email);
            if (existenteEmail != null)
            {
                ModelState.AddModelError(nameof(Propietario.Email), "Ya existe un propietario registrado con ese correo electrónico.");
                return View(propietario);
            }

            try
            {
                _repo.Alta(propietario);
                TempData["Mensaje"] = $"Propietario {propietario.NombreCompleto} registrado con éxito.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al guardar el propietario: " + ex.Message);
                return View(propietario);
            }
        }

        // GET: Propietarios/Edit/5
        public IActionResult Edit(int id)
        {
            var propietario = _repo.ObtenerPorId(id);
            if (propietario == null)
            {
                TempData["Mensaje"] = "El propietario solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Propietario propietario)
        {
            if (id != propietario.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(propietario);
            }

            // Validar unicidad de DNI excluyendo el actual
            var existenteDni = _repo.ObtenerPorDni(propietario.Dni);
            if (existenteDni != null && existenteDni.Id != id)
            {
                ModelState.AddModelError(nameof(Propietario.Dni), "El DNI ingresado ya pertenece a otro propietario.");
                return View(propietario);
            }

            // Validar unicidad de Email excluyendo el actual
            var existenteEmail = _repo.ObtenerPorEmail(propietario.Email);
            if (existenteEmail != null && existenteEmail.Id != id)
            {
                ModelState.AddModelError(nameof(Propietario.Email), "El correo electrónico ya pertenece a otro propietario.");
                return View(propietario);
            }

            try
            {
                _repo.Modificacion(propietario);
                TempData["Mensaje"] = "Datos del propietario actualizados correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al modificar: " + ex.Message);
                return View(propietario);
            }
        }

        // GET: Propietarios/Delete/5
        public IActionResult Delete(int id)
        {
            var propietario = _repo.ObtenerPorId(id);
            if (propietario == null)
            {
                TempData["Mensaje"] = "El propietario no fue encontrado.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repo.BajaLogica(id);
                TempData["Mensaje"] = "El propietario ha sido dado de baja lógicamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al dar de baja al propietario: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Propietarios/Buscar?q=texto
        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<OpcionSelect>());
            }

            var lista = _repo.Buscar(q)
                .Select(p => new OpcionSelect { Id = p.Id, Text = $"{p.NombreCompleto} (DNI: {p.Dni})" });

            return Json(lista);
        }
    }
}

