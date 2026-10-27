using System.Security.Claims;
using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.Servicios;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepositorioUsuario _repoUsuarios;
        private readonly IImagenServicio _imagenServicio;

        public AccountController(IRepositorioUsuario repoUsuarios, IImagenServicio imagenServicio)
        {
            _repoUsuarios = repoUsuarios;
            _imagenServicio = imagenServicio;
        }

        // GET: Account/Login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        // POST: Account/Login
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = _repoUsuarios.ObtenerPorEmail(model.Email.Trim().ToLower());

            if (usuario == null || !usuario.Estado || !PasswordHelper.VerificarHash(model.Password, usuario.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
                return View(model);
            }

            // Migración transparente: si la contraseña estaba en texto plano (semilla), la re-hasheamos.
            if (!PasswordHelper.EsHashValido(usuario.PasswordHash))
            {
                usuario.PasswordHash = PasswordHelper.Hash(model.Password);
                _repoUsuarios.Modificacion(usuario);
            }

            await IniciarSesion(usuario, model.Recordarme);

            TempData["Mensaje"] = $"Bienvenido/a, {usuario.Nombre} {usuario.Apellido}.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/Logout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Mensaje"] = "Sesión cerrada correctamente.";
            TempData["TipoMensaje"] = "info";
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/AccesoDenegado
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        // GET: Account/Perfil
        [Authorize]
        [HttpGet]
        public IActionResult Perfil()
        {
            var usuario = _repoUsuarios.ObtenerPorId(IdUsuarioActual());
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(usuario);
        }

        // GET: Account/EditarDatos
        [Authorize]
        [HttpGet]
        public IActionResult EditarDatos()
        {
            var usuario = _repoUsuarios.ObtenerPorId(IdUsuarioActual());
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new EditarPerfilViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email
            });
        }

        // POST: Account/EditarDatos
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarDatos(EditarPerfilViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = _repoUsuarios.ObtenerPorId(IdUsuarioActual());
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var existente = _repoUsuarios.ObtenerPorEmail(model.Email.Trim().ToLower());
            if (existente != null && existente.Id != usuario.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "El correo electrónico ya pertenece a otro usuario.");
                return View(model);
            }

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Email = model.Email;

            try
            {
                _repoUsuarios.Modificacion(usuario);

                // Refresca la cookie para que el nombre/email del navbar se actualicen sin re-loguear.
                await RefrescarSesion(usuario);

                TempData["Mensaje"] = "Tus datos se actualizaron correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al actualizar tus datos: " + ex.Message);
                return View(model);
            }
        }

        // GET: Account/CambiarClave
        [Authorize]
        [HttpGet]
        public IActionResult CambiarClave()
        {
            return View(new CambiarClaveViewModel());
        }

        // POST: Account/CambiarClave
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarClave(CambiarClaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = _repoUsuarios.ObtenerPorId(IdUsuarioActual());
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!PasswordHelper.VerificarHash(model.ClaveActual, usuario.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.ClaveActual), "La contraseña actual es incorrecta.");
                return View(model);
            }

            try
            {
                usuario.PasswordHash = PasswordHelper.Hash(model.ClaveNueva);
                _repoUsuarios.Modificacion(usuario);

                TempData["Mensaje"] = "Tu contraseña se cambió correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al cambiar la contraseña: " + ex.Message);
                return View(model);
            }
        }

        // GET: Account/CambiarAvatar
        [Authorize]
        [HttpGet]
        public IActionResult CambiarAvatar()
        {
            var usuario = _repoUsuarios.ObtenerPorId(IdUsuarioActual());
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new CambiarAvatarViewModel
            {
                Id = usuario.Id,
                Avatar = usuario.Avatar
            });
        }

        // POST: Account/CambiarAvatar
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarAvatar(CambiarAvatarViewModel model)
        {
            var usuario = _repoUsuarios.ObtenerPorId(IdUsuarioActual());
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            model.Avatar = usuario.Avatar;

            if (model.Archivo == null)
            {
                ModelState.AddModelError(nameof(model.Archivo), "Debe seleccionar una imagen.");
                return View(model);
            }

            if (!_imagenServicio.EsImagenValida(model.Archivo, out string? error))
            {
                ModelState.AddModelError(nameof(model.Archivo), error ?? "Debe seleccionar una imagen válida.");
                return View(model);
            }

            try
            {
                string? rutaAnterior = usuario.Avatar;
                string? nuevaRuta = await _imagenServicio.GuardarAsync(model.Archivo, "avatars");
                if (nuevaRuta == null)
                {
                    ModelState.AddModelError(nameof(model.Archivo), "No se pudo guardar la imagen.");
                    return View(model);
                }

                usuario.Avatar = nuevaRuta;
                _repoUsuarios.Modificacion(usuario);

                if (!string.IsNullOrWhiteSpace(rutaAnterior))
                {
                    _imagenServicio.Eliminar(rutaAnterior);
                }

                TempData["Mensaje"] = "Tu foto de perfil se actualizó correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al actualizar la foto de perfil: " + ex.Message);
                return View(model);
            }
        }

        private async Task RefrescarSesion(Usuario usuario)
        {
            var autenticacion = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            bool persistente = autenticacion.Properties?.IsPersistent ?? false;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await IniciarSesion(usuario, persistente);
        }

        private int IdUsuarioActual()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        private async Task IniciarSesion(Usuario usuario, bool recordarme)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Rol)
            };

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad),
                new AuthenticationProperties
                {
                    IsPersistent = recordarme,
                    ExpiresUtc = recordarme ? DateTimeOffset.UtcNow.AddDays(30) : null
                });
        }
    }
}
