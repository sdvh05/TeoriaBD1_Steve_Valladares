using System;
namespace TBD1_Proyecto.Models

{
    public class ObligacionFija
    {
        public string IdObligacion { get; set; }
        public string IdUser { get; set; }
        public string IdSubcategoria { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public int Dia { get; set; }   // día del mes que vence (1-31)
        public int Vigente { get; set; }   // 1 = activa, 0 = inactiva
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }   // nullable — null si es indefinida

        // Navegación — se llena desde el DAO con JOIN
        public string NombreSubcategoria { get; set; }

        // Propiedad calculada — días restantes hasta el próximo vencimiento
        public int DiasHastaVencimiento
        {
            get
            {
                var hoy = DateTime.Today;
                var vence = new DateTime(hoy.Year, hoy.Month, Dia);
                if (vence < hoy)
                    vence = vence.AddMonths(1);
                return (vence - hoy).Days;
            }
        }

        // Propiedad calculada — alerta si vence en 3 días o menos
        public bool EsUrgente => DiasHastaVencimiento <= 3;
    }
}
