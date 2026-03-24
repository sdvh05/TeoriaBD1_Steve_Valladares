using System;

namespace TBD1_App.Models
{
    public class PresupuestoDetalle
    {
        public string  IdDetalle          { get; set; }
        public string  IdPresupuesto      { get; set; }
        public string  IdSubcategoria     { get; set; }
        public decimal Monto              { get; set; }
        public string  Justificacion      { get; set; }

        
        public string NombreSubcategoria  { get; set; }
        public string NombreCategoria     { get; set; }
        public string Tipo                { get; set; }
    }
}
