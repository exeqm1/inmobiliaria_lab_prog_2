namespace Inmobiliaria.ViewModels
{
    public class ReporteReservaItem
    {
        public int Id { get; set; }

        public string Inmueble { get; set; } = string.Empty;

        public string Inquilino { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public DateTime? FechaFinEfectiva { get; set; }

        public decimal MontoDiario { get; set; }

        public decimal? Multa { get; set; }

        public decimal TotalPagado { get; set; }

        public int Dias => (FechaFin - FechaInicio).Days;

        public decimal Total => (Dias * MontoDiario) + (Multa ?? 0);

        public decimal Saldo => Total - TotalPagado;

        public bool Rescindida => FechaFinEfectiva.HasValue;
    }
}
