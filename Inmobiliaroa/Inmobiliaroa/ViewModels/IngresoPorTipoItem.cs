namespace Inmobiliaria.ViewModels
{
    public class IngresoPorTipoItem
    {
        public string TipoDescripcion { get; set; } = string.Empty;

        public int CantidadReservas { get; set; }

        public decimal IngresosCobrados { get; set; }
    }
}
