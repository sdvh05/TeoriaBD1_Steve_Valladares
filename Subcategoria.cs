using System;
namespace TBD1_Proyecto.Models

{
    public class Subcategoria
    {
        public string IdSubcategoria { get; set; }
        public string IdCategoria { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Activa { get; set; }        // 1 = activa, 0 = inactiva
        public int SubPorDefecto { get; set; }        // 1 = es la "General" creada por trigger

        // Navegación — se llena desde el DAO con JOIN, no viene de la tabla
        public string NombreCategoria { get; set; }
        public string TipoCategoria { get; set; }        // ingreso / gasto / ahorro

        // Propiedad calculada — útil para combos de transacciones
        public string NombreCompleto => $"{NombreCategoria} > {Nombre}";
    }
}
