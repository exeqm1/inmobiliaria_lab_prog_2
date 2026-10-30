using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public class RepositorioPago : RepositorioBase, IRepositorioPago
    {
        public RepositorioPago(IConfiguration configuration) : base(configuration)
        {
        }

        private const string SelectBase = @"
            SELECT p.id, p.reserva_id, p.concepto, p.fecha_pago, p.importe, p.usuario_creador_id,
                   p.usuario_anulador_id, p.anulado,
                   uc.nombre AS creador_nombre, uc.apellido AS creador_apellido,
                   ua.nombre AS anulador_nombre, ua.apellido AS anulador_apellido
            FROM Pagos p
            INNER JOIN Usuarios uc ON p.usuario_creador_id = uc.id
            LEFT JOIN Usuarios ua ON p.usuario_anulador_id = ua.id";

        public List<Pago> ObtenerPorReserva(int reservaId)
        {
            var lista = new List<Pago>();
            string sql = SelectBase + " WHERE p.reserva_id = @reservaId ORDER BY p.fecha_pago DESC, p.id DESC";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@reservaId", reservaId);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        public Pago? ObtenerPorId(int id)
        {
            Pago? pago = null;
            string sql = SelectBase + " WHERE p.id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                pago = Mapear(reader);
            }

            return pago;
        }

        public decimal TotalPagado(int reservaId)
        {
            const string sql = "SELECT COALESCE(SUM(importe), 0) FROM Pagos WHERE reserva_id = @reservaId AND anulado = 0";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@reservaId", reservaId);
            conexion.Open();

            return Convert.ToDecimal(comando.ExecuteScalar());
        }

        public int Alta(Pago p)
        {
            const string sql = @"
                INSERT INTO Pagos (reserva_id, concepto, fecha_pago, importe, usuario_creador_id, anulado)
                VALUES (@reservaId, @concepto, @fechaPago, @importe, @usuarioCreadorId, 0);
                SELECT LAST_INSERT_ID();";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@reservaId", p.ReservaId);
            comando.Parameters.AddWithValue("@concepto", p.Concepto.Trim());
            comando.Parameters.AddWithValue("@fechaPago", p.FechaPago);
            comando.Parameters.AddWithValue("@importe", p.Importe);
            comando.Parameters.AddWithValue("@usuarioCreadorId", p.UsuarioCreadorId);
            conexion.Open();

            p.Id = Convert.ToInt32(comando.ExecuteScalar());
            return p.Id;
        }

        public int ModificacionConcepto(int id, string concepto)
        {
            const string sql = "UPDATE Pagos SET concepto = @concepto WHERE id = @id AND anulado = 0";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@concepto", concepto.Trim());
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int Anular(int id, int usuarioAnuladorId)
        {
            const string sql = "UPDATE Pagos SET anulado = 1, usuario_anulador_id = @usuarioAnuladorId WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@usuarioAnuladorId", usuarioAnuladorId);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        private static Pago Mapear(MySqlDataReader reader)
        {
            var pago = new Pago
            {
                Id = reader.GetInt32("id"),
                ReservaId = reader.GetInt32("reserva_id"),
                Concepto = reader.GetString("concepto"),
                FechaPago = reader.GetDateTime("fecha_pago"),
                Importe = reader.GetDecimal("importe"),
                UsuarioCreadorId = reader.GetInt32("usuario_creador_id"),
                UsuarioAnuladorId = reader["usuario_anulador_id"] as int?,
                Anulado = reader.GetBoolean("anulado"),
                UsuarioCreador = new Usuario
                {
                    Id = reader.GetInt32("usuario_creador_id"),
                    Nombre = reader.GetString("creador_nombre"),
                    Apellido = reader.GetString("creador_apellido")
                }
            };

            if (pago.UsuarioAnuladorId.HasValue)
            {
                pago.UsuarioAnulador = new Usuario
                {
                    Id = pago.UsuarioAnuladorId.Value,
                    Nombre = reader.GetString("anulador_nombre"),
                    Apellido = reader.GetString("anulador_apellido")
                };
            }

            return pago;
        }
    }
}
