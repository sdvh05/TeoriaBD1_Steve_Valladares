namespace TBD1_App.Models
{
    public class Subcategoria
    {
        public string IdSubcategoria  { get; set; }
        public string IdCategoria     { get; set; }
        public string Nombre          { get; set; }
        public string Descripcion     { get; set; }
        public int    Activa          { get; set; }
        public int    SubPorDefecto   { get; set; }

       
        public string NombreCategoria { get; set; }
        public string TipoCategoria   { get; set; }   // ingreso / gasto / ahorro

        public override string ToString() => Nombre;
    }
}
