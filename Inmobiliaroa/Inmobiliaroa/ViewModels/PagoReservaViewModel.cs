using Inmobiliaria.Models;

namespace Inmobiliaria.ViewModels
{
    public class PagoReservaViewModel
    {
        public Reserva Reserva { get; set; } = null!;

        public List<Pago> Pagos { get; set; } = new();

        public decimal TotalPagado { get; set; }

        public int Dias => (Reserva.FechaFin - Reserva.FechaInicio).Days;

        public decimal TotalReserva => (Dias * Reserva.MontoDiario) + (Reserva.Multa ?? 0);

        public decimal Saldo => TotalReserva - TotalPagado;
    }
}
