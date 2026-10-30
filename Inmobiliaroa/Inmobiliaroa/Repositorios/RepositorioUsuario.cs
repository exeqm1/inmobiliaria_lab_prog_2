using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration)
        {
        }

        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            const string sql = @"
                SELECT id, nombre, apellido, email, password, rol, avatar, estado
                FROM Usuarios
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

        public PaginacionResultado<Usuario> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanio < 1) tamanio = 10;
            int offset = (pagina - 1) * tamanio;

            var resultado = new PaginacionResultado<Usuario>
            {
                PaginaActual = pagina,
                ElementosPorPagina = tamanio,
                CriterioBusqueda = busqueda
            };

            bool tieneFiltro = !string.IsNullOrWhiteSpace(busqueda);
            string filtroSql = tieneFiltro
                ? " AND (nombre LIKE @filtro OR apellido LIKE @filtro OR email LIKE @filtro OR rol LIKE @filtro)"
                : "";

            string sqlCount = $"SELECT COUNT(*) FROM Usuarios WHERE estado = 1 {filtroSql}";
            string sqlData = $@"
                SELECT id, nombre, apellido, email, password, rol, avatar, estado
                FROM Usuarios
                WHERE estado = 1 {filtroSql}
                ORDER BY apellido, nombre
                LIMIT @limit OFFSET @offset";

            using var conexion = GetConnection();
            conexion.Open();

            using (var cmdCount = new MySqlCommand(sqlCount, conexion))
            {
                if (tieneFiltro)
                {
                    cmdCount.Parameters.AddWithValue("@filtro", $"%{busqueda}%");
                }
                resultado.TotalElementos = Convert.ToInt32(cmdCount.ExecuteScalar());
            }

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

        public Usuario? ObtenerPorId(int id)
        {
            Usuario? usuario = null;
            const string sql = @"
                SELECT id, nombre, apellido, email, password, rol, avatar, estado
                FROM Usuarios
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                usuario = Mapear(reader);
            }

            return usuario;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            Usuario? usuario = null;
            const string sql = @"
                SELECT id, nombre, apellido, email, password, rol, avatar, estado
                FROM Usuarios
                WHERE email = @email";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@email", email);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                usuario = Mapear(reader);
            }

            return usuario;
        }

        public int Alta(Usuario u)
        {
            const string sql = @"
                INSERT INTO Usuarios (nombre, apellido, email, password, rol, avatar, estado)
                VALUES (@nombre, @apellido, @email, @password, @rol, @avatar, 1);
                SELECT LAST_INSERT_ID();";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@nombre", u.Nombre.Trim());
            comando.Parameters.AddWithValue("@apellido", u.Apellido.Trim());
            comando.Parameters.AddWithValue("@email", u.Email.Trim().ToLower());
            comando.Parameters.AddWithValue("@password", u.PasswordHash);
            comando.Parameters.AddWithValue("@rol", u.Rol.Trim());
            comando.Parameters.AddWithValue("@avatar", (object?)u.Avatar ?? DBNull.Value);
            conexion.Open();

            u.Id = Convert.ToInt32(comando.ExecuteScalar());
            return u.Id;
        }

        public int Modificacion(Usuario u)
        {
            const string sql = @"
                UPDATE Usuarios
                SET nombre = @nombre,
                    apellido = @apellido,
                    email = @email,
                    password = @password,
                    rol = @rol,
                    avatar = @avatar
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@nombre", u.Nombre.Trim());
            comando.Parameters.AddWithValue("@apellido", u.Apellido.Trim());
            comando.Parameters.AddWithValue("@email", u.Email.Trim().ToLower());
            comando.Parameters.AddWithValue("@password", u.PasswordHash);
            comando.Parameters.AddWithValue("@rol", u.Rol.Trim());
            comando.Parameters.AddWithValue("@avatar", (object?)u.Avatar ?? DBNull.Value);
            comando.Parameters.AddWithValue("@id", u.Id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int BajaLogica(int id)
        {
            const string sql = "UPDATE Usuarios SET estado = 0 WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        private static Usuario Mapear(MySqlDataReader reader)
        {
            return new Usuario
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                Email = reader.GetString("email"),
                PasswordHash = reader.GetString("password"),
                Rol = reader.GetString("rol"),
                Avatar = reader["avatar"] as string,
                Estado = reader.GetBoolean("estado")
            };
        }
    }
}
