using System.Data;
using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public class RepositorioPropietario : RepositorioBase, IRepositorioPropietario
    {
        public RepositorioPropietario(IConfiguration configuration) : base(configuration)
        {
        }

        public List<Propietario> ObtenerTodos()
        {
            var lista = new List<Propietario>();
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Propietarios 
                WHERE estado = 1 
                ORDER BY apellido, nombre";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        public PaginacionResultado<Propietario> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanio < 1) tamanio = 10;
            int offset = (pagina - 1) * tamanio;

            var resultado = new PaginacionResultado<Propietario>
            {
                PaginaActual = pagina,
                ElementosPorPagina = tamanio,
                CriterioBusqueda = busqueda
            };

            bool tieneFiltro = !string.IsNullOrWhiteSpace(busqueda);
            string filtroSql = tieneFiltro
                ? " AND (nombre LIKE @filtro OR apellido LIKE @filtro OR dni LIKE @filtro OR email LIKE @filtro)"
                : "";

            string sqlCount = $"SELECT COUNT(*) FROM Propietarios WHERE estado = 1 {filtroSql}";
            string sqlData = $@"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Propietarios 
                WHERE estado = 1 {filtroSql} 
                ORDER BY id DESC 
                LIMIT @limit OFFSET @offset";

            using var conexion = GetConnection();
            conexion.Open();

            // Total elementos
            using (var cmdCount = new MySqlCommand(sqlCount, conexion))
            {
                if (tieneFiltro)
                {
                    cmdCount.Parameters.AddWithValue("@filtro", $"%{busqueda}%");
                }
                resultado.TotalElementos = Convert.ToInt32(cmdCount.ExecuteScalar());
            }

            // Página actual
            using (var cmdData = new MySqlCommand(sqlData, conexion))
            {
                if (tieneFiltro)
                {
                    cmdData.Parameters.AddWithValue("@filtro", $"%{busqueda}%");
                }
                cmdData.Parameters.AddWithValue("@limit", tamanio);
                cmdData.Parameters.AddWithValue("@offset", offset);

                using var reader = cmdData.ExecuteReader();
                while (reader.Read())
                {
                    resultado.Elementos.Add(Mapear(reader));
                }
            }

            return resultado;
        }

        public Propietario? ObtenerPorId(int id)
        {
            Propietario? propietario = null;
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Propietarios 
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                propietario = Mapear(reader);
            }

            return propietario;
        }

        public Propietario? ObtenerPorDni(string dni)
        {
            Propietario? propietario = null;
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Propietarios 
                WHERE dni = @dni";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@dni", dni);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                propietario = Mapear(reader);
            }

            return propietario;
        }

        public Propietario? ObtenerPorEmail(string email)
        {
            Propietario? propietario = null;
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Propietarios 
                WHERE email = @email";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@email", email);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                propietario = Mapear(reader);
            }

            return propietario;
        }

        public List<Propietario> Buscar(string termino, int max = 10)
        {
            var lista = new List<Propietario>();
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Propietarios 
                WHERE estado = 1 AND (nombre LIKE @term OR apellido LIKE @term OR dni LIKE @term)
                ORDER BY apellido, nombre
                LIMIT @max";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@term", $"%{termino}%");
            comando.Parameters.AddWithValue("@max", max);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        public int Alta(Propietario p)
        {
            const string sql = @"
                INSERT INTO Propietarios (dni, nombre, apellido, telefono, email, estado) 
                VALUES (@dni, @nombre, @apellido, @telefono, @email, 1);
                SELECT LAST_INSERT_ID();";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@dni", p.Dni.Trim());
            comando.Parameters.AddWithValue("@nombre", p.Nombre.Trim());
            comando.Parameters.AddWithValue("@apellido", p.Apellido.Trim());
            comando.Parameters.AddWithValue("@telefono", p.Telefono.Trim());
            comando.Parameters.AddWithValue("@email", p.Email.Trim().ToLower());
            conexion.Open();

            p.Id = Convert.ToInt32(comando.ExecuteScalar());
            return p.Id;
        }

        public int Modificacion(Propietario p)
        {
            const string sql = @"
                UPDATE Propietarios 
                SET dni = @dni, 
                    nombre = @nombre, 
                    apellido = @apellido, 
                    telefono = @telefono, 
                    email = @email 
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@dni", p.Dni.Trim());
            comando.Parameters.AddWithValue("@nombre", p.Nombre.Trim());
            comando.Parameters.AddWithValue("@apellido", p.Apellido.Trim());
            comando.Parameters.AddWithValue("@telefono", p.Telefono.Trim());
            comando.Parameters.AddWithValue("@email", p.Email.Trim().ToLower());
            comando.Parameters.AddWithValue("@id", p.Id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int BajaLogica(int id)
        {
            const string sql = "UPDATE Propietarios SET estado = 0 WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        private static Propietario Mapear(MySqlDataReader reader)
        {
            return new Propietario
            {
                Id = reader.GetInt32("id"),
                Dni = reader.GetString("dni"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                Telefono = reader.GetString("telefono"),
                Email = reader.GetString("email"),
                Estado = reader.GetBoolean("estado")
            };
        }
    }
}

