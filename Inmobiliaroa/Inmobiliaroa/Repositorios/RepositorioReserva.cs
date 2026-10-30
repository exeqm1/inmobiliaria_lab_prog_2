using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public class RepositorioReserva : RepositorioBase, IRepositorioReserva
    {
        public RepositorioReserva(IConfiguration configuration) : base(configuration)
        {
        }

        private const string SelectBase = @"
            SELECT r.id, r.inmueble_id, r.inquilino_id, r.fecha_inicio, r.fecha_fin, r.monto_diario,
                   r.fecha_fin_efectiva, r.multa, r.usuario_creador_id, r.usuario_finalizador_id, r.estado,
                   i.direccion AS inmueble_direccion,
                   inq.nombre AS inquilino_nombre, inq.apellido AS inquilino_apellido,
                   uc.nombre AS creador_nombre, uc.apellido AS creador_apellido,
                   uf.nombre AS finalizador_nombre, uf.apellido AS finalizador_apellido
            FROM Reservas r
            INNER JOIN Inmuebles i ON r.inmueble_id = i.id
            INNER JOIN Inquilinos inq ON r.inquilino_id = inq.id
            INNER JOIN Usuarios uc ON r.usuario_creador_id = uc.id
            LEFT JOIN Usuarios uf ON r.usuario_finalizador_id = uf.id";

        public List<Reserva> ObtenerTodos()
        {
            var lista = new List<Reserva>();
            string sql = SelectBase + " WHERE r.estado = 1 ORDER BY r.fecha_inicio DESC";

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

        public PaginacionResultado<Reserva> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanio < 1) tamanio = 10;
            int offset = (pagina - 1) * tamanio;

            var resultado = new PaginacionResultado<Reserva>
            {
                PaginaActual = pagina,
                ElementosPorPagina = tamanio,
                CriterioBusqueda = busqueda
            };

            bool tieneFiltro = !string.IsNullOrWhiteSpace(busqueda);
            string filtroSql = tieneFiltro
                ? " AND (i.direccion LIKE @filtro OR inq.nombre LIKE @filtro OR inq.apellido LIKE @filtro)"
                : "";

            string sqlCount = $@"
                SELECT COUNT(*)
                FROM Reservas r
                INNER JOIN Inmuebles i ON r.inmueble_id = i.id
                INNER JOIN Inquilinos inq ON r.inquilino_id = inq.id
                WHERE r.estado = 1 {filtroSql}";

            string sqlData = SelectBase + $" WHERE r.estado = 1 {filtroSql} ORDER BY r.fecha_inicio DESC LIMIT @limit OFFSET @offset";

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

        public Reserva? ObtenerPorId(int id)
        {
            Reserva? reserva = null;
            string sql = SelectBase + " WHERE r.id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                reserva = Mapear(reader);
            }

            return reserva;
        }

        public bool ExisteSuperposicion(int inmuebleId, DateTime inicio, DateTime fin, int? excluirReservaId = null)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM Reservas
                WHERE inmueble_id = @inmuebleId
                  AND estado = 1
                  AND fecha_inicio < @fin
                  AND fecha_fin > @inicio";

            if (excluirReservaId.HasValue)
            {
                sql += " AND id <> @excluir";
            }

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@inmuebleId", inmuebleId);
            comando.Parameters.AddWithValue("@inicio", inicio.Date);
            comando.Parameters.AddWithValue("@fin", fin.Date);
            if (excluirReservaId.HasValue)
            {
                comando.Parameters.AddWithValue("@excluir", excluirReservaId.Value);
            }
            conexion.Open();

            return Convert.ToInt32(comando.ExecuteScalar()) > 0;
        }

        public int Alta(Reserva r)
        {
            const string sql = @"
                INSERT INTO Reservas (inmueble_id, inquilino_id, fecha_inicio, fecha_fin, monto_diario, fecha_fin_efectiva, multa, usuario_creador_id, estado)
                VALUES (@inmuebleId, @inquilinoId, @fechaInicio, @fechaFin, @montoDiario, NULL, 0, @usuarioCreadorId, 1);
                SELECT LAST_INSERT_ID();";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@inmuebleId", r.InmuebleId);
            comando.Parameters.AddWithValue("@inquilinoId", r.InquilinoId);
            comando.Parameters.AddWithValue("@fechaInicio", r.FechaInicio.Date);
            comando.Parameters.AddWithValue("@fechaFin", r.FechaFin.Date);
            comando.Parameters.AddWithValue("@montoDiario", r.MontoDiario);
            comando.Parameters.AddWithValue("@usuarioCreadorId", r.UsuarioCreadorId);
            conexion.Open();

            r.Id = Convert.ToInt32(comando.ExecuteScalar());
            return r.Id;
        }

        public int AltaConSena(Reserva r, string concepto, decimal importe)
        {
            const string sqlReserva = @"
                INSERT INTO Reservas (inmueble_id, inquilino_id, fecha_inicio, fecha_fin, monto_diario, fecha_fin_efectiva, multa, usuario_creador_id, estado)
                VALUES (@inmuebleId, @inquilinoId, @fechaInicio, @fechaFin, @montoDiario, NULL, 0, @usuarioCreadorId, 1);
                SELECT LAST_INSERT_ID();";

            const string sqlPago = @"
                INSERT INTO Pagos (reserva_id, concepto, fecha_pago, importe, usuario_creador_id, anulado)
                VALUES (@reservaId, @concepto, NOW(), @importe, @usuarioCreadorId, 0);";

            using var conexion = GetConnection();
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            using var cmdReserva = new MySqlCommand(sqlReserva, conexion, transaccion);
            cmdReserva.Parameters.AddWithValue("@inmuebleId", r.InmuebleId);
            cmdReserva.Parameters.AddWithValue("@inquilinoId", r.InquilinoId);
            cmdReserva.Parameters.AddWithValue("@fechaInicio", r.FechaInicio.Date);
            cmdReserva.Parameters.AddWithValue("@fechaFin", r.FechaFin.Date);
            cmdReserva.Parameters.AddWithValue("@montoDiario", r.MontoDiario);
            cmdReserva.Parameters.AddWithValue("@usuarioCreadorId", r.UsuarioCreadorId);

            r.Id = Convert.ToInt32(cmdReserva.ExecuteScalar());

            using var cmdPago = new MySqlCommand(sqlPago, conexion, transaccion);
            cmdPago.Parameters.AddWithValue("@reservaId", r.Id);
            cmdPago.Parameters.AddWithValue("@concepto", concepto.Trim());
            cmdPago.Parameters.AddWithValue("@importe", importe);
            cmdPago.Parameters.AddWithValue("@usuarioCreadorId", r.UsuarioCreadorId);
            cmdPago.ExecuteNonQuery();

            transaccion.Commit();
            return r.Id;
        }

        public int Modificacion(Reserva r)
        {
            const string sql = @"
                UPDATE Reservas
                SET inquilino_id = @inquilinoId,
                    fecha_inicio = @fechaInicio,
                    fecha_fin = @fechaFin
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@inquilinoId", r.InquilinoId);
            comando.Parameters.AddWithValue("@fechaInicio", r.FechaInicio.Date);
            comando.Parameters.AddWithValue("@fechaFin", r.FechaFin.Date);
            comando.Parameters.AddWithValue("@id", r.Id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int RescindirConPago(int id, DateTime fechaFinEfectiva, decimal multa, string concepto, int usuarioFinalizadorId)
        {
            const string sqlPago = @"
                INSERT INTO Pagos (reserva_id, concepto, fecha_pago, importe, usuario_creador_id, anulado)
                VALUES (@reservaId, @concepto, NOW(), @importe, @usuarioFinalizadorId, 0);";

            const string sqlReserva = @"
                UPDATE Reservas
                SET fecha_fin_efectiva = @fechaFinEfectiva,
                    multa = @multa,
                    usuario_finalizador_id = @usuarioFinalizadorId
                WHERE id = @id";

            using var conexion = GetConnection();
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            using var cmdPago = new MySqlCommand(sqlPago, conexion, transaccion);
            cmdPago.Parameters.AddWithValue("@reservaId", id);
            cmdPago.Parameters.AddWithValue("@concepto", concepto.Trim());
            cmdPago.Parameters.AddWithValue("@importe", multa);
            cmdPago.Parameters.AddWithValue("@usuarioFinalizadorId", usuarioFinalizadorId);
            cmdPago.ExecuteNonQuery();

            using var cmdReserva = new MySqlCommand(sqlReserva, conexion, transaccion);
            cmdReserva.Parameters.AddWithValue("@fechaFinEfectiva", fechaFinEfectiva.Date);
            cmdReserva.Parameters.AddWithValue("@multa", multa);
            cmdReserva.Parameters.AddWithValue("@usuarioFinalizadorId", usuarioFinalizadorId);
            cmdReserva.Parameters.AddWithValue("@id", id);
            int filas = cmdReserva.ExecuteNonQuery();

            transaccion.Commit();
            return filas;
        }

        public int BajaLogica(int id)
        {
            const string sql = "UPDATE Reservas SET estado = 0 WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        private static Reserva Mapear(MySqlDataReader reader)
        {
            var reserva = new Reserva
            {
                Id = reader.GetInt32("id"),
                InmuebleId = reader.GetInt32("inmueble_id"),
                InquilinoId = reader.GetInt32("inquilino_id"),
                FechaInicio = reader.GetDateTime("fecha_inicio"),
                FechaFin = reader.GetDateTime("fecha_fin"),
                MontoDiario = reader.GetDecimal("monto_diario"),
                FechaFinEfectiva = reader["fecha_fin_efectiva"] as DateTime?,
                Multa = reader["multa"] as decimal?,
                UsuarioCreadorId = reader.GetInt32("usuario_creador_id"),
                UsuarioFinalizadorId = reader["usuario_finalizador_id"] as int?,
                Estado = reader.GetBoolean("estado"),
                Inmueble = new Inmueble
                {
                    Id = reader.GetInt32("inmueble_id"),
                    Direccion = reader.GetString("inmueble_direccion")
                },
                Inquilino = new Inquilino
                {
                    Id = reader.GetInt32("inquilino_id"),
                    Nombre = reader.GetString("inquilino_nombre"),
                    Apellido = reader.GetString("inquilino_apellido")
                },
                UsuarioCreador = new Usuario
                {
                    Id = reader.GetInt32("usuario_creador_id"),
                    Nombre = reader.GetString("creador_nombre"),
                    Apellido = reader.GetString("creador_apellido")
                }
            };

            if (reserva.UsuarioFinalizadorId.HasValue)
            {
                reserva.UsuarioFinalizador = new Usuario
                {
                    Id = reserva.UsuarioFinalizadorId.Value,
                    Nombre = reader.GetString("finalizador_nombre"),
                    Apellido = reader.GetString("finalizador_apellido")
                };
            }

            return reserva;
        }
    }
}
