using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmObligaciones : Form
    {
        // ── Colores ────────────────────────────────────────
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_CARD      = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color COLOR_OK     = Color.FromArgb(63,  185, 120);   // pagada / lejos
        private readonly Color COLOR_WARN   = Color.FromArgb(251, 191,  36);   // 3+ días
        private readonly Color COLOR_URG    = Color.FromArgb(251, 146,  60);   // <3 días
        private readonly Color COLOR_VENC   = Color.FromArgb(248,  81,  73);   // vencida

        // ── DAOs ───────────────────────────────────────────
        private readonly ObligacionDAO _dao    = new ObligacionDAO();
        private readonly CategoriaDAO  _catDAO = new CategoriaDAO();

        // ── Estado ─────────────────────────────────────────
        private readonly Usuario _usuario;
        private List<ObligacionFija> _obligaciones = new List<ObligacionFija>();

        // ── Controles ──────────────────────────────────────
        private DataGridView dgvObligaciones;
        private Button       btnNueva, btnEditar, btnDesactivar;
        private Label        lblResumen;
        private Label        lblError;

        public FrmObligaciones(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarObligaciones();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Obligaciones Fijas";
            this.Size            = new Size(1050, 640);
            this.MinimumSize     = new Size(1050, 640);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── HEADER ─────────────────────────────────────
            var pnlHeader = new Panel { Size = new Size(1050, 60), Location = new Point(0, 0), BackColor = BG_PANEL };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1)) e.Graphics.DrawLine(pen, 0, 59, 1050, 59);
                using (var b = new SolidBrush(COLOR_WARN)) e.Graphics.FillRectangle(b, 0, 0, 4, 60);
            };
            this.Controls.Add(pnlHeader);
            pnlHeader.Controls.Add(new Label { Text = "Obligaciones Fijas", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(18, 16) });
            pnlHeader.Controls.Add(new Label { Text = "Pagos recurrentes mensuales — servicios, deudas, seguros", Font = new Font("Segoe UI", 9f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(225, 22) });

            // ── LEYENDA DE COLORES ─────────────────────────
            var pnlLeyenda = new Panel { Size = new Size(1050, 38), Location = new Point(0, 60), BackColor = BG_CARD };
            pnlLeyenda.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1)) e.Graphics.DrawLine(pen, 0, 37, 1050, 37);
            };
            this.Controls.Add(pnlLeyenda);

            int lx = 15;
            AgregarLeyenda(pnlLeyenda, ref lx, COLOR_OK,   "Al día (>7 días)");
            AgregarLeyenda(pnlLeyenda, ref lx, COLOR_WARN, "Próxima (3-7 días)");
            AgregarLeyenda(pnlLeyenda, ref lx, COLOR_URG,  "Urgente (<3 días)");
            AgregarLeyenda(pnlLeyenda, ref lx, COLOR_VENC, "Vencida");

            // ── RESUMEN ────────────────────────────────────
            lblResumen = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(580, 10)
            };
            pnlLeyenda.Controls.Add(lblResumen);

            // ── BOTONES ACCIÓN ─────────────────────────────
            var pnlAcciones = new Panel { Size = new Size(1050, 42), Location = new Point(0, 98), BackColor = BG_DARK };
            this.Controls.Add(pnlAcciones);

            btnNueva = CrearBtn(pnlAcciones, "+ Nueva Obligación", 10, 6, 160, ACCENT, Color.FromArgb(10, 12, 18));
            btnNueva.Click += (s, e) => AbrirDialog(null);

            btnEditar = CrearBtn(pnlAcciones, "✎ Editar", 180, 6, 100, BG_CARD, TEXT_PRIMARY);
            btnEditar.FlatAppearance.BorderColor = BORDER;
            btnEditar.Click += BtnEditar_Click;

            btnDesactivar = CrearBtn(pnlAcciones, "⊘ Desactivar", 290, 6, 120, BG_CARD, COLOR_VENC);
            btnDesactivar.FlatAppearance.BorderColor = COLOR_VENC;
            btnDesactivar.Click += BtnDesactivar_Click;

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 9f), ForeColor = COLOR_VENC, AutoSize = true, Location = new Point(425, 14), Visible = false };
            pnlAcciones.Controls.Add(lblError);

            // ── GRID ───────────────────────────────────────
            dgvObligaciones = new DataGridView
            {
                Size              = new Size(1030, 460),
                Location          = new Point(10, 148),
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
            dgvObligaciones.DefaultCellStyle.BackColor          = BG_CARD;
            dgvObligaciones.DefaultCellStyle.ForeColor          = TEXT_PRIMARY;
            dgvObligaciones.DefaultCellStyle.SelectionBackColor = Color.FromArgb(38, 46, 60);
            dgvObligaciones.DefaultCellStyle.SelectionForeColor = ACCENT;
            dgvObligaciones.ColumnHeadersDefaultCellStyle.BackColor = BG_PANEL;
            dgvObligaciones.ColumnHeadersDefaultCellStyle.ForeColor = TEXT_MUTED;
            dgvObligaciones.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            dgvObligaciones.ColumnHeadersBorderStyle    = DataGridViewHeaderBorderStyle.None;
            dgvObligaciones.EnableHeadersVisualStyles   = false;
            dgvObligaciones.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(25, 32, 42);
            dgvObligaciones.RowTemplate.Height = 34;

            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",       Visible = false });
            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEstado",   HeaderText = "",              Width = 12, FillWeight = 2  });
            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombre",   HeaderText = "OBLIGACIÓN",    FillWeight = 22 });
            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCategoria",HeaderText = "CATEGORÍA",     FillWeight = 15 });
            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSubcat",   HeaderText = "SUBCATEGORÍA",  FillWeight = 15 });
            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMonto",    HeaderText = "MONTO (L)",     FillWeight = 12,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDia",      HeaderText = "DÍA VENCE",     FillWeight = 10,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDias",     HeaderText = "DÍAS RESTANTES",FillWeight = 13,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            dgvObligaciones.Columns.Add(new DataGridViewTextBoxColumn { Name = "colInicio",   HeaderText = "DESDE",         FillWeight = 11 });

            dgvObligaciones.CellFormatting += DgvObligaciones_CellFormatting;
            this.Controls.Add(dgvObligaciones);
        }

        // ── Agrega ítem de leyenda ─────────────────────────
        private void AgregarLeyenda(Panel panel, ref int x, Color color, string texto)
        {
            var dot = new Label { Size = new Size(12, 12), Location = new Point(x, 13), BackColor = color };
            panel.Controls.Add(dot);
            x += 16;
            panel.Controls.Add(new Label { Text = texto, Font = new Font("Segoe UI", 8.5f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(x, 10) });
            x += TextRenderer.MeasureText(texto, new Font("Segoe UI", 8.5f)).Width + 20;
        }

        private Button CrearBtn(Panel panel, string txt, int x, int y, int w, Color back, Color fore)
        {
            var btn = new Button { Text = txt, Size = new Size(w, 30), Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            panel.Controls.Add(btn);
            return btn;
        }

        // ── Color por urgencia ─────────────────────────────
        private Color ColorPorDias(int dias)
        {
            if (dias < 0)  return COLOR_VENC;
            if (dias <= 3) return COLOR_URG;
            if (dias <= 7) return COLOR_WARN;
            return COLOR_OK;
        }

        private void DgvObligaciones_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _obligaciones.Count) return;
            var obl = _obligaciones[e.RowIndex];
            Color color = ColorPorDias(obl.DiasVencimiento);

            string col = dgvObligaciones.Columns[e.ColumnIndex].Name;
            if (col == "colEstado") { e.CellStyle.BackColor = color; e.Value = ""; e.FormattingApplied = true; }
            if (col == "colNombre") { e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold); }
            if (col == "colMonto")  { e.CellStyle.ForeColor = color; }
            if (col == "colDias")
            {
                e.CellStyle.ForeColor = color;
                e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                if (obl.DiasVencimiento < 0)
                    e.Value = $"Vencida ({Math.Abs(obl.DiasVencimiento)}d)";
                else if (obl.DiasVencimiento == 0)
                    e.Value = "¡HOY!";
                else
                    e.Value = $"{obl.DiasVencimiento} días";
                e.FormattingApplied = true;
            }
        }

        // ── Cargar obligaciones ────────────────────────────
        private void CargarObligaciones()
        {
            lblError.Visible = false;
            try
            {
                _obligaciones = _dao.ObtenerActivas(_usuario.IdUser);
                dgvObligaciones.Rows.Clear();

                decimal totalMensual = 0;
                foreach (var o in _obligaciones)
                {
                    totalMensual += o.Monto;
                    dgvObligaciones.Rows.Add(
                        o.IdObligacion,
                        "",
                        o.Nombre,
                        o.NombreCategoria,
                        o.NombreSubcategoria,
                        o.Monto.ToString("N2"),
                        $"Día {o.Dia}",
                        o.DiasVencimiento,
                        o.FechaInicio.ToString("dd/MM/yyyy")
                    );
                }

                int urgentes = _obligaciones.FindAll(o => o.DiasVencimiento <= 3 && o.DiasVencimiento >= 0).Count;
                int vencidas = _obligaciones.FindAll(o => o.DiasVencimiento < 0).Count;

                lblResumen.Text = $"Total mensual: L {totalMensual:N2}   |   " +
                                  $"{_obligaciones.Count} activas   |   " +
                                  (urgentes > 0 ? $"⚠ {urgentes} urgentes   |   " : "") +
                                  (vencidas > 0 ? $"🔴 {vencidas} vencidas" : "✓ Al día");
            }
            catch (Exception ex) { lblError.Text = "⚠  " + ex.Message; lblError.Visible = true; }
        }

        // ── Obtener seleccionada ───────────────────────────
        private ObligacionFija ObtenerSeleccionada()
        {
            if (dgvObligaciones.SelectedRows.Count == 0) return null;
            int idx = dgvObligaciones.SelectedRows[0].Index;
            return idx >= 0 && idx < _obligaciones.Count ? _obligaciones[idx] : null;
        }

        // ── Botones ────────────────────────────────────────
        private void AbrirDialog(ObligacionFija existente)
        {
            var dlg = new FrmObligacionDialog(_usuario, existente);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarObligaciones();
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            var obl = ObtenerSeleccionada();
            if (obl == null) { lblError.Text = "⚠  Selecciona una obligación."; lblError.Visible = true; return; }
            AbrirDialog(obl);
        }

        private void BtnDesactivar_Click(object sender, EventArgs e)
        {
            var obl = ObtenerSeleccionada();
            if (obl == null) { lblError.Text = "⚠  Selecciona una obligación."; lblError.Visible = true; return; }

            var confirm = MessageBox.Show(
                $"¿Desactivar la obligación '{obl.Nombre}' de L {obl.Monto:N2}?\n\nYa no aparecerá en el listado ni generará alertas.",
                "Confirmar desactivación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;
            try { _dao.Desactivar(obl.IdObligacion); CargarObligaciones(); }
            catch (Exception ex) { lblError.Text = "⚠  " + ex.Message; lblError.Visible = true; }
        }
    }

    // ════════════════════════════════════════════════════════
    // Dialog: Nueva / Editar Obligación
    // ════════════════════════════════════════════════════════
    public class FrmObligacionDialog : Form
    {
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT     = Color.FromArgb(30,  37,  48);
        private readonly Color BG_INPUT_HOV = Color.FromArgb(38,  46,  60);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color ERROR_COLOR  = Color.FromArgb(248,  81,  73);

        private readonly ObligacionDAO _dao    = new ObligacionDAO();
        private readonly CategoriaDAO  _catDAO = new CategoriaDAO();

        private readonly Usuario        _usuario;
        private readonly ObligacionFija _existente;
        private readonly bool           _modoEdicion;

        private TextBox       txtNombre, txtDescripcion, txtMonto;
        private ComboBox      cboSubcategoria;
        private NumericUpDown nudDia;
        private DateTimePicker dtpInicio;
        private CheckBox      chkFechaFin;
        private DateTimePicker dtpFin;
        private Label         lblError;
        private Button        btnGuardar, btnCancelar;
        private Panel         pnlContenido;

        public FrmObligacionDialog(Usuario usuario, ObligacionFija existente)
        {
            _usuario     = usuario;
            _existente   = existente;
            _modoEdicion = existente != null;
            InicializarComponentes();
            CargarSubcategorias();
            if (_modoEdicion) PrecargarDatos();
        }

        private void InicializarComponentes()
        {
            this.Text            = _modoEdicion ? "Editar Obligación" : "Nueva Obligación";
            this.Size            = new Size(520, 580);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            pnlContenido = new Panel { Size = new Size(470, 520), Location = new Point(20, 15), BackColor = BG_PANEL };
            pnlContenido.Paint += (s, e) => { using (var pen = new Pen(BORDER, 1)) e.Graphics.DrawRectangle(pen, 0, 0, pnlContenido.Width - 1, pnlContenido.Height - 1); };
            this.Controls.Add(pnlContenido);

            pnlContenido.Controls.Add(new Label { Text = _modoEdicion ? "Editar Obligación" : "Nueva Obligación Fija", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(20, 18) });
            pnlContenido.Controls.Add(new Label { Size = new Size(430, 1), Location = new Point(20, 45), BackColor = BORDER });

            int y = 55;

            // Nombre
            AgregarLabel("NOMBRE *", 20, y);
            txtNombre = AgregarTxt(20, y + 18, 430);
            y += 68;

            // Subcategoría (solo gastos según el documento)
            AgregarLabel("SUBCATEGORÍA (solo tipo gasto) *", 20, y);
            cboSubcategoria = new ComboBox { Size = new Size(430, 34), Location = new Point(20, y + 18), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            pnlContenido.Controls.Add(cboSubcategoria);
            y += 68;

            // Descripción
            AgregarLabel("DESCRIPCIÓN", 20, y);
            txtDescripcion = AgregarTxt(20, y + 18, 430);
            y += 68;

            // Monto / Día
            AgregarLabel("MONTO MENSUAL (L) *", 20, y);
            txtMonto = AgregarTxt(20, y + 18, 200);

            AgregarLabel("DÍA DE VENCIMIENTO *", 240, y);
            nudDia = new NumericUpDown
            {
                Size      = new Size(80, 34),
                Location  = new Point(240, y + 18),
                Minimum   = 1,
                Maximum   = 31,
                Value     = 1,
                BackColor = BG_INPUT,
                ForeColor = TEXT_PRIMARY,
                Font      = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlContenido.Controls.Add(nudDia);
            pnlContenido.Controls.Add(new Label { Text = "(1-31)", Font = new Font("Segoe UI", 8.5f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(328, y + 26) });
            y += 68;

            // Fecha inicio
            AgregarLabel("FECHA DE INICIO *", 20, y);
            dtpInicio = new DateTimePicker { Size = new Size(200, 34), Location = new Point(20, y + 18), Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = new Font("Segoe UI", 10f) };
            pnlContenido.Controls.Add(dtpInicio);
            y += 68;

            // Fecha fin (opcional)
            chkFechaFin = new CheckBox
            {
                Text      = "Tiene fecha de finalización",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(20, y),
                BackColor = Color.Transparent
            };
            chkFechaFin.CheckedChanged += (s, e) => dtpFin.Enabled = chkFechaFin.Checked;
            pnlContenido.Controls.Add(chkFechaFin);

            dtpFin = new DateTimePicker { Size = new Size(200, 34), Location = new Point(260, y - 3), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddYears(1), Enabled = false, Font = new Font("Segoe UI", 10f) };
            pnlContenido.Controls.Add(dtpFin);
            y += 40;

            // Error
            lblError = new Label { Text = "", Font = new Font("Segoe UI", 9f), ForeColor = ERROR_COLOR, AutoSize = false, Size = new Size(430, 20), Location = new Point(20, y), Visible = false };
            pnlContenido.Controls.Add(lblError);
            y += 28;

            // Botones
            btnCancelar = new Button { Text = "Cancelar", Size = new Size(130, 38), Location = new Point(20, y), FlatStyle = FlatStyle.Flat, BackColor = BG_INPUT, ForeColor = TEXT_MUTED, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f) };
            btnCancelar.FlatAppearance.BorderColor = BORDER;
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            pnlContenido.Controls.Add(btnCancelar);

            btnGuardar = new Button { Text = _modoEdicion ? "GUARDAR CAMBIOS" : "CREAR OBLIGACIÓN", Size = new Size(290, 38), Location = new Point(160, y), FlatStyle = FlatStyle.Flat, BackColor = ACCENT, ForeColor = Color.FromArgb(10, 12, 18), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            pnlContenido.Controls.Add(btnGuardar);
        }

        private void AgregarLabel(string txt, int x, int y) =>
            pnlContenido.Controls.Add(new Label { Text = txt, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(x, y) });

        private TextBox AgregarTxt(int x, int y, int w)
        {
            var t = new TextBox { Size = new Size(w, 34), Location = new Point(x, y), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10.5f) };
            t.GotFocus  += (s, e) => ((TextBox)s).BackColor = BG_INPUT_HOV;
            t.LostFocus += (s, e) => ((TextBox)s).BackColor = BG_INPUT;
            pnlContenido.Controls.Add(t);
            return t;
        }

        private void CargarSubcategorias()
        {
            // Solo subcategorías de tipo "gasto" — el documento lo requiere
            var subs = _catDAO.ObtenerTodasSubcategorias();
            cboSubcategoria.Items.Clear();
            foreach (var s in subs)
                if (s.TipoCategoria == "gasto")
                    cboSubcategoria.Items.Add(new ComboItem(s.IdSubcategoria, $"{s.NombreCategoria} › {s.Nombre}"));
            if (cboSubcategoria.Items.Count > 0)
                cboSubcategoria.SelectedIndex = 0;
        }

        private void PrecargarDatos()
        {
            var o = _existente;
            txtNombre.Text      = o.Nombre;
            txtDescripcion.Text = o.Descripcion ?? "";
            txtMonto.Text       = o.Monto.ToString("N2");
            nudDia.Value        = Math.Min(o.Dia, 31);
            dtpInicio.Value     = o.FechaInicio;

            if (o.FechaFin.HasValue)
            {
                chkFechaFin.Checked = true;
                dtpFin.Value        = o.FechaFin.Value;
                dtpFin.Enabled      = true;
            }

            for (int i = 0; i < cboSubcategoria.Items.Count; i++)
                if (((ComboItem)cboSubcategoria.Items[i]).Value == o.IdSubcategoria)
                { cboSubcategoria.SelectedIndex = i; break; }

            // En edición no se cambia la subcategoría
            cboSubcategoria.Enabled = false;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            { lblError.Text = "⚠  El nombre es obligatorio."; lblError.Visible = true; return; }

            if (cboSubcategoria.SelectedItem == null)
            { lblError.Text = "⚠  Selecciona una subcategoría."; lblError.Visible = true; return; }

            if (!decimal.TryParse(txtMonto.Text.Replace(",", ""), out decimal monto) || monto <= 0)
            { lblError.Text = "⚠  El monto debe ser mayor a 0."; lblError.Visible = true; return; }

            if (chkFechaFin.Checked && dtpFin.Value <= dtpInicio.Value)
            { lblError.Text = "⚠  La fecha de fin debe ser posterior a la de inicio."; lblError.Visible = true; return; }

            try
            {
                btnGuardar.Enabled = false;

                if (_modoEdicion)
                {
                    _existente.Nombre      = txtNombre.Text.Trim();
                    _existente.Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim();
                    _existente.Monto       = monto;
                    _existente.Dia         = (int)nudDia.Value;
                    _dao.Actualizar(_existente);
                }
                else
                {
                    var obl = new ObligacionFija
                    {
                        IdUser         = _usuario.IdUser,
                        IdSubcategoria = ((ComboItem)cboSubcategoria.SelectedItem).Value,
                        Nombre         = txtNombre.Text.Trim(),
                        Descripcion    = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
                        Monto          = monto,
                        Dia            = (int)nudDia.Value,
                        FechaInicio    = dtpInicio.Value.Date,
                        FechaFin       = chkFechaFin.Checked ? (DateTime?)dtpFin.Value.Date : null
                    };
                    _dao.Insertar(obl);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Contains("ORA-20") ? ex.Message.Substring(ex.Message.IndexOf("ORA-20")) : ex.Message;
                lblError.Text    = "⚠  " + msg;
                lblError.Visible = true;
            }
            finally { btnGuardar.Enabled = true; }
        }
    }
}
