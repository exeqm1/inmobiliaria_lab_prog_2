using Inmobiliaria.Models;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioTipoInmueble
    {
        List<TipoInmueble> ObtenerTodos();

        PaginacionResultado<TipoInmueble> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null);

        TipoInmueble? ObtenerPorId(int id);

        List<TipoInmueble> Buscar(string termino, int max = 10);

        int Alta(TipoInmueble tipo);

        int Modificacion(TipoInmueble tipo);

        int BajaLogica(int id);
    }
}
