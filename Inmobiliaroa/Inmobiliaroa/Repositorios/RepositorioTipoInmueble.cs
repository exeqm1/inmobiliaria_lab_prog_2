using Inmobiliaria.Models;
using Inmobiliaria.Repositorios.Interfaces;
using MySql.Data.MySqlClient;

namespace Inmobiliaria.Repositorios
{
    public class RepositorioTipoInmueble : RepositorioBase, IRepositorioTipoInmueble
    {
        public RepositorioTipoInmueble(IConfiguration configuration) : base(configuration)
        {
        }

        public List<TipoInmueble> ObtenerTodos()
        {
            var lista = new List<TipoInmueble>();
            const string sql = @"
                SELECT id, descripcion, estado
                FROM Tipos_Inmueble
                WHERE estado = 1
                ORDER BY descripcion";

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

        public PaginacionResultado<TipoInmueble> ObtenerPaginado(int pagina = 1, int tamanio = 10, string? busqueda = null)
        {
            if (pagina < 1) pagina = 1;
            if (tamanio < 1) tamanio = 10;
            int offset = (pagina - 1) * tamanio;

            var resultado = new PaginacionResultado<TipoInmueble>
            {
                PaginaActual = pagina,
                ElementosPorPagina = tamanio,
                CriterioBusqueda = busqueda
            };

            bool tieneFiltro = !string.IsNullOrWhiteSpace(busqueda);
            string filtroSql = tieneFiltro ? " AND descripcion LIKE @filtro" : "";

            string sqlCount = $"SELECT COUNT(*) FROM Tipos_Inmueble WHERE estado = 1 {filtroSql}";
            string sqlData = $@"
                SELECT id, descripcion, estado
                FROM Tipos_Inmueble
                WHERE estado = 1 {filtroSql}
                ORDER BY id DESC
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

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? tipo = null;
            const string sql = @"
                SELECT id, descripcion, estado
                FROM Tipos_Inmueble
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            using var reader = comando.ExecuteReader();
            if (reader.Read())
            {
                tipo = Mapear(reader);
            }

            return tipo;
        }

        public List<TipoInmueble> Buscar(string termino, int max = 10)
        {
            var lista = new List<TipoInmueble>();
            const string sql = @"
                SELECT id, descripcion, estado
                FROM Tipos_Inmueble
                WHERE estado = 1 AND descripcion LIKE @term
                ORDER BY descripcion
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

        public int Alta(TipoInmueble tipo)
        {
            const string sql = @"
                INSERT INTO Tipos_Inmueble (descripcion, estado)
                VALUES (@descripcion, 1);
                SELECT LAST_INSERT_ID();";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@descripcion", tipo.Descripcion.Trim());
            conexion.Open();

            tipo.Id = Convert.ToInt32(comando.ExecuteScalar());
            return tipo.Id;
        }

        public int Modificacion(TipoInmueble tipo)
        {
            const string sql = @"
                UPDATE Tipos_Inmueble
                SET descripcion = @descripcion
                WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@descripcion", tipo.Descripcion.Trim());
            comando.Parameters.AddWithValue("@id", tipo.Id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        public int BajaLogica(int id)
        {
            const string sql = "UPDATE Tipos_Inmueble SET estado = 0 WHERE id = @id";

            using var conexion = GetConnection();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@id", id);
            conexion.Open();

            return comando.ExecuteNonQuery();
        }

        private static TipoInmueble Mapear(MySqlDataReader reader)
        {
            return new TipoInmueble
            {
                Id = reader.GetInt32("id"),
                Descripcion = reader.GetString("descripcion"),
                Estado = reader.GetBoolean("estado")
            };
        }
    }
}
