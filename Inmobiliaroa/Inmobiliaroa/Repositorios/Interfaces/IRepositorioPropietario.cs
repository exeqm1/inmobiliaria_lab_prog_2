using Inmobiliaria.Models;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioPropietario
    {
        List<Propietario> ObtenerTodos();

        PaginacionResultado<Propietario> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null);

        Propietario? ObtenerPorId(int id);

        Propietario? ObtenerPorDni(string dni);

        Propietario? ObtenerPorEmail(string email);

        List<Propietario> Buscar(string termino, int max = 10);

        int Alta(Propietario propietario);

        int Modificacion(Propietario propietario);

        int BajaLogica(int id);
    }
}
