using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public abstract class RepositorioBase
    {
        protected readonly string _connectionString;

        public RepositorioBase(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada en appsettings.json.");
        }

        protected MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}

