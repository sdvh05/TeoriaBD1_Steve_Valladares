using System;
namespace TBD1_Proyecto.Models

{
    public class Presupuesto
    {
        public string IdPresupuesto { get; set; }
        public string IdUser { get; set; }
        public string NombreDescriptivo { get; set; }
        public int AnioInicio { get; set; }
        public int MesInicio { get; set; }
        public int AnioFin { get; set; }
        public int MesFin { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal TotalAhorro { get; set; }
        public DateTime FechaHoraCreacion { get; set; }
        public string EstadoPresupuesto { get; set; }  // activo / cerrado / borrador

        // Propiedad calculada — útil para mostrar en combos
        public string PeriodoDescriptivo =>
            $"{NombreDescriptivo} ({MesInicio}/{AnioInicio} - {MesFin}/{AnioFin})";
    }
}

