namespace Inmobiliaria.Models
{
    public class PaginacionResultado<T>
    {
        public List<T> Elementos { get; set; } = new();
        public int PaginaActual { get; set; } = 1;
        public int ElementosPorPagina { get; set; } = 10;
        public int TotalElementos { get; set; } = 0;
        public int TotalPaginas => (int)Math.Ceiling((double)TotalElementos / (ElementosPorPagina > 0 ? ElementosPorPagina : 10));
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
        public string? CriterioBusqueda { get; set; }
    }
}

