using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public class RepositorioInmueble : RepositorioBase, IRepositorioInmueble
    {
        public RepositorioInmueble(IConfiguration configuration) : base(configuration)
        {
        }

        private const string SelectBase = @"
            SELECT i.id, i.direccion, i.cupo, i.tipo_id, i.coordenadas, i.precio_dia,
                   i.porcentaje_reserva, i.portada, i.disponible, i.propietario_id, i.estado,
                   t.descripcion AS tipo_descripcion,
                   p.nombre AS propietario_nombre, p.apellido AS propietario_apellido
            FROM Inmuebles i
            INNER JOIN Tipos_Inmueble t ON i.tipo_id = t.id
            INNER JOIN Propietarios p ON i.propietario_id = p.id";

        public List<Inmueble> ObtenerTodos()
        {
            var lista = new List<Inmueble>();
            string sql = SelectBase + " WHERE i.estado = 1 ORDER BY i.id DESC";

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

        public PaginacionResultado<Inmueble> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null, bool? disponible = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanio < 1) tamanio = 10;
            int offset = (pagina - 1) * tamanio;

            var resultado = new PaginacionResultado<Inmueble>
            {
                PaginaActual = pagina,
                ElementosPorPagina = tamanio,
                CriterioBusqueda = busqueda
            };

            bool tieneFiltro = !string.IsNullOrWhiteSpace(busqueda);
            string filtroSql = tieneFiltro
                ? " AND (i.direccion LIKE @filtro OR t.descripcion LIKE @filtro OR p.nombre LIKE @filtro OR p.apellido LIKE @filtro)"
                : "";

            bool tieneDisponible = disponible.HasValue;
            string filtroDisponibleSql = tieneDisponible ? " AND i.disponible = @disponible" : "";

            string sqlCount = $@"
                SELECT COUNT(*)
                FROM Inmuebles i
                INNER JOIN Tipos_Inmueble t ON i.tipo_id = t.id
                INNER JOIN Propietarios p ON i.propietario_id = p.id
                WHERE i.estado = 1 {filtroSql} {filtroDisponibleSql}";

            string sqlData = SelectBase + $" WHERE i.estado = 1 {filtroSql} {filtroDisponibleSql} ORDER BY i.id DESC LIMIT @limit OFFSET @offset";

            using var conexion = GetConnection();
            conexion.Open();

            using (var cmdCount = new MySqlCommand(sqlCount, conexion))
            {
                if (tieneFiltro)
                {
                    cmdCount.Parameters.AddWithValue("@filtro", $"%{busqueda}%");
                }
                if (tieneDisponible)
                {
                    cmdCount.Parameters.AddWithValue("@disponible", disponible!.Value ? 1 : 0);
                }
                resultado.TotalElementos = Convert.ToInt32(cmdCount.ExecuteScalar());
            }

            using (var cmdData = new MySqlCommand(sqlData, conexion))
            {
                if (tieneFiltro)
                {
                    cmdData.Parameters.AddWithValue("@filtro", $"%{busqueda}%");
                }
                if (tieneDisponible)
                {
                    cmdData.Parameters.AddWithValue("@disponible", disponible!.Value ? 1 : 0);
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

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? inmueble = null;
            string sql = SelectBase + " WHERE i.id = @id";

            using (var conexion = GetConnection())
            {
                conexion.Open();

                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    using var reader = comando.ExecuteReader();
                    if (reader.Read())
                    {
                        inmueble = Mapear(reader);
                    }
                }

                if (inmueble != null)
                {
                    inmueble.Fotos = ObtenerFotos(id, conexion);
                }
            }

            return inmueble;
        }

        private static List<FotoInmueble> ObtenerFotos(int inmuebleId, MySqlConnection conexion)
        {
            var fotos = new List<FotoInmueble>();
            const string sql = "SELECT id, inmueble_id, url_foto FROM Fotos_Inmueble WHERE inmueble_id = @id ORDER BY id";

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", inmuebleId);

            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                fotos.Add(new FotoInmueble
                {
                    Id = reader.GetInt32("id"),
                    InmuebleId = reader.GetInt32("inmueble_id"),
                    UrlFoto = reader.GetString("url_foto")
                });
            }

            return fotos;
        }

        public List<Inmueble> Buscar(string termino, int max = 10)
        {
            var lista = new List<Inmueble>();
            string sql = SelectBase + @"
                WHERE i.estado = 1 AND (i.direccion LIKE @term OR t.descripcion LIKE @term)
                ORDER BY i.direccion
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

        public int Alta(Inmueble i)
        {
            const string sql = @"
                INSERT INTO Inmuebles (direccion, cupo, tipo_id, coordenadas, precio_dia, porcentaje_reserva, portada, disponible, propietario_id, estado)
                VALUES (@direccion, @cupo, @tipoId, @coordenadas, @precioDia, @porcentajeReserva, @portada, @disponible, @propietarioId, 1);
                SELECT LAST_INSERT_ID();";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@direccion", i.Direccion.Trim());
            comando.Parameters.AddWithValue("@cupo", i.Cupo);
            comando.Parameters.AddWithValue("@tipoId", i.TipoId);
            comando.Parameters.AddWithValue("@coordenadas", (object?)i.Coordenadas ?? DBNull.Value);
            comando.Parameters.AddWithValue("@precioDia", i.PrecioDia);
            comando.Parameters.AddWithValue("@porcentajeReserva", i.PorcentajeReserva);
            comando.Parameters.AddWithValue("@portada", (object?)i.Portada ?? DBNull.Value);
            comando.Parameters.AddWithValue("@disponible", i.Disponible);
            comando.Parameters.AddWithValue("@propietarioId", i.PropietarioId);
            conexion.Open();

            i.Id = Convert.ToInt32(comando.ExecuteScalar());
            return i.Id;
        }

        public int Modificacion(Inmueble i)
        {
            const string sql = @"
                UPDATE Inmuebles
                SET direccion = @direccion,
                    cupo = @cupo,
                    tipo_id = @tipoId,
                    coordenadas = @coordenadas,
                    precio_dia = @precioDia,
                    porcentaje_reserva = @porcentajeReserva,
                    portada = @portada,
                    disponible = @disponible,
                    propietario_id = @propietarioId
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@direccion", i.Direccion.Trim());
            comando.Parameters.AddWithValue("@cupo", i.Cupo);
            comando.Parameters.AddWithValue("@tipoId", i.TipoId);
            comando.Parameters.AddWithValue("@coordenadas", (object?)i.Coordenadas ?? DBNull.Value);
            comando.Parameters.AddWithValue("@precioDia", i.PrecioDia);
            comando.Parameters.AddWithValue("@porcentajeReserva", i.PorcentajeReserva);
            comando.Parameters.AddWithValue("@portada", (object?)i.Portada ?? DBNull.Value);
            comando.Parameters.AddWithValue("@disponible", i.Disponible);
            comando.Parameters.AddWithValue("@propietarioId", i.PropietarioId);
            comando.Parameters.AddWithValue("@id", i.Id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int BajaLogica(int id)
        {
            const string sql = "UPDATE Inmuebles SET estado = 0 WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int AgregarFoto(int inmuebleId, string urlFoto)
        {
            const string sql = "INSERT INTO Fotos_Inmueble (inmueble_id, url_foto) VALUES (@inmuebleId, @urlFoto)";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@inmuebleId", inmuebleId);
            comando.Parameters.AddWithValue("@urlFoto", urlFoto);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int EliminarFoto(int fotoId)
        {
            const string sql = "DELETE FROM Fotos_Inmueble WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", fotoId);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        private static Inmueble Mapear(MySqlDataReader reader)
        {
            return new Inmueble
            {
                Id = reader.GetInt32("id"),
                Direccion = reader.GetString("direccion"),
                Cupo = reader.GetInt32("cupo"),
                TipoId = reader.GetInt32("tipo_id"),
                Coordenadas = reader["coordenadas"] as string,
                PrecioDia = reader.GetDecimal("precio_dia"),
                PorcentajeReserva = reader.GetInt32("porcentaje_reserva"),
                Portada = reader["portada"] as string,
                Disponible = reader.GetBoolean("disponible"),
                PropietarioId = reader.GetInt32("propietario_id"),
                Estado = reader.GetBoolean("estado"),
                Tipo = new TipoInmueble
                {
                    Id = reader.GetInt32("tipo_id"),
                    Descripcion = reader.GetString("tipo_descripcion")
                },
                Propietario = new Propietario
                {
                    Id = reader.GetInt32("propietario_id"),
                    Nombre = reader.GetString("propietario_nombre"),
                    Apellido = reader.GetString("propietario_apellido")
                }
            };
        }
    }
}
