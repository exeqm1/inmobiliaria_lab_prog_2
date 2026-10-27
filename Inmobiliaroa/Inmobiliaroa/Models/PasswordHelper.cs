using System.Security.Cryptography;

namespace Inmobiliaria.Models
{
    // Hashing de contraseñas con PBKDF2 (SHA256). Formato almacenado: "iteraciones.saltBase64.hashBase64".
    public static class PasswordHelper
    {
        private const int Iteraciones = 100_000;
        private const int TamanioSalt = 16;
        private const int TamanioHash = 32;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(TamanioSalt);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iteraciones, HashAlgorithmName.SHA256, TamanioHash);

            return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerificarHash(string password, string almacenado)
        {
            if (string.IsNullOrEmpty(almacenado))
            {
                return false;
            }

            string[] partes = almacenado.Split('.');

            // Compatibilidad con contraseñas semilla en texto plano (migración transparente al iniciar sesión).
            if (partes.Length != 3)
            {
                return string.Equals(password, almacenado, StringComparison.Ordinal);
            }

            if (!int.TryParse(partes[0], out int iteraciones) || iteraciones <= 0)
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(partes[1]);
                byte[] esperado = Convert.FromBase64String(partes[2]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

                return CryptographicOperations.FixedTimeEquals(actual, esperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Indica si el valor almacenado ya está en formato hash (no texto plano).
        public static bool EsHashValido(string almacenado)
        {
            return !string.IsNullOrEmpty(almacenado) && almacenado.Split('.').Length == 3;
        }
    }
}
