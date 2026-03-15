using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Models;

namespace TBD1_App.Backend
{
    public class PresupuestoDAO
    {

        public List<Presupuesto> ObtenerPorUsuario(string idUser)
        {
            var lista = new List<Presupuesto>();
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT * FROM PRESUPUESTO
                WHERE  ID_USER = :id
                ORDER  BY ANIO_INICIO DESC, MES_INICIO DESC",
                new OracleParameter("id", idUser));

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));

            return lista;
        }

        public List<Presupuesto> ObtenerActivos(string idUser)
        {
            var lista = new List<Presupuesto>();
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT * FROM PRESUPUESTO
                WHERE  ID_USER            = :id
                AND    ESTADO_PRESUPUESTO = 'activo'
                ORDER  BY ANIO_INICIO DESC, MES_INICIO DESC",
                new OracleParameter("id", idUser));

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));

            return lista;
        }

    
        public Presupuesto ObtenerPorId(string idPresupuesto)
        {
            var dt = OracleHelper.ExecuteQuery(
                "SELECT * FROM PRESUPUESTO WHERE ID_PRESUPUESTO = :id",
                new OracleParameter("id", idPresupuesto));

            if (dt.Rows.Count == 0) return null;
            return MapearFila(dt.Rows[0]);
        }

        public string Insertar(Presupuesto p)
        {
            var pIdGenerado = new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
            {
                Direction = ParameterDirection.Output
            };

            OracleHelper.ExecuteProcedureWithOutput("SP_INSERTAR_PRESUPUESTO",
                new OracleParameter("P_ID_USER", p.IdUser),
                new OracleParameter("P_NOMBRE_DESCRIPTIVO", p.NombreDescriptivo),
                new OracleParameter("P_ANIO_INICIO", p.AnioInicio),
                new OracleParameter("P_MES_INICIO", p.MesInicio),
                new OracleParameter("P_ANIO_FIN", p.AnioFin),
                new OracleParameter("P_MES_FIN", p.MesFin),
                pIdGenerado
            );

            return pIdGenerado.Value?.ToString();
        }


        public void Actualizar(Presupuesto p)
        {
            OracleHelper.ExecuteProcedure("SP_ACTUALIZAR_PRESUPUESTO",
                new OracleParameter("P_ID_PRESUPUESTO", p.IdPresupuesto),
                new OracleParameter("P_NOMBRE_DESCRIPTIVO", p.NombreDescriptivo)
            );
        }

 
        public string CrearCompleto(Presupuesto p, string detallesJson)
        {
            var pIdGenerado = new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
            {
                Direction = ParameterDirection.Output
            };

            var pJson = new OracleParameter("P_DETALLES_JSON", OracleDbType.Clob)
            {
                Value = detallesJson
            };

            OracleHelper.ExecuteProcedureWithOutput("SP_CREAR_PRESUPUESTO_COMPLETO",
                new OracleParameter("P_ID_USER", p.IdUser),
                new OracleParameter("P_NOMBRE_DESCRIPTIVO", p.NombreDescriptivo),
                new OracleParameter("P_ANIO_INICIO", p.AnioInicio),
                new OracleParameter("P_MES_INICIO", p.MesInicio),
                new OracleParameter("P_ANIO_FIN", p.AnioFin),
                new OracleParameter("P_MES_FIN", p.MesFin),
                pJson,
                pIdGenerado
            );

            return pIdGenerado.Value?.ToString();
        }

   
        public void Cerrar(string idPresupuesto)
        {
            OracleHelper.ExecuteProcedure("SP_CERRAR_PRESUPUESTO",
                new OracleParameter("P_ID_PRESUPUESTO", idPresupuesto));
        }


        public List<PresupuestoDetalle> ObtenerDetalles(string idPresupuesto)
        {
            var lista = new List<PresupuestoDetalle>();
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT  D.ID_DETALLE, D.ID_PRESUPUESTO, D.ID_SUBCATEGORIA,
                        D.MONTO, D.JUSTIFICACION,
                        S.NOMBRE AS NOMBRE_SUBCATEGORIA,
                        C.NOMBRE AS NOMBRE_CATEGORIA,
                        C.TIPO
                FROM    PRESUPUESTO_DETALLE D
                JOIN    SUBCATEGORIA        S ON D.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN    CATEGORIA           C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE   D.ID_PRESUPUESTO = :id
                ORDER   BY C.TIPO, C.NOMBRE, S.NOMBRE",
                new OracleParameter("id", idPresupuesto));

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearDetalle(row));

            return lista;
        }


        public string InsertarDetalle(PresupuestoDetalle d)
        {
            var pIdGenerado = new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
            {
                Direction = ParameterDirection.Output
            };

            OracleHelper.ExecuteProcedureWithOutput("SP_INSERTAR_DETALLE",
                new OracleParameter("P_ID_PRESUPUESTO", d.IdPresupuesto),
                new OracleParameter("P_ID_SUBCATEGORIA", d.IdSubcategoria),
                new OracleParameter("P_MONTO", d.Monto),
                new OracleParameter("P_JUSTIFICACION", (object)d.Justificacion ?? DBNull.Value),
                pIdGenerado
            );

            return pIdGenerado.Value?.ToString();
        }


        public void ActualizarDetalle(PresupuestoDetalle d)
        {
            OracleHelper.ExecuteProcedure("SP_ACTUALIZAR_DETALLE",
                new OracleParameter("P_ID_DETALLE", d.IdDetalle),
                new OracleParameter("P_MONTO", d.Monto),
                new OracleParameter("P_JUSTIFICACION", (object)d.Justificacion ?? DBNull.Value)
            );
        }


        public void EliminarDetalle(string idDetalle)
        {
            OracleHelper.ExecuteProcedure("SP_ELIMINAR_DETALLE",
                new OracleParameter("P_ID_DETALLE", idDetalle));
        }

   
        private Presupuesto MapearFila(DataRow row)
        {
            return new Presupuesto
            {
                IdPresupuesto = row["ID_PRESUPUESTO"].ToString(),
                IdUser = row["ID_USER"].ToString(),
                NombreDescriptivo = row["NOMBRE_DESCRIPTIVO"].ToString(),
                AnioInicio = Convert.ToInt32(row["ANIO_INICIO"]),
                MesInicio = Convert.ToInt32(row["MES_INICIO"]),
                AnioFin = Convert.ToInt32(row["ANIO_FIN"]),
                MesFin = Convert.ToInt32(row["MES_FIN"]),
                TotalIngresos = Convert.ToDecimal(row["TOTAL_INGRESOS"]),
                TotalGastos = Convert.ToDecimal(row["TOTAL_GASTOS"]),
                TotalAhorro = Convert.ToDecimal(row["TOTAL_AHORRO"]),
                FechaHoraCreacion = Convert.ToDateTime(row["FECHA_HORA_CREACION"]),
                EstadoPresupuesto = row["ESTADO_PRESUPUESTO"].ToString()
            };
        }

    
        private PresupuestoDetalle MapearDetalle(DataRow row)
        {
            return new PresupuestoDetalle
            {
                IdDetalle = row["ID_DETALLE"].ToString(),
                IdPresupuesto = row["ID_PRESUPUESTO"].ToString(),
                IdSubcategoria = row["ID_SUBCATEGORIA"].ToString(),
                Monto = Convert.ToDecimal(row["MONTO"]),
                Justificacion = row["JUSTIFICACION"] == DBNull.Value ? null : row["JUSTIFICACION"].ToString(),
                NombreSubcategoria = row["NOMBRE_SUBCATEGORIA"].ToString(),
                NombreCategoria = row["NOMBRE_CATEGORIA"].ToString(),
                Tipo = row["TIPO"].ToString()
            };
        }
    }
}
