using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.Servicios;
using Inmobiliaria.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private const string CarpetaImagenes = "inmuebles";

        private readonly IRepositorioInmueble _repo;
        private readonly IRepositorioTipoInmueble _repoTipos;
        private readonly IRepositorioPropietario _repoPropietarios;
        private readonly IImagenServicio _imagenes;

        public InmueblesController(
            IRepositorioInmueble repo,
            IRepositorioTipoInmueble repoTipos,
            IRepositorioPropietario repoPropietarios,
            IImagenServicio imagenes)
        {
            _repo = repo;
            _repoTipos = repoTipos;
            _repoPropietarios = repoPropietarios;
            _imagenes = imagenes;
        }

        // GET: Inmuebles
        public IActionResult Index(int pagina = 1, int tamanio = 10, string? busqueda = null, bool? disponible = null)
        {
            var resultado = _repo.ObtenerPaginado(pagina, tamanio, busqueda, disponible);
            ViewBag.Disponible = disponible;
            return View(resultado);
        }

        // GET: Inmuebles/Details/5
        public IActionResult Details(int id)
        {
            var inmueble = _repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                TempData["Mensaje"] = "El inmueble solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }

        // GET: Inmuebles/Create
        public IActionResult Create()
        {
            CargarCombos();
            return View(new InmuebleFormViewModel());
        }

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InmuebleFormViewModel model)
        {
            ValidarArchivos(model);

            if (!ModelState.IsValid)
            {
                CargarCombos(model);
                return View(model);
            }

            var inmueble = new Inmueble
            {
                Direccion = model.Direccion,
                Cupo = model.Cupo,
                TipoId = model.TipoId,
                Coordenadas = model.Coordenadas,
                PrecioDia = model.PrecioDia,
                PorcentajeReserva = model.PorcentajeReserva,
                Disponible = model.Disponible,
                PropietarioId = model.PropietarioId
            };

            if (model.PortadaArchivo != null)
            {
                inmueble.Portada = await _imagenes.GuardarAsync(model.PortadaArchivo, CarpetaImagenes);
            }

            try
            {
                _repo.Alta(inmueble);
                await GuardarFotosAsync(inmueble.Id, model.FotosArchivos);

                TempData["Mensaje"] = $"Inmueble \"{inmueble.Direccion}\" registrado con éxito.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _imagenes.Eliminar(inmueble.Portada);
                ModelState.AddModelError(string.Empty, "Error al guardar el inmueble: " + ex.Message);
                CargarCombos(model);
                return View(model);
            }
        }

        // GET: Inmuebles/Edit/5
        public IActionResult Edit(int id)
        {
            var inmueble = _repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                TempData["Mensaje"] = "El inmueble solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var model = new InmuebleFormViewModel
            {
                Id = inmueble.Id,
                Direccion = inmueble.Direccion,
                Cupo = inmueble.Cupo,
                TipoId = inmueble.TipoId,
                Coordenadas = inmueble.Coordenadas,
                PrecioDia = inmueble.PrecioDia,
                PorcentajeReserva = inmueble.PorcentajeReserva,
                Disponible = inmueble.Disponible,
                PropietarioId = inmueble.PropietarioId,
                PortadaActual = inmueble.Portada,
                FotosExistentes = inmueble.Fotos
            };

            CargarCombos(model);
            return View(model);
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InmuebleFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ValidarArchivos(model);

            if (!ModelState.IsValid)
            {
                CargarCombos(model);
                model.FotosExistentes = _repo.ObtenerPorId(id)?.Fotos ?? new List<FotoInmueble>();
                return View(model);
            }

            var inmueble = _repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                TempData["Mensaje"] = "El inmueble solicitado no existe.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            string? portadaAnterior = inmueble.Portada;

            inmueble.Direccion = model.Direccion;
            inmueble.Cupo = model.Cupo;
            inmueble.TipoId = model.TipoId;
            inmueble.Coordenadas = model.Coordenadas;
            inmueble.PrecioDia = model.PrecioDia;
            inmueble.PorcentajeReserva = model.PorcentajeReserva;
            inmueble.Disponible = model.Disponible;
            inmueble.PropietarioId = model.PropietarioId;

            if (model.PortadaArchivo != null)
            {
                inmueble.Portada = await _imagenes.GuardarAsync(model.PortadaArchivo, CarpetaImagenes);
            }

            try
            {
                _repo.Modificacion(inmueble);

                // Si se reemplazó la portada, eliminar la anterior.
                if (model.PortadaArchivo != null)
                {
                    _imagenes.Eliminar(portadaAnterior);
                }

                // Eliminar fotos de galería marcadas por el usuario.
                foreach (int fotoId in model.FotosAEliminar)
                {
                    var foto = inmueble.Fotos.FirstOrDefault(f => f.Id == fotoId);
                    if (foto != null)
                    {
                        _repo.EliminarFoto(fotoId);
                        _imagenes.Eliminar(foto.UrlFoto);
                    }
                }

                await GuardarFotosAsync(inmueble.Id, model.FotosArchivos);

                TempData["Mensaje"] = "Inmueble actualizado correctamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al modificar: " + ex.Message);
                CargarCombos(model);
                model.FotosExistentes = inmueble.Fotos;
                return View(model);
            }
        }

        // GET: Inmuebles/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var inmueble = _repo.ObtenerPorId(id);
            if (inmueble == null)
            {
                TempData["Mensaje"] = "El inmueble no fue encontrado.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }

        // POST: Inmuebles/Delete/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repo.BajaLogica(id);
                TempData["Mensaje"] = "El inmueble ha sido dado de baja lógicamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al dar de baja el inmueble: " + ex.Message;
                TempData["TipoMensaje"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        private void CargarCombos(InmuebleFormViewModel? model = null)
        {
            ViewBag.Tipos = new SelectList(_repoTipos.ObtenerTodos(), "Id", "Descripcion", model?.TipoId);
            ViewBag.PropietarioTexto = model != null && model.PropietarioId > 0
                ? _repoPropietarios.ObtenerPorId(model.PropietarioId)?.NombreCompleto
                : null;
        }

        // GET: Inmuebles/Buscar?q=texto&disponibles=true
        [HttpGet]
        public IActionResult Buscar(string q, bool? disponibles = null)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<OpcionSelect>());
            }

            var lista = _repo.Buscar(q)
                .Where(i => disponibles != true || i.Disponible)
                .Select(i => new OpcionSelect
                {
                    Id = i.Id,
                    Text = $"{i.Direccion} - ${i.PrecioDia:N2}/día"
                });

            return Json(lista);
        }

        private void ValidarArchivos(InmuebleFormViewModel model)
        {
            if (model.PortadaArchivo != null && !_imagenes.EsImagenValida(model.PortadaArchivo, out string? errorPortada))
            {
                ModelState.AddModelError(nameof(model.PortadaArchivo), errorPortada!);
            }

            if (model.FotosArchivos != null)
            {
                foreach (var foto in model.FotosArchivos)
                {
                    if (!_imagenes.EsImagenValida(foto, out string? errorFoto))
                    {
                        ModelState.AddModelError(nameof(model.FotosArchivos), errorFoto!);
                    }
                }
            }
        }

        private async Task GuardarFotosAsync(int inmuebleId, List<IFormFile>? fotos)
        {
            if (fotos == null)
            {
                return;
            }

            foreach (var foto in fotos)
            {
                string? ruta = await _imagenes.GuardarAsync(foto, CarpetaImagenes);
                if (ruta != null)
                {
                    _repo.AgregarFoto(inmuebleId, ruta);
                }
            }
        }
    }
}
