using System;

namespace TBD1_App.Models
{
    public class Usuario
    {
        public string  IdUser           { get; set; }
        public string  PrimerNombre     { get; set; }
        public string  SegundoNombre    { get; set; }
        public string  PrimerApellido   { get; set; }
        public string  SegundoApellido  { get; set; }
        public string  Correo           { get; set; }
        public string  Password         { get; set; }
        public DateTime FechaRegistro   { get; set; }
        public decimal SalarioBase      { get; set; }
        public int     Estado           { get; set; }   // 1 = activo, 0 = inactivo

       
        public string NombreCompleto =>
            $"{PrimerNombre} {SegundoNombre} {PrimerApellido} {SegundoApellido}".Trim();
    }
}
