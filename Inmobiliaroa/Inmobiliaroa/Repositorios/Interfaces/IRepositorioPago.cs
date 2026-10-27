using Inmobiliaria.Models;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioPago
    {
        List<Pago> ObtenerPorReserva(int reservaId);

        Pago? ObtenerPorId(int id);

        decimal TotalPagado(int reservaId);

        int Alta(Pago pago);

        int Anular(int id, int usuarioAnuladorId);
    }
}
