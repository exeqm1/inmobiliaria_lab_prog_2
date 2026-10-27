using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.ViewModels
{
    public class CambiarClaveViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string ClaveActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string ClaveNueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(ClaveNueva), ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
        [Display(Name = "Confirmar Nueva Contraseña")]
        public string ConfirmarClave { get; set; } = string.Empty;
    }
}
