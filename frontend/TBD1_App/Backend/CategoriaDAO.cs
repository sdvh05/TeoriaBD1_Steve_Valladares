using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Models;

namespace TBD1_App.Backend
{
    public class CategoriaDAO
    {
        // ─────────────────────────────────────────
        // Obtener todas las categorías
        // ─────────────────────────────────────────
        public List<Categoria> ObtenerTodas()
        {
            var lista = new List<Categoria>();
            var dt    = OracleHelper.ExecuteQuery(
                "SELECT * FROM CATEGORIA ORDER BY ORDEN, NOMBRE");

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearCategoria(row));

            return lista;
        }

        // ─────────────────────────────────────────
        // Obtener categorías filtradas por tipo
        // tipo: 'ingreso' / 'gasto' / 'ahorro'
        // ─────────────────────────────────────────
        public List<Categoria> ObtenerPorTipo(string tipo)
        {
            var lista = new List<Categoria>();
            var dt    = OracleHelper.ExecuteQuery(
                "SELECT * FROM CATEGORIA WHERE TIPO = :tipo ORDER BY ORDEN, NOMBRE",
                new OracleParameter("tipo", tipo));

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearCategoria(row));

            return lista;
        }

        // ─────────────────────────────────────────
        // Insertar categoría
        // Oracle genera el ID automaticamente (OUT)
        // TRG_SUBCAT_DEFECTO crea "General" automaticamente
        // ─────────────────────────────────────────
        public string Insertar(Categoria c)
        {
            var pIdGenerado = new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
            {
                Direction = ParameterDirection.Output
            };

            OracleHelper.ExecuteProcedureWithOutput("SP_INSERTAR_CATEGORIA",
                new OracleParameter("P_NOMBRE",         c.Nombre),
                new OracleParameter("P_DESCRIPCION",    (object)c.Descripcion    ?? DBNull.Value),
                new OracleParameter("P_TIPO",           c.Tipo),
                new OracleParameter("P_NOMBRE_ICON_UI", (object)c.NombreIconUi   ?? DBNull.Value),
                new OracleParameter("P_COLOR_HEX",      (object)c.ColorHex       ?? DBNull.Value),
                new OracleParameter("P_ORDEN",          c.Orden),
                pIdGenerado
            );

            return pIdGenerado.Value?.ToString();
        }

        // ─────────────────────────────────────────
        // Actualizar categoría
        // ─────────────────────────────────────────
        public void Actualizar(Categoria c)
        {
            OracleHelper.ExecuteProcedure("SP_ACTUALIZAR_CATEGORIA",
                new OracleParameter("P_ID_CATEGORIA", c.IdCategoria),
                new OracleParameter("P_NOMBRE",       c.Nombre),
                new OracleParameter("P_DESCRIPCION",  (object)c.Descripcion ?? DBNull.Value),
                new OracleParameter("P_COLOR_HEX",    (object)c.ColorHex    ?? DBNull.Value),
                new OracleParameter("P_ORDEN",        c.Orden)
            );
        }

        // ─────────────────────────────────────────
        // Eliminar categoría
        // ─────────────────────────────────────────
        public void Eliminar(string idCategoria)
        {
            OracleHelper.ExecuteProcedure("SP_ELIMINAR_CATEGORIA",
                new OracleParameter("P_ID_CATEGORIA", idCategoria));
        }

        // ─────────────────────────────────────────
        // Obtener subcategorías activas de una categoría
        // ─────────────────────────────────────────
        public List<Subcategoria> ObtenerSubcategorias(string idCategoria)
        {
            var lista = new List<Subcategoria>();
            var dt    = OracleHelper.ExecuteQuery(@"
                SELECT  S.ID_SUBCATEGORIA, S.ID_CATEGORIA,
                        S.NOMBRE, S.DESCRIPCION,
                        S.ACTIVA, S.SUB_POR_DEFECTO,
                        C.NOMBRE AS NOMBRE_CATEGORIA,
                        C.TIPO   AS TIPO_CATEGORIA
                FROM    SUBCATEGORIA S
                JOIN    CATEGORIA    C ON S.ID_CATEGORIA = C.ID_CATEGORIA
                WHERE   S.ID_CATEGORIA = :id
                AND     S.ACTIVA = 1
                ORDER   BY S.SUB_POR_DEFECTO DESC, S.NOMBRE",
                new OracleParameter("id", idCategoria));

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearSubcategoria(row));

            return lista;
        }

        // ─────────────────────────────────────────
        // Obtener TODAS las subcategorías activas
        // Útil para combos en Transaccion
        // ─────────────────────────────────────────
        public List<Subcategoria> ObtenerTodasSubcategorias()
        {
            var lista = new List<Subcategoria>();
            var dt    = OracleHelper.ExecuteQuery(@"
                SELECT  S.ID_SUBCATEGORIA, S.ID_CATEGORIA,
                        S.NOMBRE, S.DESCRIPCION,
                        S.ACTIVA, S.SUB_POR_DEFECTO,
                        C.NOMBRE AS NOMBRE_CATEGORIA,
                        C.TIPO   AS TIPO_CATEGORIA
                FROM    SUBCATEGORIA S
                JOIN    CATEGORIA    C ON S.ID_CATEGORIA = C.ID_CATEGORIA
                WHERE   S.ACTIVA = 1
                ORDER   BY C.TIPO, C.NOMBRE, S.NOMBRE");

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearSubcategoria(row));

            return lista;
        }

        // ─────────────────────────────────────────
        // Insertar subcategoría
        // Oracle genera el ID automaticamente (OUT)
        // ─────────────────────────────────────────
        public string InsertarSubcategoria(Subcategoria s)
        {
            var pIdGenerado = new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
            {
                Direction = ParameterDirection.Output
            };

            OracleHelper.ExecuteProcedureWithOutput("SP_INSERTAR_SUBCATEGORIA",
                new OracleParameter("P_ID_CATEGORIA",  s.IdCategoria),
                new OracleParameter("P_NOMBRE",        s.Nombre),
                new OracleParameter("P_DESCRIPCION",   (object)s.Descripcion ?? DBNull.Value),
                pIdGenerado
            );

            return pIdGenerado.Value?.ToString();
        }

        // ─────────────────────────────────────────
        // Actualizar subcategoría
        // ─────────────────────────────────────────
        public void ActualizarSubcategoria(Subcategoria s)
        {
            OracleHelper.ExecuteProcedure("SP_ACTUALIZAR_SUBCATEGORIA",
                new OracleParameter("P_ID_SUBCATEGORIA", s.IdSubcategoria),
                new OracleParameter("P_NOMBRE",          s.Nombre),
                new OracleParameter("P_DESCRIPCION",     (object)s.Descripcion ?? DBNull.Value)
            );
        }

        // ─────────────────────────────────────────
        // Desactivar subcategoría (soft delete)
        // ─────────────────────────────────────────
        public void EliminarSubcategoria(string idSubcategoria)
        {
            OracleHelper.ExecuteProcedure("SP_ELIMINAR_SUBCATEGORIA",
                new OracleParameter("P_ID_SUBCATEGORIA", idSubcategoria));
        }

        // ─────────────────────────────────────────
        // Mapeo DataRow → Categoria
        // ─────────────────────────────────────────
        private Categoria MapearCategoria(DataRow row)
        {
            return new Categoria
            {
                IdCategoria  = row["ID_CATEGORIA"].ToString(),
                Nombre       = row["NOMBRE"].ToString(),
                Descripcion  = row["DESCRIPCION"]   == DBNull.Value ? null : row["DESCRIPCION"].ToString(),
                Tipo         = row["TIPO"].ToString(),
                NombreIconUi = row["NOMBRE_ICON_UI"] == DBNull.Value ? null : row["NOMBRE_ICON_UI"].ToString(),
                ColorHex     = row["COLOR_HEX"]      == DBNull.Value ? null : row["COLOR_HEX"].ToString(),
                Orden        = Convert.ToInt32(row["ORDEN"])
            };
        }

        // ─────────────────────────────────────────
        // Mapeo DataRow → Subcategoria
        // ─────────────────────────────────────────
        private Subcategoria MapearSubcategoria(DataRow row)
        {
            return new Subcategoria
            {
                IdSubcategoria  = row["ID_SUBCATEGORIA"].ToString(),
                IdCategoria     = row["ID_CATEGORIA"].ToString(),
                Nombre          = row["NOMBRE"].ToString(),
                Descripcion     = row["DESCRIPCION"]      == DBNull.Value ? null : row["DESCRIPCION"].ToString(),
                Activa          = Convert.ToInt32(row["ACTIVA"]),
                SubPorDefecto   = Convert.ToInt32(row["SUB_POR_DEFECTO"]),
                NombreCategoria = row["NOMBRE_CATEGORIA"].ToString(),
                TipoCategoria   = row["TIPO_CATEGORIA"].ToString()
            };
        }
    }
}
