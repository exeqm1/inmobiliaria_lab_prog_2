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

        // Da de alta la reserva y, en la misma transacción, el pago de la seña inicial.
        int AltaConSena(Reserva reserva, string concepto, decimal importe);

        int Modificacion(Reserva reserva);

        // Registra una rescisión anticipada y el pago de la multa en una misma transacción.
        int RescindirConPago(int id, DateTime fechaFinEfectiva, decimal multa, string concepto, int usuarioFinalizadorId);

        int BajaLogica(int id);
    }
}
