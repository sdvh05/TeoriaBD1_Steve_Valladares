using System;

namespace TBD1_App.Models
{
    // ── Reporte 1: Balance mensual ─────────────────────────
    public class ReporteBalanceMes
    {
        public int     Anio          { get; set; }
        public int     Mes           { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos   { get; set; }
        public decimal TotalAhorros  { get; set; }
        public decimal BalanceFinal  { get; set; }
        public string  NombreMes     => new DateTime(Anio, Mes, 1).ToString("MMMM yyyy");
    }

    // ── Reporte 2: Gastos por categoría ───────────────────
    public class ReporteGastoCategoria
    {
        public string  IdCategoria      { get; set; }
        public string  NombreCategoria  { get; set; }
        public decimal TotalGastado     { get; set; }
        public int     NumTransacciones { get; set; }
        public decimal Porcentaje       { get; set; }
    }

    // ── Reporte 3: Cumplimiento presupuesto ───────────────
    public class ReporteCumplimiento
    {
        public string  NombreCategoria    { get; set; }
        public string  TipoCategoria      { get; set; }
        public string  NombreSubcategoria { get; set; }
        public decimal MontoPresupuestado { get; set; }
        public decimal MontoEjecutado     { get; set; }
        public decimal Diferencia         { get; set; }
        public decimal PorcentajeEjecucion{ get; set; }
        public string  Justificacion      { get; set; }

        // Verde < 80, Amarillo 80-100, Rojo > 100
        public string Semaforo =>
            PorcentajeEjecucion < 80   ? "Verde" :
            PorcentajeEjecucion <= 100 ? "Amarillo" : "Rojo";
    }

    // ── Reporte 4: Tendencia gastos ───────────────────────
    public class ReporteTendencia
    {
        public int     Anio            { get; set; }
        public int     Mes             { get; set; }
        public string  IdCategoria     { get; set; }
        public string  NombreCategoria { get; set; }
        public decimal TotalGastado    { get; set; }
        public string  Periodo         => new DateTime(Anio, Mes, 1).ToString("MMM yy");
    }

    // ── Reporte 5: Estado obligaciones ────────────────────
    public class ReporteObligacion
    {
        public string   IdObligacion      { get; set; }
        public string   NombreObligacion  { get; set; }
        public string   NombreCategoria   { get; set; }
        public string   NombreSubcategoria{ get; set; }
        public decimal  Monto             { get; set; }
        public int      DiaVencimiento    { get; set; }
        public int      DiasRestantes     { get; set; }
        public string   EstadoPago        { get; set; }
        public DateTime?FechaUltimoPago   { get; set; }
        public DateTime FechaInicio       { get; set; }
        public DateTime?FechaFin          { get; set; }
    }

    // ── Reporte 6: Progreso ahorro ────────────────────────
    public class ReporteProgresoAhorro
    {
        public string  IdSubcategoria        { get; set; }
        public string  NombreMeta            { get; set; }
        public string  NombreCategoria       { get; set; }
        public decimal MontoObjetivoMensual  { get; set; }
        public int     MesesVigencia         { get; set; }
        public decimal MontoObjetivoTotal    { get; set; }
        public decimal MontoAcumulado        { get; set; }
        public decimal PorcentajeCompletado  { get; set; }
        public int     DiasRestantes         { get; set; }
        public int     AnioFin               { get; set; }
        public int     MesFin                { get; set; }
        public string  FechaObjetivo         => new DateTime(AnioFin, MesFin, 28).ToString("dd/MM/yyyy");
    }
}
