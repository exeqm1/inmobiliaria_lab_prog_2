namespace Inmobiliaria.ViewModels
{
    public class ReporteReservasViewModel
    {
        public DateTime Desde { get; set; }

        public DateTime Hasta { get; set; }

        public List<ReporteReservaItem> Reservas { get; set; } = new();

        public int Cantidad => Reservas.Count;

        public decimal TotalFacturado => Reservas.Sum(r => r.Total);

        public decimal TotalCobrado => Reservas.Sum(r => r.TotalPagado);

        public decimal TotalMultas => Reservas.Sum(r => r.Multa ?? 0);

        public decimal TotalSaldo => Reservas.Sum(r => r.Saldo);
    }
}
