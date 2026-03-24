using System;

namespace TBD1_App.Models
{
    public class ObligacionFija
    {
        public string IdObligacion     { get; set; }
        public string IdUser           { get; set; }
        public string IdSubcategoria   { get; set; }
        public string Nombre           { get; set; }
        public string Descripcion      { get; set; }
        public decimal Monto           { get; set; }
        public int Dia                 { get; set; }
        public int Vigente             { get; set; }
        public DateTime FechaInicio    { get; set; }
        public DateTime? FechaFin      { get; set; }

        
        public string NombreSubcategoria { get; set; }
        public string NombreCategoria    { get; set; }
        public int    DiasVencimiento    { get; set; }

        
        public bool EsUrgente => DiasVencimiento <= 3;
    }
}
