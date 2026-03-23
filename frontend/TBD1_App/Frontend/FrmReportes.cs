using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TBD1_App.Backend;
using TBD1_App.Models;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;

namespace TBD1_App.Frontend
{
    public class FrmReportes : Form
    {
        // ── Colores ────────────────────────────────────────
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_CARD      = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color COLOR_ING    = Color.FromArgb(63,  185, 120);
        private readonly Color COLOR_GAS    = Color.FromArgb(248,  81,  73);
        private readonly Color COLOR_AHO    = Color.FromArgb(251, 191,  36);
        private readonly Color COLOR_WARN   = Color.FromArgb(251, 191,  36);
        private readonly Color COLOR_URG    = Color.FromArgb(251, 146,  60);

        // ── DAOs ───────────────────────────────────────────
        private readonly ReporteDAO     _dao    = new ReporteDAO();
        private readonly PresupuestoDAO _preDAO = new PresupuestoDAO();

        private readonly Usuario _usuario;
        private List<Presupuesto> _presupuestos = new List<Presupuesto>();

        // ── Tab control ────────────────────────────────────
        private TabControl tabReportes;
        private readonly List<ComboBox> _combosPresupuesto = new List<ComboBox>();

        public FrmReportes(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarPresupuestos();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Reportes";
            this.Size            = new Size(1150, 720);
            this.MinimumSize     = new Size(1150, 720);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Font            = new Font("Segoe UI", 9.5f);

            // Header
            var pnlHeader = new Panel { Size = new Size(1150, 55), Location = new Point(0, 0), BackColor = BG_PANEL };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1)) e.Graphics.DrawLine(pen, 0, 54, 1150, 54);
                using (var b = new SolidBrush(ACCENT)) e.Graphics.FillRectangle(b, 0, 0, 4, 55);
            };
            pnlHeader.Controls.Add(new Label { Text = "Reportes Analíticos", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(18, 14) });
            pnlHeader.Controls.Add(new Label { Text = "Visualiza y analiza tu información financiera", Font = new Font("Segoe UI", 9f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(220, 19) });

            // Botón exportar PDF
            var btnPdf = new Button
            {
                Text      = "⬇  Exportar PDF",
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size      = new Size(140, 32),
                Location  = new Point(990, 11),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(63, 185, 120),
                ForeColor = Color.FromArgb(10, 12, 18),
                Cursor    = Cursors.Hand
            };
            btnPdf.FlatAppearance.BorderSize = 0;
            btnPdf.Click += BtnExportarPdf_Click;
            pnlHeader.Controls.Add(btnPdf);

            this.Controls.Add(pnlHeader);

            // TabControl con estilo
            tabReportes = new TabControl
            {
                Size      = new Size(1130, 640),
                Location  = new Point(10, 62),
                Font      = new Font("Segoe UI", 9.5f)
            };
            tabReportes.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabReportes.DrawItem += TabReportes_DrawItem;
            tabReportes.SelectedIndexChanged += (s, e) => tabReportes.Invalidate();

            tabReportes.TabPages.Add(CrearTabBalance());
            tabReportes.TabPages.Add(CrearTabGastosCategoria());
            tabReportes.TabPages.Add(CrearTabCumplimiento());
            tabReportes.TabPages.Add(CrearTabTendencia());
            tabReportes.TabPages.Add(CrearTabObligaciones());
            tabReportes.TabPages.Add(CrearTabAhorro());

            this.Controls.Add(tabReportes);
        }

        // ── Draw tabs con estilo dark ──────────────────────
        private void TabReportes_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tab = tabReportes.TabPages[e.Index];
            bool sel = e.Index == tabReportes.SelectedIndex;
            Color bg  = sel ? BG_CARD  : BG_PANEL;
            Color fg  = sel ? ACCENT   : TEXT_MUTED;

            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            if (sel)
                e.Graphics.FillRectangle(new SolidBrush(ACCENT),
                    e.Bounds.X, e.Bounds.Bottom - 2, e.Bounds.Width, 2);

            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(tab.Text, new Font("Segoe UI", 8.5f, sel ? FontStyle.Bold : FontStyle.Regular),
                new SolidBrush(fg), e.Bounds, sf);
        }

        // ── Cargar presupuestos para combos ───────────────
        private void CargarPresupuestos()
        {
            _presupuestos = _preDAO.ObtenerPorUsuario(_usuario.IdUser);

            // Llenar todos los combos de presupuesto registrados
            foreach (var cbo in _combosPresupuesto)
            {
                cbo.Items.Clear();
                foreach (var pre in _presupuestos)
                    cbo.Items.Add(new ComboItem(pre.IdPresupuesto, pre.NombreDescriptivo));
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
        }

        // ════════════════════════════════════════════════════
        // TAB 1 — Balance Mensual
        // ════════════════════════════════════════════════════
        private TabPage CrearTabBalance()
        {
            var tab = new TabPage("R1: Balance Mensual") { BackColor = BG_DARK, Padding = new Padding(0) };

            var pnlFiltros = CrearPanelFiltros(tab, 0);

            // Filtros
            AgregarLabel(pnlFiltros, "DESDE:", 10, 15);
            var cboAnioIni = AgregarComboAnio(pnlFiltros, 60, 10);
            var cboMesIni  = AgregarComboMes(pnlFiltros, 130, 10);

            AgregarLabel(pnlFiltros, "HASTA:", 260, 15);
            var cboAnioFin = AgregarComboAnio(pnlFiltros, 310, 10);
            var cboMesFin  = AgregarComboMes(pnlFiltros, 380, 10);

            var btnGenerar = CrearBtnGenerar(pnlFiltros, 520, 8);

            // Área gráfica + tabla
            var pnlGrafica = new Panel { Size = new Size(1110, 300), Location = new Point(5, 55), BackColor = BG_CARD };
            tab.Controls.Add(pnlGrafica);

            var dgv = CrearGrid(tab, new Size(1110, 220), new Point(5, 362));
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMes",      HeaderText = "MES/AÑO",   FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIng",      HeaderText = "INGRESOS",  FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colGas",      HeaderText = "GASTOS",    FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAho",      HeaderText = "AHORROS",   FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "colBalance",  HeaderText = "BALANCE",   FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });

            List<ReporteBalanceMes> datos = null;

            dgv.CellFormatting += (s, e) =>
            {
                if (datos == null || e.RowIndex < 0 || e.RowIndex >= datos.Count) return;
                if (dgv.Columns[e.ColumnIndex].Name == "colIng")     e.CellStyle.ForeColor = COLOR_ING;
                if (dgv.Columns[e.ColumnIndex].Name == "colGas")     e.CellStyle.ForeColor = COLOR_GAS;
                if (dgv.Columns[e.ColumnIndex].Name == "colAho")     e.CellStyle.ForeColor = COLOR_AHO;
                if (dgv.Columns[e.ColumnIndex].Name == "colBalance")
                    e.CellStyle.ForeColor = datos[e.RowIndex].BalanceFinal >= 0 ? COLOR_ING : COLOR_GAS;
            };

            btnGenerar.Click += (s, e) =>
            {
                try
                {
                    datos = _dao.ObtenerBalanceRango(_usuario.IdUser,
                        (int)cboAnioIni.SelectedItem, int.Parse(((ComboItem)cboMesIni.SelectedItem).Value),
                        (int)cboAnioFin.SelectedItem, int.Parse(((ComboItem)cboMesFin.SelectedItem).Value));

                    dgv.Rows.Clear();
                    foreach (var d in datos)
                        dgv.Rows.Add(d.NombreMes,
                            $"L {d.TotalIngresos:N2}", $"L {d.TotalGastos:N2}",
                            $"L {d.TotalAhorros:N2}",  $"L {d.BalanceFinal:N2}");

                    DibujarBarrasBalance(pnlGrafica, datos);
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            return tab;
        }

        // ════════════════════════════════════════════════════
        // TAB 2 — Gastos por Categoría (Pie)
        // ════════════════════════════════════════════════════
        private TabPage CrearTabGastosCategoria()
        {
            var tab = new TabPage("R2: Gastos Categoría") { BackColor = BG_DARK };
            var pnlFiltros = CrearPanelFiltros(tab, 0);

            AgregarLabel(pnlFiltros, "AÑO:", 10, 15);
            var cboAnio = AgregarComboAnio(pnlFiltros, 50, 10);
            AgregarLabel(pnlFiltros, "MES:", 140, 15);
            var cboMes = AgregarComboMes(pnlFiltros, 180, 10);
            var btnGen = CrearBtnGenerar(pnlFiltros, 340, 8);

            var pnlPie = new Panel { Size = new Size(500, 480), Location = new Point(5, 55), BackColor = BG_CARD };
            tab.Controls.Add(pnlPie);

            var dgv = CrearGrid(tab, new Size(595, 480), new Point(510, 55));
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CATEGORÍA",      FillWeight = 35 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TOTAL (L)",      FillWeight = 25, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "%",              FillWeight = 15, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TRANSACCIONES",  FillWeight = 25, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });

            btnGen.Click += (s, e) =>
            {
                try
                {
                    var datos = _dao.ObtenerGastosCategoria(_usuario.IdUser,
                        (int)cboAnio.SelectedItem, int.Parse(((ComboItem)cboMes.SelectedItem).Value));
                    dgv.Rows.Clear();
                    foreach (var d in datos)
                        dgv.Rows.Add(d.NombreCategoria, $"L {d.TotalGastado:N2}", $"{d.Porcentaje}%", d.NumTransacciones);
                    DibujarPieGastos(pnlPie, datos);
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            return tab;
        }

        // ════════════════════════════════════════════════════
        // TAB 3 — Cumplimiento Presupuesto
        // ════════════════════════════════════════════════════
        private TabPage CrearTabCumplimiento()
        {
            var tab = new TabPage("R3: Cumplimiento") { BackColor = BG_DARK };
            var pnlFiltros = CrearPanelFiltros(tab, 0);

            AgregarLabel(pnlFiltros, "PRESUPUESTO:", 10, 15);
            var cboPre = AgregarComboPresupuesto(pnlFiltros, 100, 10, 220);

            AgregarLabel(pnlFiltros, "AÑO:", 335, 15);
            var cboAnio = AgregarComboAnio(pnlFiltros, 370, 10);
            AgregarLabel(pnlFiltros, "MES:", 450, 15);
            var cboMes = AgregarComboMes(pnlFiltros, 485, 10);
            var btnGen = CrearBtnGenerar(pnlFiltros, 640, 8);

            var dgv = CrearGrid(tab, new Size(1110, 530), new Point(5, 55));
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CATEGORÍA",      FillWeight = 18 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SUBCATEGORÍA",   FillWeight = 18 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PRESUPUESTADO",  FillWeight = 16, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "EJECUTADO",      FillWeight = 16, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DIFERENCIA",     FillWeight = 16, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% EJECUCIÓN",    FillWeight = 10, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SEMÁFORO",       FillWeight = 6,  DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });

            List<ReporteCumplimiento> datos = null;

            dgv.CellFormatting += (s, e) =>
            {
                if (datos == null || e.RowIndex < 0 || e.RowIndex >= datos.Count) return;
                var d = datos[e.RowIndex];
                if (dgv.Columns[e.ColumnIndex].Name != null)
                {
                    Color color = d.Semaforo == "Verde" ? COLOR_ING : d.Semaforo == "Amarillo" ? COLOR_WARN : COLOR_GAS;
                    if (e.ColumnIndex == 6) { e.CellStyle.BackColor = color; e.CellStyle.ForeColor = BG_DARK; }
                    if (e.ColumnIndex == 5) e.CellStyle.ForeColor = color;
                    if (e.ColumnIndex == 4) e.CellStyle.ForeColor = d.Diferencia >= 0 ? COLOR_ING : COLOR_GAS;
                }
            };

            btnGen.Click += (s, e) =>
            {
                if (cboPre.SelectedItem == null) return;
                string idPre = ((ComboItem)cboPre.SelectedItem).Value;
                try
                {
                    datos = _dao.ObtenerCumplimiento(_usuario.IdUser, idPre,
                        (int)cboAnio.SelectedItem, int.Parse(((ComboItem)cboMes.SelectedItem).Value));
                    dgv.Rows.Clear();
                    foreach (var d in datos)
                        dgv.Rows.Add(d.NombreCategoria, d.NombreSubcategoria,
                            $"L {d.MontoPresupuestado:N2}", $"L {d.MontoEjecutado:N2}",
                            $"L {d.Diferencia:N2}", $"{d.PorcentajeEjecucion}%", d.Semaforo);
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            return tab;
        }

        // ════════════════════════════════════════════════════
        // TAB 4 — Tendencia Gastos
        // ════════════════════════════════════════════════════
        private TabPage CrearTabTendencia()
        {
            var tab = new TabPage("R4: Tendencia") { BackColor = BG_DARK };
            var pnlFiltros = CrearPanelFiltros(tab, 0);

            AgregarLabel(pnlFiltros, "DESDE:", 10, 15);
            var cboAnioIni = AgregarComboAnio(pnlFiltros, 60, 10);
            var cboMesIni  = AgregarComboMes(pnlFiltros, 130, 10);
            AgregarLabel(pnlFiltros, "HASTA:", 260, 15);
            var cboAnioFin = AgregarComboAnio(pnlFiltros, 310, 10);
            var cboMesFin  = AgregarComboMes(pnlFiltros, 380, 10);
            var btnGen = CrearBtnGenerar(pnlFiltros, 520, 8);

            var pnlGrafica = new Panel { Size = new Size(1110, 380), Location = new Point(5, 55), BackColor = BG_CARD };
            tab.Controls.Add(pnlGrafica);

            var dgv = CrearGrid(tab, new Size(1110, 175), new Point(5, 442));
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PERÍODO",    FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CATEGORÍA",  FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TOTAL (L)",  FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });

            btnGen.Click += (s, e) =>
            {
                try
                {
                    var datos = _dao.ObtenerTendenciaGastos(_usuario.IdUser,
                        (int)cboAnioIni.SelectedItem, int.Parse(((ComboItem)cboMesIni.SelectedItem).Value),
                        (int)cboAnioFin.SelectedItem, int.Parse(((ComboItem)cboMesFin.SelectedItem).Value));
                    dgv.Rows.Clear();
                    foreach (var d in datos)
                        dgv.Rows.Add(d.Periodo, d.NombreCategoria, $"L {d.TotalGastado:N2}");
                    DibujarLineasTendencia(pnlGrafica, datos);
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            return tab;
        }

        // ════════════════════════════════════════════════════
        // TAB 5 — Estado Obligaciones
        // ════════════════════════════════════════════════════
        private TabPage CrearTabObligaciones()
        {
            var tab = new TabPage("R5: Obligaciones") { BackColor = BG_DARK };
            var pnlFiltros = CrearPanelFiltros(tab, 0);

            AgregarLabel(pnlFiltros, "AÑO:", 10, 15);
            var cboAnio = AgregarComboAnio(pnlFiltros, 50, 10);
            AgregarLabel(pnlFiltros, "MES:", 140, 15);
            var cboMes = AgregarComboMes(pnlFiltros, 180, 10);
            var btnGen = CrearBtnGenerar(pnlFiltros, 340, 8);

            var dgv = CrearGrid(tab, new Size(1110, 530), new Point(5, 55));
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "",              Width = 10, FillWeight = 2 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OBLIGACIÓN",    FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CATEGORÍA",     FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MONTO (L)",     FillWeight = 12, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DÍA VENCE",     FillWeight = 10, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DÍAS REST.",    FillWeight = 10, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ESTADO",        FillWeight = 12, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ÚLTIMO PAGO",   FillWeight = 19 });

            List<ReporteObligacion> datos = null;

            dgv.CellFormatting += (s, e) =>
            {
                if (datos == null || e.RowIndex < 0 || e.RowIndex >= datos.Count) return;
                var d = datos[e.RowIndex];
                Color c = d.EstadoPago == "PAGADA"    ? COLOR_ING :
                          d.EstadoPago == "PENDIENTE" ? COLOR_WARN :
                          d.EstadoPago == "POR VENCER"? COLOR_URG  : COLOR_GAS;
                if (e.ColumnIndex == 0) { e.CellStyle.BackColor = c; e.Value = ""; e.FormattingApplied = true; }
                if (e.ColumnIndex == 6) { e.CellStyle.ForeColor = c; e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); }
            };

            btnGen.Click += (s, e) =>
            {
                try
                {
                    datos = _dao.ObtenerEstadoObligaciones(_usuario.IdUser,
                        (int)cboAnio.SelectedItem, int.Parse(((ComboItem)cboMes.SelectedItem).Value));
                    dgv.Rows.Clear();
                    foreach (var d in datos)
                        dgv.Rows.Add("", d.NombreObligacion, d.NombreCategoria,
                            $"L {d.Monto:N2}", $"Día {d.DiaVencimiento}",
                            d.DiasRestantes < 0 ? $"Vencida {Math.Abs(d.DiasRestantes)}d" : $"{d.DiasRestantes}d",
                            d.EstadoPago,
                            d.FechaUltimoPago.HasValue ? d.FechaUltimoPago.Value.ToString("dd/MM/yyyy") : "Sin pagos");
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            return tab;
        }

        // ════════════════════════════════════════════════════
        // TAB 6 — Progreso Ahorro
        // ════════════════════════════════════════════════════
        private TabPage CrearTabAhorro()
        {
            var tab = new TabPage("R6: Metas Ahorro") { BackColor = BG_DARK };
            var pnlFiltros = CrearPanelFiltros(tab, 0);

            AgregarLabel(pnlFiltros, "PRESUPUESTO:", 10, 15);
            var cboPre = AgregarComboPresupuesto(pnlFiltros, 100, 10, 250);
            var btnGen = CrearBtnGenerar(pnlFiltros, 365, 8);

            // Panel de barras de progreso
            var pnlBarras = new Panel { Size = new Size(1110, 340), Location = new Point(5, 55), BackColor = BG_CARD, AutoScroll = true };
            tab.Controls.Add(pnlBarras);

            var dgv = CrearGrid(tab, new Size(1110, 200), new Point(5, 402));
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "META",           FillWeight = 22 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OBJETIVO TOTAL", FillWeight = 18, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ACUMULADO",      FillWeight = 18, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "% COMPLETADO",   FillWeight = 14, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "FECHA OBJETIVO", FillWeight = 14, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DÍAS REST.",     FillWeight = 14, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });

            btnGen.Click += (s, e) =>
            {
                if (cboPre.SelectedItem == null) return;
                string idPre = ((ComboItem)cboPre.SelectedItem).Value;
                try
                {
                    var datos = _dao.ObtenerProgresoAhorro(_usuario.IdUser, idPre);
                    dgv.Rows.Clear();
                    pnlBarras.Controls.Clear();

                    int y = 10;
                    foreach (var d in datos)
                    {
                        // Barra de progreso visual
                        DibujarBarraProgreso(pnlBarras, d, ref y);

                        dgv.Rows.Add(d.NombreMeta,
                            $"L {d.MontoObjetivoTotal:N2}", $"L {d.MontoAcumulado:N2}",
                            $"{d.PorcentajeCompletado}%", d.FechaObjetivo,
                            d.DiasRestantes > 0 ? $"{d.DiasRestantes}d" : "Vencido");
                    }

                    if (datos.Count == 0)
                        pnlBarras.Controls.Add(new Label { Text = "No hay metas de ahorro en este presupuesto.", Font = new Font("Segoe UI", 11f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(20, 130) });
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            return tab;
        }

        // ════════════════════════════════════════════════════
        // GRÁFICAS
        // ════════════════════════════════════════════════════

        private void DibujarBarrasBalance(Panel panel, List<ReporteBalanceMes> datos)
        {
            panel.Paint -= null;
            panel.Paint += (s, e) =>
            {
                if (datos == null || datos.Count == 0) return;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BG_CARD);

                int padding = 60, barWidth = 18, groupGap = 20;
                int chartH = panel.Height - 60;
                decimal maxVal = datos.Max(d => Math.Max(d.TotalIngresos, Math.Max(d.TotalGastos, d.TotalAhorros)));
                if (maxVal == 0) return;

                int groupWidth = barWidth * 3 + 10 + groupGap;
                int x = padding + 20;

                // Líneas guía
                for (int i = 0; i <= 4; i++)
                {
                    int gy = 20 + (chartH - 40) * i / 4;
                    using (var pen = new Pen(Color.FromArgb(40, 255, 255, 255), 1))
                        g.DrawLine(pen, padding, gy, panel.Width - 20, gy);
                    decimal val = maxVal * (4 - i) / 4;
                    g.DrawString($"L{val:N0}", new Font("Segoe UI", 7f), new SolidBrush(TEXT_MUTED), 2, gy - 6);
                }

                foreach (var d in datos)
                {
                    void DrawBar(decimal val, Color color, int offsetX)
                    {
                        int h = (int)(val * (chartH - 40) / maxVal);
                        int by = 20 + (chartH - 40) - h;
                        g.FillRectangle(new SolidBrush(color), x + offsetX, by, barWidth, h);
                    }

                    DrawBar(d.TotalIngresos, COLOR_ING, 0);
                    DrawBar(d.TotalGastos,   COLOR_GAS, barWidth + 2);
                    DrawBar(d.TotalAhorros,  COLOR_AHO, (barWidth + 2) * 2);

                    g.DrawString(d.NombreMes.Substring(0, Math.Min(3, d.NombreMes.Length)),
                        new Font("Segoe UI", 7.5f), new SolidBrush(TEXT_MUTED),
                        x + 2, panel.Height - 30);

                    x += groupWidth;
                }

                // Leyenda
                int lx = padding;
                foreach (var (col, txt) in new[] { (COLOR_ING, "Ingresos"), (COLOR_GAS, "Gastos"), (COLOR_AHO, "Ahorros") })
                {
                    g.FillRectangle(new SolidBrush(col), lx, panel.Height - 15, 12, 10);
                    g.DrawString(txt, new Font("Segoe UI", 8f), new SolidBrush(TEXT_MUTED), lx + 15, panel.Height - 16);
                    lx += 80;
                }
            };
            panel.Invalidate();
        }

        private void DibujarPieGastos(Panel panel, List<ReporteGastoCategoria> datos)
        {
            Color[] paleta = {
                Color.FromArgb(56,189,248), Color.FromArgb(248,81,73), Color.FromArgb(251,191,36),
                Color.FromArgb(63,185,120), Color.FromArgb(167,139,250), Color.FromArgb(251,146,60),
                Color.FromArgb(236,72,153), Color.FromArgb(20,184,166)
            };
            panel.Paint -= null;
            panel.Paint += (s, e) =>
            {
                if (datos == null || datos.Count == 0) return;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BG_CARD);

                decimal total = datos.Sum(d => d.TotalGastado);
                if (total == 0) return;

                int cx = panel.Width / 2, cy = panel.Height / 2;
                int r  = Math.Min(cx, cy) - 40;
                float startAngle = -90;

                for (int i = 0; i < datos.Count; i++)
                {
                    float sweep = (float)(datos[i].TotalGastado * 360 / total);
                    Color col = paleta[i % paleta.Length];
                    g.FillPie(new SolidBrush(col), cx - r, cy - r, r * 2, r * 2, startAngle, sweep);
                    g.DrawPie(new Pen(BG_CARD, 2), cx - r, cy - r, r * 2, r * 2, startAngle, sweep);

                    // Etiqueta
                    float mid = startAngle + sweep / 2;
                    float lx  = cx + (float)Math.Cos(mid * Math.PI / 180) * (r * 0.7f);
                    float ly  = cy + (float)Math.Sin(mid * Math.PI / 180) * (r * 0.7f);
                    if (sweep > 15)
                        g.DrawString($"{datos[i].Porcentaje}%", new Font("Segoe UI", 8f, FontStyle.Bold),
                            new SolidBrush(Color.White), lx - 15, ly - 8);

                    startAngle += sweep;
                }

                // Leyenda
                int ly2 = 10;
                for (int i = 0; i < datos.Count; i++)
                {
                    g.FillRectangle(new SolidBrush(paleta[i % paleta.Length]), 10, ly2, 12, 12);
                    g.DrawString(datos[i].NombreCategoria, new Font("Segoe UI", 8f), new SolidBrush(TEXT_PRIMARY), 26, ly2);
                    ly2 += 18;
                }
            };
            panel.Invalidate();
        }

        private void DibujarLineasTendencia(Panel panel, List<ReporteTendencia> datos)
        {
            panel.Paint -= null;
            panel.Paint += (s, e) =>
            {
                if (datos == null || datos.Count == 0) return;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BG_CARD);

                var categorias = datos.Select(d => d.NombreCategoria).Distinct().ToList();
                var periodos   = datos.Select(d => d.Periodo).Distinct().ToList();
                if (periodos.Count == 0) return;

                Color[] paleta = { ACCENT, COLOR_GAS, COLOR_AHO, COLOR_ING,
                                   Color.FromArgb(167,139,250), Color.FromArgb(251,146,60) };
                int pad = 70, padB = 50;
                int chartW = panel.Width  - pad - 20;
                int chartH = panel.Height - padB - 20;
                decimal maxVal = datos.Max(d => d.TotalGastado);
                if (maxVal == 0) return;

                // Líneas guía
                for (int i = 0; i <= 4; i++)
                {
                    int gy = 10 + chartH * i / 4;
                    using (var pen = new Pen(Color.FromArgb(40, 255, 255, 255), 1))
                        g.DrawLine(pen, pad, gy, pad + chartW, gy);
                    decimal val = maxVal * (4 - i) / 4;
                    g.DrawString($"L{val:N0}", new Font("Segoe UI", 7f), new SolidBrush(TEXT_MUTED), 2, gy - 6);
                }

                // Etiquetas eje X
                for (int i = 0; i < periodos.Count; i++)
                {
                    int px = pad + chartW * i / Math.Max(periodos.Count - 1, 1);
                    g.DrawString(periodos[i], new Font("Segoe UI", 7.5f), new SolidBrush(TEXT_MUTED), px - 15, panel.Height - padB + 5);
                }

                // Líneas por categoría
                for (int ci = 0; ci < categorias.Count; ci++)
                {
                    var color = paleta[ci % paleta.Length];
                    var puntos = new List<PointF>();

                    for (int pi = 0; pi < periodos.Count; pi++)
                    {
                        var d = datos.FirstOrDefault(x => x.NombreCategoria == categorias[ci] && x.Periodo == periodos[pi]);
                        decimal val = d?.TotalGastado ?? 0;
                        float px = pad + chartW * pi / Math.Max(periodos.Count - 1, 1);
                        float py = 10 + chartH - (float)(val * chartH / maxVal);
                        puntos.Add(new PointF(px, py));
                    }

                    if (puntos.Count > 1)
                        g.DrawLines(new Pen(color, 2), puntos.ToArray());
                    foreach (var p in puntos)
                        g.FillEllipse(new SolidBrush(color), p.X - 4, p.Y - 4, 8, 8);
                }

                // Leyenda
                int lx = pad;
                for (int ci = 0; ci < categorias.Count; ci++)
                {
                    g.FillRectangle(new SolidBrush(paleta[ci % paleta.Length]), lx, panel.Height - 20, 12, 10);
                    g.DrawString(categorias[ci], new Font("Segoe UI", 8f), new SolidBrush(TEXT_MUTED), lx + 15, panel.Height - 22);
                    lx += TextRenderer.MeasureText(categorias[ci], new Font("Segoe UI", 8f)).Width + 35;
                }
            };
            panel.Invalidate();
        }

        private void DibujarBarraProgreso(Panel panel, ReporteProgresoAhorro d, ref int y)
        {
            int pct = Math.Min((int)d.PorcentajeCompletado, 100);
            Color barColor = pct < 50 ? COLOR_GAS : pct < 80 ? COLOR_WARN : COLOR_ING;

            panel.Controls.Add(new Label { Text = $"{d.NombreMeta}  —  L {d.MontoAcumulado:N2} / L {d.MontoObjetivoTotal:N2}  ({d.PorcentajeCompletado}%)", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(15, y) });
            y += 22;

            var bg = new Panel { Size = new Size(1060, 20), Location = new Point(15, y), BackColor = BG_DARK };
            var fill = new Panel { Size = new Size(1060 * pct / 100, 20), Location = new Point(0, 0), BackColor = barColor };
            bg.Controls.Add(fill);
            panel.Controls.Add(bg);
            y += 30;

            panel.Controls.Add(new Label { Text = $"Fecha objetivo: {d.FechaObjetivo}  •  Días restantes: {d.DiasRestantes}  •  Mensual necesario: L {(d.DiasRestantes > 0 ? (d.MontoObjetivoTotal - d.MontoAcumulado) / Math.Max(d.MesesVigencia, 1) : 0):N2}", Font = new Font("Segoe UI", 8.5f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(15, y) });
            y += 28;

            panel.Controls.Add(new Label { Size = new Size(1060, 1), Location = new Point(15, y), BackColor = BORDER });
            y += 12;
        }

        // ════════════════════════════════════════════════════
        // EXPORTAR A PDF
        // ════════════════════════════════════════════════════
        private void BtnExportarPdf_Click(object sender, EventArgs e)
        {
            var tab = tabReportes.SelectedTab;
            if (tab == null) { MessageBox.Show("Genera un reporte primero.", "Sin datos"); return; }

            // Verificar que el tab tenga datos (al menos un DataGridView con filas)
            var dgv = EncontrarGrid(tab);
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Genera el reporte primero usando el botón ▶ Generar.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Diálogo para elegir ruta y nombre
            using (var dlg = new SaveFileDialog())
            {
                dlg.Title            = "Guardar reporte como PDF";
                dlg.Filter           = "Archivo PDF (*.pdf)|*.pdf";
                dlg.DefaultExt       = "pdf";
                dlg.FileName         = $"Reporte_{tab.Text.Replace(":", "").Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    GenerarPdf(dlg.FileName, tab, dgv);
                    MessageBox.Show($"PDF guardado exitosamente en:\n{dlg.FileName}",
                        "✓ Exportación exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar el PDF:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerarPdf(string rutaArchivo, TabPage tab, DataGridView dgv)
        {
            using (var doc = new Document(PageSize.A4.Rotate(), 30f, 30f, 40f, 30f))
            using (var writer = PdfWriter.GetInstance(doc, new FileStream(rutaArchivo, FileMode.Create)))
            {
                doc.Open();

                // ── Fuentes ────────────────────────────────────
                var fuenteTitulo  = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 16f, iTextSharp.text.Font.BOLD,   new BaseColor(56, 189, 248));
                var fuenteSubtit  = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10f, iTextSharp.text.Font.NORMAL, new BaseColor(110, 118, 129));
                var fuenteHeader  = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA,  8f, iTextSharp.text.Font.BOLD,   new BaseColor(56, 189, 248));
                var fuenteCelda   = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA,  8f, iTextSharp.text.Font.NORMAL, new BaseColor(200, 210, 220));
                var fuentePie     = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA,  7f, iTextSharp.text.Font.ITALIC,  new BaseColor(110, 118, 129));

                var bgDark   = new BaseColor(15,  17,  23);
                var bgPanel  = new BaseColor(22,  27,  34);
                var bgCard   = new BaseColor(30,  37,  48);
                var bgHeader = new BaseColor(38,  46,  60);

                // ── Encabezado del documento ───────────────────
                var tblEncabezado = new PdfPTable(2) { WidthPercentage = 100 };
                tblEncabezado.SetWidths(new float[] { 70f, 30f });
                tblEncabezado.SpacingAfter = 10f;

                var celdaTitulo = new PdfPCell(new Phrase("PRESUPUESTO PERSONAL — REPORTE", fuenteTitulo))
                { BackgroundColor = bgPanel, Border = iTextSharp.text.Rectangle.NO_BORDER, Padding = 10f, PaddingBottom = 4f };
                tblEncabezado.AddCell(celdaTitulo);

                var infoTexto = $"Usuario: {_usuario.PrimerNombre} {_usuario.PrimerApellido}\nFecha: {DateTime.Now:dd/MM/yyyy HH:mm}";
                var celdaInfo = new PdfPCell(new Phrase(infoTexto, fuenteSubtit))
                { BackgroundColor = bgPanel, Border = iTextSharp.text.Rectangle.NO_BORDER, Padding = 10f, HorizontalAlignment = Element.ALIGN_RIGHT };
                tblEncabezado.AddCell(celdaInfo);

                var celdaNombreReporte = new PdfPCell(new Phrase(tab.Text, fuenteSubtit))
                { BackgroundColor = bgPanel, Border = iTextSharp.text.Rectangle.NO_BORDER, Padding = 4f, PaddingLeft = 10f, Colspan = 2 };
                tblEncabezado.AddCell(celdaNombreReporte);

                doc.Add(tblEncabezado);

                // ── Capturar gráfica si existe ─────────────────
                var panelGrafica = EncontrarPanelGrafica(tab);
                if (panelGrafica != null && panelGrafica.Width > 0 && panelGrafica.Height > 0)
                {
                    try
                    {
                        using (var bmp = new Bitmap(panelGrafica.Width, panelGrafica.Height))
                        {
                            panelGrafica.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, panelGrafica.Width, panelGrafica.Height));
                            using (var ms = new MemoryStream())
                            {
                                bmp.Save(ms, ImageFormat.Png);
                                var img = iTextSharp.text.Image.GetInstance(ms.ToArray());
                                img.ScaleToFit(doc.PageSize.Width - 60f, 220f);
                                img.Alignment = Element.ALIGN_CENTER;
                                img.SpacingAfter = 10f;
                                doc.Add(img);
                            }
                        }
                    }
                    catch { /* Si falla la captura, continúa con la tabla */ }
                }

                // ── Tabla de datos ─────────────────────────────
                int colCount = dgv.Columns.Count;
                var tblDatos = new PdfPTable(colCount) { WidthPercentage = 100 };
                tblDatos.SpacingBefore = 5f;

                // Encabezados
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    var celda = new PdfPCell(new Phrase(col.HeaderText, fuenteHeader))
                    {
                        BackgroundColor  = bgHeader,
                        Border           = iTextSharp.text.Rectangle.BOTTOM_BORDER,
                        BorderColor      = new BaseColor(56, 189, 248),
                        BorderWidth      = 1f,
                        Padding          = 6f,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tblDatos.AddCell(celda);
                }

                // Filas
                bool altRow = false;
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    var bgFila = altRow ? bgCard : bgPanel;
                    altRow = !altRow;

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        string valor = cell.Value?.ToString() ?? "";
                        int align = cell.Style.Alignment == DataGridViewContentAlignment.MiddleRight
                            ? Element.ALIGN_RIGHT
                            : cell.Style.Alignment == DataGridViewContentAlignment.MiddleCenter
                            ? Element.ALIGN_CENTER
                            : Element.ALIGN_LEFT;

                        var celda = new PdfPCell(new Phrase(valor, fuenteCelda))
                        {
                            BackgroundColor     = bgFila,
                            Border              = iTextSharp.text.Rectangle.BOTTOM_BORDER,
                            BorderColor         = new BaseColor(48, 54, 61),
                            BorderWidth         = 0.5f,
                            Padding             = 5f,
                            HorizontalAlignment = align
                        };
                        tblDatos.AddCell(celda);
                    }
                }

                doc.Add(tblDatos);

                // ── Pie de página ──────────────────────────────
                doc.Add(new Paragraph($"\nGenerado por Sistema de Presupuesto Personal  •  Steve Valladares 22341344  •  {DateTime.Now:dd/MM/yyyy}", fuentePie)
                    { Alignment = Element.ALIGN_CENTER });

                doc.Close();
            }
        }

        // ── Busca el primer DataGridView con filas en el tab ─
        private DataGridView EncontrarGrid(TabPage tab)
        {
            foreach (Control c in tab.Controls)
            {
                if (c is DataGridView dgv && dgv.Rows.Count > 0)
                    return dgv;
                // Buscar dentro de panels
                foreach (Control child in c.Controls)
                    if (child is DataGridView d && d.Rows.Count > 0)
                        return d;
            }
            return null;
        }

        // ── Busca el panel de gráfica (panel grande de fondo oscuro) ─
        private Panel EncontrarPanelGrafica(TabPage tab)
        {
            Panel mejor = null;
            foreach (Control c in tab.Controls)
            {
                if (c is Panel p && p.Width > 400 && p.Height > 150)
                {
                    if (mejor == null || p.Width * p.Height > mejor.Width * mejor.Height)
                        mejor = p;
                }
            }
            return mejor;
        }
        private Panel CrearPanelFiltros(TabPage tab, int y)
        {
            var p = new Panel { Size = new Size(1110, 48), Location = new Point(5, y), BackColor = BG_PANEL };
            p.Paint += (s, e) => { using (var pen = new Pen(BORDER, 1)) e.Graphics.DrawLine(pen, 0, 47, 1110, 47); };
            tab.Controls.Add(p);
            return p;
        }

        private void AgregarLabel(Panel p, string txt, int x, int y) =>
            p.Controls.Add(new Label { Text = txt, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(x, y) });

        private ComboBox AgregarComboAnio(Panel p, int x, int y)
        {
            var c = new ComboBox { Size = new Size(65, 26), Location = new Point(x, y), BackColor = BG_CARD, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };
            for (int a = DateTime.Now.Year - 3; a <= DateTime.Now.Year + 1; a++) c.Items.Add(a);
            c.SelectedItem = DateTime.Now.Year;
            p.Controls.Add(c);
            return c;
        }

        private ComboBox AgregarComboMes(Panel p, int x, int y)
        {
            var c = new ComboBox { Size = new Size(120, 26), Location = new Point(x, y), BackColor = BG_CARD, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };
            for (int m = 1; m <= 12; m++) c.Items.Add(new ComboItem(m.ToString(), new DateTime(2000, m, 1).ToString("MMMM")));
            c.SelectedIndex = DateTime.Now.Month - 1;
            p.Controls.Add(c);
            return c;
        }

        private ComboBox AgregarComboPresupuesto(Panel p, int x, int y, int w)
        {
            var c = new ComboBox { Size = new Size(w, 26), Location = new Point(x, y), BackColor = BG_CARD, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };
            // Los items se llenan en CargarPresupuestos() después de inicializar
            if (!_combosPresupuesto.Contains(c))
                _combosPresupuesto.Add(c);
            p.Controls.Add(c);
            return c;
        }

        private Button CrearBtnGenerar(Panel p, int x, int y)
        {
            var btn = new Button { Text = "▶ Generar", Size = new Size(110, 30), Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = ACCENT, ForeColor = Color.FromArgb(10, 12, 18), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            p.Controls.Add(btn);
            return btn;
        }

        private DataGridView CrearGrid(TabPage tab, Size size, Point location)
        {
            var dgv = new DataGridView
            {
                Size = size, Location = location,
                BackgroundColor = BG_CARD, BorderStyle = BorderStyle.None, GridColor = BORDER,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9f), RowTemplate = { Height = 28 }
            };
            dgv.DefaultCellStyle.BackColor          = BG_CARD;
            dgv.DefaultCellStyle.ForeColor          = TEXT_PRIMARY;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(38, 46, 60);
            dgv.DefaultCellStyle.SelectionForeColor = ACCENT;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = BG_PANEL;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TEXT_MUTED;
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            dgv.ColumnHeadersBorderStyle    = DataGridViewHeaderBorderStyle.None;
            dgv.EnableHeadersVisualStyles   = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(25, 32, 42);
            tab.Controls.Add(dgv);
            return dgv;
        }
    }
}
