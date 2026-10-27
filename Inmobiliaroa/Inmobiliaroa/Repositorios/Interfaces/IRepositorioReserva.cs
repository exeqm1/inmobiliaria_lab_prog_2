using Inmobiliaria.Models;

namespace Inmobiliaria.Repositorios.Interfaces
{
    public interface IRepositorioReserva
    {
        List<Reserva> ObtenerTodos();

        PaginacionResultado<Reserva> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null);

        Reserva? ObtenerPorId(int id);

        // Indica si el inmueble ya tiene una reserva activa que se superpone con el rango indicado.
        bool ExisteSuperposicion(int inmuebleId, DateTime inicio, DateTime fin, int? excluirReservaId = null);

        int Alta(Reserva reserva);

        int Modificacion(Reserva reserva);

        // Registra una rescisión anticipada: fecha de fin efectiva, multa y usuario finalizador.
        int Rescindir(int id, DateTime fechaFinEfectiva, decimal multa, int usuarioFinalizadorId);

        // Extiende la fecha de fin de una reserva activa (renovación).
        int Renovar(int id, DateTime nuevaFechaFin);

        int BajaLogica(int id);
    }
}
