using System.Data;
using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public class RepositorioInquilino : RepositorioBase, IRepositorioInquilino
    {
        public RepositorioInquilino(IConfiguration configuration) : base(configuration)
        {
        }

        public List<Inquilino> ObtenerTodos()
        {
            var lista = new List<Inquilino>();
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Inquilinos 
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

        public PaginacionResultado<Inquilino> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanio < 1) tamanio = 10;
            int offset = (pagina - 1) * tamanio;

            var resultado = new PaginacionResultado<Inquilino>
            {
                PaginaActual = pagina,
                ElementosPorPagina = tamanio,
                CriterioBusqueda = busqueda
            };

            bool tieneFiltro = !string.IsNullOrWhiteSpace(busqueda);
            string filtroSql = tieneFiltro
                ? " AND (nombre LIKE @filtro OR apellido LIKE @filtro OR dni LIKE @filtro OR email LIKE @filtro)"
                : "";

            string sqlCount = $"SELECT COUNT(*) FROM Inquilinos WHERE estado = 1 {filtroSql}";
            string sqlData = $@"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Inquilinos 
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

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? inquilino = null;
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Inquilinos 
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                inquilino = Mapear(reader);
            }

            return inquilino;
        }

        public Inquilino? ObtenerPorDni(string dni)
        {
            Inquilino? inquilino = null;
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Inquilinos 
                WHERE dni = @dni";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@dni", dni);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                inquilino = Mapear(reader);
            }

            return inquilino;
        }

        public Inquilino? ObtenerPorEmail(string email)
        {
            Inquilino? inquilino = null;
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Inquilinos 
                WHERE email = @email";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@email", email);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                inquilino = Mapear(reader);
            }

            return inquilino;
        }

        public List<Inquilino> Buscar(string termino, int max = 10)
        {
            var lista = new List<Inquilino>();
            const string sql = @"
                SELECT id, dni, nombre, apellido, telefono, email, estado 
                FROM Inquilinos 
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

        public int Alta(Inquilino i)
        {
            const string sql = @"
                INSERT INTO Inquilinos (dni, nombre, apellido, telefono, email, estado) 
                VALUES (@dni, @nombre, @apellido, @telefono, @email, 1);
                SELECT LAST_INSERT_ID();";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@dni", i.Dni.Trim());
            comando.Parameters.AddWithValue("@nombre", i.Nombre.Trim());
            comando.Parameters.AddWithValue("@apellido", i.Apellido.Trim());
            comando.Parameters.AddWithValue("@telefono", i.Telefono.Trim());
            comando.Parameters.AddWithValue("@email", i.Email.Trim().ToLower());
            conexion.Open();

            i.Id = Convert.ToInt32(comando.ExecuteScalar());
            return i.Id;
        }

        public int Modificacion(Inquilino i)
        {
            const string sql = @"
                UPDATE Inquilinos 
                SET dni = @dni, 
                    nombre = @nombre, 
                    apellido = @apellido, 
                    telefono = @telefono, 
                    email = @email 
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@dni", i.Dni.Trim());
            comando.Parameters.AddWithValue("@nombre", i.Nombre.Trim());
            comando.Parameters.AddWithValue("@apellido", i.Apellido.Trim());
            comando.Parameters.AddWithValue("@telefono", i.Telefono.Trim());
            comando.Parameters.AddWithValue("@email", i.Email.Trim().ToLower());
            comando.Parameters.AddWithValue("@id", i.Id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int BajaLogica(int id)
        {
            const string sql = "UPDATE Inquilinos SET estado = 0 WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        private static Inquilino Mapear(MySqlDataReader reader)
        {
            return new Inquilino
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

