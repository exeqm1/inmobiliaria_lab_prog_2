using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using Inmobiliaria.ViewModels;
using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public class RepositorioReporte : RepositorioBase, IRepositorioReporte
    {
        public RepositorioReporte(IConfiguration configuration) : base(configuration)
        {
        }

        public DashboardViewModel ObtenerDashboard()
        {
            using var conexion = GetConnection();
            conexion.Open();

            var model = new DashboardViewModel
            {
                PropietariosActivos = Contar(conexion, "SELECT COUNT(*) FROM Propietarios WHERE estado = 1"),
                InquilinosActivos = Contar(conexion, "SELECT COUNT(*) FROM Inquilinos WHERE estado = 1"),
                InmueblesActivos = Contar(conexion, "SELECT COUNT(*) FROM Inmuebles WHERE estado = 1"),
                InmueblesDisponibles = Contar(conexion, "SELECT COUNT(*) FROM Inmuebles WHERE estado = 1 AND disponible = 1"),
                ReservasVigentes = Contar(conexion, "SELECT COUNT(*) FROM Reservas WHERE estado = 1 AND fecha_fin_efectiva IS NULL"),
                ReservasFinalizadas = Contar(conexion, "SELECT COUNT(*) FROM Reservas WHERE estado = 1 AND fecha_fin_efectiva IS NOT NULL"),
                ReservasCanceladas = Contar(conexion, "SELECT COUNT(*) FROM Reservas WHERE estado = 0"),
                PagosRegistrados = Contar(conexion, "SELECT COUNT(*) FROM Pagos WHERE anulado = 0"),
                IngresosTotales = Convert.ToDecimal(new MySqlCommand("SELECT COALESCE(SUM(importe), 0) FROM Pagos WHERE anulado = 0", conexion).ExecuteScalar())
            };

            model.ProximasReservas = ObtenerProximasReservas(conexion);
            model.IngresosPorTipo = ObtenerIngresosPorTipo(conexion);

            return model;
        }

        public ReporteReservasViewModel ObtenerReporteReservas(DateTime desde, DateTime hasta)
        {
            var model = new ReporteReservasViewModel
            {
                Desde = desde.Date,
                Hasta = hasta.Date
            };

            const string sql = @"
                SELECT r.id, r.fecha_inicio, r.fecha_fin, r.fecha_fin_efectiva, r.monto_diario, r.multa,
                       i.direccion AS inmueble_direccion,
                       inq.nombre AS inquilino_nombre, inq.apellido AS inquilino_apellido,
                       (SELECT COALESCE(SUM(p.importe), 0)
                          FROM Pagos p
                         WHERE p.reserva_id = r.id AND p.anulado = 0) AS total_pagado
                FROM Reservas r
                INNER JOIN Inmuebles i ON r.inmueble_id = i.id
                INNER JOIN Inquilinos inq ON r.inquilino_id = inq.id
                WHERE r.estado = 1
                  AND r.fecha_inicio >= @desde
                  AND r.fecha_inicio <= @hasta
                ORDER BY r.fecha_inicio DESC, r.id DESC";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@desde", desde.Date);
            comando.Parameters.AddWithValue("@hasta", hasta.Date);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                model.Reservas.Add(new ReporteReservaItem
                {
                    Id = reader.GetInt32("id"),
                    Inmueble = reader.GetString("inmueble_direccion"),
                    Inquilino = $"{reader.GetString("inquilino_nombre")} {reader.GetString("inquilino_apellido")}",
                    FechaInicio = reader.GetDateTime("fecha_inicio"),
                    FechaFin = reader.GetDateTime("fecha_fin"),
                    FechaFinEfectiva = reader["fecha_fin_efectiva"] as DateTime?,
                    MontoDiario = reader.GetDecimal("monto_diario"),
                    Multa = reader["multa"] as decimal?,
                    TotalPagado = Convert.ToDecimal(reader["total_pagado"])
                });
            }

            return model;
        }

        private static List<Reserva> ObtenerProximasReservas(MySqlConnection conexion)
        {
            var lista = new List<Reserva>();
            const string sql = @"
                SELECT r.id, r.inmueble_id, r.inquilino_id, r.fecha_inicio, r.fecha_fin, r.monto_diario,
                       r.fecha_fin_efectiva, r.multa, r.usuario_creador_id, r.usuario_finalizador_id, r.estado,
                       i.direccion AS inmueble_direccion,
                       inq.nombre AS inquilino_nombre, inq.apellido AS inquilino_apellido,
                       uc.nombre AS creador_nombre, uc.apellido AS creador_apellido
                FROM Reservas r
                INNER JOIN Inmuebles i ON r.inmueble_id = i.id
                INNER JOIN Inquilinos inq ON r.inquilino_id = inq.id
                INNER JOIN Usuarios uc ON r.usuario_creador_id = uc.id
                WHERE r.estado = 1 AND r.fecha_fin_efectiva IS NULL AND r.fecha_fin >= CURDATE()
                ORDER BY r.fecha_inicio ASC
                LIMIT 5";

            using var comando = new MySqlCommand(sql, conexion);
            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Reserva
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
                });
            }

            return lista;
        }

        private static List<IngresoPorTipoItem> ObtenerIngresosPorTipo(MySqlConnection conexion)
        {
            var lista = new List<IngresoPorTipoItem>();
            const string sql = @"
                SELECT t.descripcion AS tipo_descripcion,
                       COUNT(DISTINCT r.id) AS cantidad,
                       COALESCE(SUM(CASE WHEN p.anulado = 0 THEN p.importe ELSE 0 END), 0) AS cobrado
                FROM Tipos_Inmueble t
                LEFT JOIN Inmuebles i ON i.tipo_id = t.id AND i.estado = 1
                LEFT JOIN Reservas r ON r.inmueble_id = i.id AND r.estado = 1
                LEFT JOIN Pagos p ON p.reserva_id = r.id
                GROUP BY t.id, t.descripcion
                ORDER BY cobrado DESC, t.descripcion ASC";

            using var comando = new MySqlCommand(sql, conexion);
            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new IngresoPorTipoItem
                {
                    TipoDescripcion = reader.GetString("tipo_descripcion"),
                    CantidadReservas = Convert.ToInt32(reader["cantidad"]),
                    IngresosCobrados = Convert.ToDecimal(reader["cobrado"])
                });
            }

            return lista;
        }

        private static int Contar(MySqlConnection conexion, string sql)
        {
            using var comando = new MySqlCommand(sql, conexion);
            return Convert.ToInt32(comando.ExecuteScalar());
        }
    }
}
