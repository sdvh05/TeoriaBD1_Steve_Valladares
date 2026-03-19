using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmTransacciones : Form
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

        // ── DAOs ───────────────────────────────────────────
        private readonly TransaccionDAO _dao    = new TransaccionDAO();
        private readonly PresupuestoDAO _preDAO = new PresupuestoDAO();

        // ── Estado ─────────────────────────────────────────
        private readonly Usuario _usuario;
        private List<Transaccion> _transacciones = new List<Transaccion>();
        private List<Presupuesto> _presupuestos  = new List<Presupuesto>();

        // ── Controles principales ──────────────────────────
        private DataGridView dgvTransacciones;
        private ComboBox     cboPresupuesto;
        private ComboBox     cboAnio;
        private ComboBox     cboMes;
        private ComboBox     cboTipo;
        private Button       btnNueva;
        private Button       btnEditar;
        private Button       btnEliminar;
        private Button       btnRefrescar;
        private Label        lblTotalIng;
        private Label        lblTotalGas;
        private Label        lblTotalAho;
        private Label        lblBalance;
        private Label        lblError;

        public FrmTransacciones(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarPresupuestos();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Transacciones";
            this.Size            = new Size(1100, 680);
            this.MinimumSize     = new Size(1100, 680);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── HEADER ─────────────────────────────────────
            var pnlHeader = new Panel
            {
                Size      = new Size(1100, 60),
                Location  = new Point(0, 0),
                BackColor = BG_PANEL
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawLine(pen, 0, 59, 1100, 59);
                using (var brush = new SolidBrush(COLOR_ING))
                    e.Graphics.FillRectangle(brush, 0, 0, 4, 60);
            };
            this.Controls.Add(pnlHeader);

            var lblTitulo = new Label
            {
                Text      = "Transacciones",
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(18, 16)
            };
            pnlHeader.Controls.Add(lblTitulo);

            var lblSub = new Label
            {
                Text      = "Registra y gestiona tus ingresos, gastos y ahorros",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(195, 22)
            };
            pnlHeader.Controls.Add(lblSub);

            // ── FILTROS ────────────────────────────────────
            var pnlFiltros = new Panel
            {
                Size      = new Size(1100, 55),
                Location  = new Point(0, 60),
                BackColor = BG_CARD
            };
            pnlFiltros.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawLine(pen, 0, 54, 1100, 54);
            };
            this.Controls.Add(pnlFiltros);

            // Label Presupuesto
            AgregarLabelFiltro(pnlFiltros, "PRESUPUESTO:", 10, 18);
            cboPresupuesto = AgregarCombo(pnlFiltros, 110, 13, 230);
            cboPresupuesto.SelectedIndexChanged += (s, e) => AlCambiarPresupuesto();

            // Label Año
            AgregarLabelFiltro(pnlFiltros, "AÑO:", 352, 18);
            cboAnio = AgregarCombo(pnlFiltros, 382, 13, 80);
            cboAnio.SelectedIndexChanged += (s, e) => CargarTransacciones();

            // Label Mes
            AgregarLabelFiltro(pnlFiltros, "MES:", 472, 18);
            cboMes = AgregarCombo(pnlFiltros, 500, 13, 130);
            cboMes.Items.Add(new ComboItem("0", "Todos"));
            for (int m = 1; m <= 12; m++)
                cboMes.Items.Add(new ComboItem(m.ToString(), new DateTime(2000, m, 1).ToString("MMMM")));
            cboMes.SelectedIndex = 0; // "Todos" por defecto
            cboMes.SelectedIndexChanged += (s, e) => CargarTransacciones();

            // Label Tipo
            AgregarLabelFiltro(pnlFiltros, "TIPO:", 642, 18);
            cboTipo = AgregarCombo(pnlFiltros, 672, 13, 120);
            cboTipo.Items.Add(new ComboItem("",        "Todos"));
            cboTipo.Items.Add(new ComboItem("ingreso", "Ingresos"));
            cboTipo.Items.Add(new ComboItem("gasto",   "Gastos"));
            cboTipo.Items.Add(new ComboItem("ahorro",  "Ahorros"));
            cboTipo.SelectedIndex = 0;
            cboTipo.SelectedIndexChanged += (s, e) => CargarTransacciones();

            // Botón refrescar
            btnRefrescar = CrearBoton(pnlFiltros, "↺ Refrescar", 806, 13, 100, BG_CARD, TEXT_MUTED);
            btnRefrescar.FlatAppearance.BorderColor = BORDER;
            btnRefrescar.Click += (s, e) => CargarTransacciones();

            // ── TOTALES ────────────────────────────────────
            var pnlTotales = new Panel
            {
                Size      = new Size(1100, 70),
                Location  = new Point(0, 115),
                BackColor = BG_DARK
            };
            this.Controls.Add(pnlTotales);

            CrearTarjetaTotalSimple(pnlTotales, 10,  8, "INGRESOS",  "L 0.00", COLOR_ING, out lblTotalIng);
            CrearTarjetaTotalSimple(pnlTotales, 215, 8, "GASTOS",    "L 0.00", COLOR_GAS, out lblTotalGas);
            CrearTarjetaTotalSimple(pnlTotales, 420, 8, "AHORROS",   "L 0.00", COLOR_AHO, out lblTotalAho);
            CrearTarjetaTotalSimple(pnlTotales, 625, 8, "BALANCE",   "L 0.00", ACCENT,    out lblBalance);

            // ── BOTONES ACCIÓN ─────────────────────────────
            var pnlAcciones = new Panel
            {
                Size      = new Size(1100, 42),
                Location  = new Point(0, 185),
                BackColor = BG_DARK
            };
            this.Controls.Add(pnlAcciones);

            btnNueva   = CrearBoton(pnlAcciones, "+ Nueva",   10,  6, 110, ACCENT,       Color.FromArgb(10, 12, 18));
            btnEditar  = CrearBoton(pnlAcciones, "✎ Editar",  130, 6, 110, BG_CARD,      TEXT_PRIMARY);
            btnEliminar= CrearBoton(pnlAcciones, "✕ Eliminar",250, 6, 110, BG_CARD,      COLOR_GAS);

            btnEditar.FlatAppearance.BorderColor   = BORDER;
            btnEliminar.FlatAppearance.BorderColor = COLOR_GAS;

            btnNueva.Click    += BtnNueva_Click;
            btnEditar.Click   += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;

            lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = COLOR_GAS,
                AutoSize  = true,
                Location  = new Point(375, 14),
                Visible   = false
            };
            pnlAcciones.Controls.Add(lblError);

            // ── GRID ───────────────────────────────────────
            dgvTransacciones = new DataGridView
            {
                Size              = new Size(1080, 390),
                Location          = new Point(10, 232),
                BackgroundColor   = BG_CARD,
                BorderStyle       = BorderStyle.None,
                GridColor         = BORDER,
                RowHeadersVisible = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = new Font("Segoe UI", 9.5f)
            };

            // Estilo dark del grid
            dgvTransacciones.DefaultCellStyle.BackColor      = BG_CARD;
            dgvTransacciones.DefaultCellStyle.ForeColor      = TEXT_PRIMARY;
            dgvTransacciones.DefaultCellStyle.SelectionBackColor = Color.FromArgb(38, 46, 60);
            dgvTransacciones.DefaultCellStyle.SelectionForeColor = ACCENT;
            dgvTransacciones.ColumnHeadersDefaultCellStyle.BackColor  = BG_PANEL;
            dgvTransacciones.ColumnHeadersDefaultCellStyle.ForeColor  = TEXT_MUTED;
            dgvTransacciones.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 8f, FontStyle.Bold);
            dgvTransacciones.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvTransacciones.EnableHeadersVisualStyles = false;
            dgvTransacciones.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(25, 32, 42);
            dgvTransacciones.RowTemplate.Height = 32;

            // Columnas
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",      HeaderText = "ID",           Width = 70,  FillWeight = 5,  Visible = false });
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFecha",    HeaderText = "FECHA",        FillWeight = 10 });
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTipo",     HeaderText = "TIPO",         FillWeight = 8  });
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCategoria",HeaderText = "CATEGORÍA",    FillWeight = 12 });
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSubcat",   HeaderText = "SUBCATEGORÍA", FillWeight = 12 });
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDesc",     HeaderText = "DESCRIPCIÓN",  FillWeight = 20 });
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMonto",    HeaderText = "MONTO (L)",    FillWeight = 10, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMetodo",   HeaderText = "MÉTODO PAGO",  FillWeight = 10 });
            dgvTransacciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFactura",  HeaderText = "N° FACTURA",   FillWeight = 8  });

            dgvTransacciones.CellFormatting += DgvTransacciones_CellFormatting;
            this.Controls.Add(dgvTransacciones);
        }

        // ── Helpers UI ─────────────────────────────────────
        private void AgregarLabelFiltro(Panel panel, string texto, int x, int y)
        {
            panel.Controls.Add(new Label
            {
                Text      = texto,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(x, y)
            });
        }

        private ComboBox AgregarCombo(Panel panel, int x, int y, int width)
        {
            var cbo = new ComboBox
            {
                Size          = new Size(width, 26),
                Location      = new Point(x, y),
                BackColor     = BG_CARD,
                ForeColor     = TEXT_PRIMARY,
                FlatStyle     = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9.5f),
                DrawMode      = DrawMode.OwnerDrawFixed,
                ItemHeight    = 22
            };

            // Dibuja cada item con padding izquierdo para que el texto no quede pegado
            cbo.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                var combo = (ComboBox)s;
                bool selected = (e.State & DrawItemState.Selected) != 0;

                Color bg = selected ? Color.FromArgb(38, 46, 60) : BG_CARD;
                Color fg = selected ? ACCENT : TEXT_PRIMARY;

                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                e.Graphics.DrawString(
                    combo.Items[e.Index].ToString(),
                    combo.Font,
                    new SolidBrush(fg),
                    e.Bounds.X + 8,
                    e.Bounds.Y + 2);
            };

            panel.Controls.Add(cbo);
            return cbo;
        }

        private Button CrearBoton(Panel panel, string texto, int x, int y, int width, Color backColor, Color foreColor)
        {
            var btn = new Button
            {
                Text      = texto,
                Size      = new Size(width, 30),
                Location  = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            panel.Controls.Add(btn);
            return btn;
        }

        private void CrearTarjetaTotalSimple(Panel panel, int x, int y, string titulo, string valor, Color color, out Label lblValor)
        {
            var pnl = new Panel
            {
                Size      = new Size(195, 54),
                Location  = new Point(x, y),
                BackColor = BG_CARD
            };
            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var brush = new SolidBrush(color))
                    e.Graphics.FillRectangle(brush, 0, 0, 3, pnl.Height);
            };
            panel.Controls.Add(pnl);

            pnl.Controls.Add(new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(10, 8)
            });

            lblValor = new Label
            {
                Text      = valor,
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = color,
                AutoSize  = false,
                Size      = new Size(175, 26),
                Location  = new Point(10, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnl.Controls.Add(lblValor);
        }

        // ── Color por tipo en el grid ──────────────────────
        private void DgvTransacciones_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _transacciones.Count) return;
            var t = _transacciones[e.RowIndex];

            Color color = t.Tipo == "ingreso" ? COLOR_ING :
                          t.Tipo == "gasto"   ? COLOR_GAS : COLOR_AHO;

            if (dgvTransacciones.Columns[e.ColumnIndex].Name == "colTipo")
            {
                e.CellStyle.ForeColor = color;
                e.CellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            }
            if (dgvTransacciones.Columns[e.ColumnIndex].Name == "colMonto")
            {
                e.CellStyle.ForeColor = color;
            }
        }

        // ── Cargar presupuestos en combo ───────────────────
        private void CargarPresupuestos()
        {
            try
            {
                _presupuestos = _preDAO.ObtenerPorUsuario(_usuario.IdUser);
                cboPresupuesto.Items.Clear();

                if (_presupuestos.Count == 0)
                {
                    cboPresupuesto.Items.Add(new ComboItem("", "Sin presupuestos"));
                    cboPresupuesto.SelectedIndex = 0;
                    return;
                }

                foreach (var p in _presupuestos)
                    cboPresupuesto.Items.Add(new ComboItem(p.IdPresupuesto, p.NombreDescriptivo));

                cboPresupuesto.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MostrarError("Error cargando presupuestos: " + ex.Message);
            }
        }

        // ── Al cambiar presupuesto — poblar años del rango ─
        private void AlCambiarPresupuesto()
        {
            if (cboPresupuesto.SelectedItem == null) return;
            var idPre = ((ComboItem)cboPresupuesto.SelectedItem).Value;
            if (string.IsNullOrEmpty(idPre)) return;

            // Buscar el presupuesto seleccionado
            var pre = _presupuestos.Find(p => p.IdPresupuesto == idPre);
            if (pre == null) return;

            // Poblar años del rango del presupuesto
            cboAnio.Items.Clear();
            for (int a = pre.AnioInicio; a <= pre.AnioFin; a++)
                cboAnio.Items.Add(new ComboItem(a.ToString(), a.ToString()));

            // Seleccionar el año más reciente del presupuesto
            cboAnio.SelectedIndex = cboAnio.Items.Count - 1;

            // Seleccionar el mes de inicio si hay un solo año, o mes actual
            if (pre.AnioInicio == pre.AnioFin)
                cboMes.SelectedIndex = pre.MesInicio - 1;

            CargarTransacciones();
        }

        // ── Cargar transacciones según filtros ─────────────
        private void CargarTransacciones()
        {
            lblError.Visible = false;

            if (cboPresupuesto.SelectedItem == null) return;
            var idPre = ((ComboItem)cboPresupuesto.SelectedItem).Value;
            if (string.IsNullOrEmpty(idPre)) return;

            if (cboAnio.SelectedItem == null || cboMes.SelectedItem == null) return;

            int anio = int.Parse(((ComboItem)cboAnio.SelectedItem).Value);
            int mes  = int.Parse(((ComboItem)cboMes.SelectedItem).Value);

            try
            {
                string tipo = ((ComboItem)cboTipo.SelectedItem).Value;

                if (mes == 0)
                {
                    // Todos los meses del año — recorre mes a mes y acumula
                    _transacciones = new System.Collections.Generic.List<Transaccion>();
                    for (int m = 1; m <= 12; m++)
                    {
                        var parcial = string.IsNullOrEmpty(tipo)
                            ? _dao.ObtenerPorMes(idPre, anio, m)
                            : _dao.ObtenerPorTipo(idPre, anio, m, tipo);
                        _transacciones.AddRange(parcial);
                    }
                    // Ordenar por fecha descendente
                    _transacciones.Sort((a, b) => b.FechaTransaccion.CompareTo(a.FechaTransaccion));
                }
                else
                {
                    _transacciones = string.IsNullOrEmpty(tipo)
                        ? _dao.ObtenerPorMes(idPre, anio, mes)
                        : _dao.ObtenerPorTipo(idPre, anio, mes, tipo);
                }

                RefrescarGrid();
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                MostrarError("Error cargando transacciones: " + ex.Message);
            }
        }

        private void RefrescarGrid()
        {
            dgvTransacciones.Rows.Clear();
            foreach (var t in _transacciones)
            {
                dgvTransacciones.Rows.Add(
                    t.IdTransaccion,
                    t.FechaTransaccion.ToString("dd/MM/yyyy"),
                    t.Tipo,
                    t.NombreCategoria,
                    t.NombreSubcategoria,
                    t.Descripcion,
                    t.Monto.ToString("N2"),
                    t.MetodoPago,
                    t.NumFactura ?? ""
                );
            }
        }

        private void ActualizarTotales()
        {
            decimal ing = 0, gas = 0, aho = 0;
            foreach (var t in _transacciones)
            {
                if (t.Tipo == "ingreso") ing += t.Monto;
                else if (t.Tipo == "gasto") gas += t.Monto;
                else aho += t.Monto;
            }
            decimal balance = ing - gas;

            lblTotalIng.Text   = $"L {ing:N2}";
            lblTotalGas.Text   = $"L {gas:N2}";
            lblTotalAho.Text   = $"L {aho:N2}";
            lblBalance.Text    = $"L {balance:N2}";
            lblBalance.ForeColor = balance >= 0 ? COLOR_ING : COLOR_GAS;
        }

        // ── Transacción seleccionada ───────────────────────
        private Transaccion ObtenerSeleccionada()
        {
            if (dgvTransacciones.SelectedRows.Count == 0) return null;
            int idx = dgvTransacciones.SelectedRows[0].Index;
            return idx >= 0 && idx < _transacciones.Count ? _transacciones[idx] : null;
        }

        // ── NUEVA ──────────────────────────────────────────
        private void BtnNueva_Click(object sender, EventArgs e)
        {
            if (cboPresupuesto.SelectedItem == null) return;
            var idPre = ((ComboItem)cboPresupuesto.SelectedItem).Value;
            if (string.IsNullOrEmpty(idPre)) { MostrarError("Selecciona un presupuesto primero."); return; }

            int anio = cboAnio.SelectedItem != null ? int.Parse(((ComboItem)cboAnio.SelectedItem).Value) : DateTime.Now.Year;
            int mes  = cboMes.SelectedItem  != null ? int.Parse(((ComboItem)cboMes.SelectedItem).Value)  : DateTime.Now.Month;

            // Si mes=0 (Todos), usar el mes actual
            if (mes == 0) mes = DateTime.Now.Month;

            var dlg = new FrmTransaccionDetalle(_usuario, idPre, anio, mes, null);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarTransacciones();
        }

        // ── EDITAR ─────────────────────────────────────────
        private void BtnEditar_Click(object sender, EventArgs e)
        {
            var t = ObtenerSeleccionada();
            if (t == null) { MostrarError("Selecciona una transacción para editar."); return; }

            var dlg = new FrmTransaccionDetalle(_usuario, t.IdPresupuesto, t.Anio, t.Mes, t);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarTransacciones();
        }

        // ── ELIMINAR ───────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var t = ObtenerSeleccionada();
            if (t == null) { MostrarError("Selecciona una transacción para eliminar."); return; }

            var confirm = MessageBox.Show(
                $"¿Eliminar la transacción '{t.Descripcion}' por L {t.Monto:N2}?\nEsta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _dao.Eliminar(t.IdTransaccion);
                CargarTransacciones();
            }
            catch (Exception ex)
            {
                MostrarError("Error al eliminar: " + ex.Message);
            }
        }

        private void MostrarError(string msg)
        {
            lblError.Text    = "⚠  " + msg;
            lblError.Visible = true;
        }
    }

    // ── ComboItem helper ───────────────────────────────────
    public class ComboItem
    {
        public string Value { get; }
        public string Display { get; }
        public ComboItem(string value, string display) { Value = value; Display = display; }
        public override string ToString() => Display;
    }
}
