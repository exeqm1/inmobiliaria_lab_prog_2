using Inmobiliaria.Models;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioInmueble
    {
        List<Inmueble> ObtenerTodos();

        PaginacionResultado<Inmueble> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null);

        Inmueble? ObtenerPorId(int id);

        List<Inmueble> Buscar(string termino, int max = 10);

        int Alta(Inmueble inmueble);

        int Modificacion(Inmueble inmueble);

        int BajaLogica(int id);

        int AgregarFoto(int inmuebleId, string urlFoto);

        int EliminarFoto(int fotoId);
    }
}
