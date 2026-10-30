using System.Security.Claims;
using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _repo;

        public UsuariosController(IRepositorioUsuario repo)
        {
            _repo = repo;
        }

        // GET: Usuarios
        public IActionResult Index(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            var resultado = _repo.ObtenerPaginado(pagina, tamanio, busqueda);
            return View(resultado);
        }

        // GET: Usuarios/Details/5
        public IActionResult Details(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null)
            {
                TempData["Mensaje"] = "El usuario solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View(new UsuarioCreateViewModel());
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UsuarioCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_repo.ObtenerPorEmail(model.Email.Trim().ToLower()) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe un usuario registrado con ese correo electrónico.");
                return View(model);
            }

            try
            {
                var usuario = new Usuario
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Email = model.Email,
                    PasswordHash = PasswordHelper.Hash(model.Password),
                    Rol = model.Rol,
                    Avatar = model.Avatar
                };

                _repo.Alta(usuario);
                TempData["Mensaje"] = $"Usuario {usuario.Nombre} {usuario.Apellido} registrado con éxito.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al guardar el usuario: " + ex.Message);
                return View(model);
            }
        }

        // GET: Usuarios/Edit/5
        public IActionResult Edit(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null)
            {
                TempData["Mensaje"] = "El usuario solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var model = new UsuarioEditViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Avatar = usuario.Avatar
            };

            return View(model);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UsuarioEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null)
            {
                TempData["Mensaje"] = "El usuario solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var existente = _repo.ObtenerPorEmail(model.Email.Trim().ToLower());
            if (existente != null && existente.Id != id)
            {
                ModelState.AddModelError(nameof(model.Email), "El correo electrónico ya pertenece a otro usuario.");
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.Password) && model.Password.Length < 6)
            {
                ModelState.AddModelError(nameof(model.Password), "La contraseña debe tener al menos 6 caracteres.");
                return View(model);
            }

            try
            {
                usuario.Nombre = model.Nombre;
                usuario.Apellido = model.Apellido;
                usuario.Email = model.Email;
                usuario.Rol = model.Rol;
                usuario.Avatar = model.Avatar;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    usuario.PasswordHash = PasswordHelper.Hash(model.Password);
                }

                _repo.Modificacion(usuario);
                TempData["Mensaje"] = "Datos del usuario actualizados correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al modificar: " + ex.Message);
                return View(model);
            }
        }

        // GET: Usuarios/Delete/5
        public IActionResult Delete(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null)
            {
                TempData["Mensaje"] = "El usuario no fue encontrado.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            int idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (id == idActual)
            {
                TempData["Mensaje"] = "No puede dar de baja su propia cuenta.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _repo.BajaLogica(id);
                TempData["Mensaje"] = "El usuario ha sido dado de baja lógicamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al dar de baja al usuario: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
