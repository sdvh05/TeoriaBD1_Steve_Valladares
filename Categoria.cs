using System;
namespace TBD1_Proyecto.Models
{
    public class Categoria
    {
        public string IdCategoria { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }  // ingreso / gasto / ahorro
        public string NombreIconUi { get; set; }
        public string ColorHex { get; set; }
        public int Orden { get; set; }

        // Propiedad calculada — útil para combos
        public string NombreConTipo => $"[{Tipo.ToUpper()}] {Nombre}";
    }
}