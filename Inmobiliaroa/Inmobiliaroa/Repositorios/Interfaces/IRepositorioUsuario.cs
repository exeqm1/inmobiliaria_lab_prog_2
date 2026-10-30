using Inmobiliaria.Models;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioUsuario
    {
        List<Usuario> ObtenerTodos();

        PaginacionResultado<Usuario> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null);

        Usuario? ObtenerPorId(int id);

        Usuario? ObtenerPorEmail(string email);

        int Alta(Usuario usuario);

        int Modificacion(Usuario usuario);

        int BajaLogica(int id);
    }
}
