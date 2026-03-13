using System;
namespace TBD1_Proyecto.Models
{
    public class Transaccion
    {
        public string IdTransaccion { get; set; }
        public string IdUser { get; set; }
        public string IdPresupuesto { get; set; }
        public string IdSubcategoria { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Tipo { get; set; }  // ingreso / gasto / ahorro
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string MetodoPago { get; set; }  // efectivo / tarjeta_debito / etc.
        public string NumFactura { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaHoraRegistro { get; set; }

        // Navegación — se llena desde el DAO con JOIN
        public string NombreSubcategoria { get; set; }
        public string NombreCategoria { get; set; }
    }
}
