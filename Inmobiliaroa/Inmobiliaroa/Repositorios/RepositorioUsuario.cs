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
