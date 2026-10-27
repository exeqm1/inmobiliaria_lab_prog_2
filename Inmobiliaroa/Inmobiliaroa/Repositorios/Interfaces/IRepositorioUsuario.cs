using Inmobiliaria.Models;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioUsuario
    {
        List<Usuario> ObtenerTodos();

        Usuario? ObtenerPorId(int id);

        Usuario? ObtenerPorEmail(string email);

        int Alta(Usuario usuario);

        int Modificacion(Usuario usuario);

        int BajaLogica(int id);
    }
}
