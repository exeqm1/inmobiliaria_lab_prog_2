using Inmobiliaria.Models;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioInquilino
    {
        List<Inquilino> ObtenerTodos();

        PaginacionResultado<Inquilino> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null);

        Inquilino? ObtenerPorId(int id);

        Inquilino? ObtenerPorDni(string dni);

        Inquilino? ObtenerPorEmail(string email);

        List<Inquilino> Buscar(string termino, int max = 10);

        int Alta(Inquilino inquilino);

        int Modificacion(Inquilino inquilino);

        int BajaLogica(int id);
    }
}
