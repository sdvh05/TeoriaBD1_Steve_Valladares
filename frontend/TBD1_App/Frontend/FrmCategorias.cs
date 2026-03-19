using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmCategorias : Form
    {
        // ── Colores ────────────────────────────────────────
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_CARD      = Color.FromArgb(30,  37,  48);
        private readonly Color BG_CARD_SEL  = Color.FromArgb(38,  46,  60);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color COLOR_ING    = Color.FromArgb(63,  185, 120);
        private readonly Color COLOR_GAS    = Color.FromArgb(248,  81,  73);
        private readonly Color COLOR_AHO    = Color.FromArgb(251, 191,  36);

        // ── DAO ────────────────────────────────────────────
        private readonly CategoriaDAO _dao = new CategoriaDAO();

        // ── Estado ─────────────────────────────────────────
        private readonly Usuario _usuario;
        private List<Categoria>    _categorias    = new List<Categoria>();
        private List<Subcategoria> _subcategorias = new List<Subcategoria>();
        private Categoria _catSeleccionada = null;

        // ── Panel izquierdo — categorías ───────────────────
        private ListBox lstCategorias;
        private Button  btnNuevaCat, btnEditarCat, btnEliminarCat;
        private ComboBox cboFiltroTipo;

        // ── Panel derecho — subcategorías ──────────────────
        private DataGridView dgvSubcategorias;
        private Button  btnNuevaSub, btnEditarSub, btnEliminarSub;
        private Label   lblNombreCat, lblTipoCat, lblDescCat;
        private Label   lblSinSeleccion;
        private Label   lblError;

        public FrmCategorias(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarCategorias();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Categorías y Subcategorías";
            this.Size            = new Size(1050, 650);
            this.MinimumSize     = new Size(1050, 650);
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
                using (var b = new SolidBrush(Color.FromArgb(167, 139, 250))) e.Graphics.FillRectangle(b, 0, 0, 4, 60);
            };
            this.Controls.Add(pnlHeader);
            pnlHeader.Controls.Add(new Label { Text = "Categorías", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(18, 16) });
            pnlHeader.Controls.Add(new Label { Text = "Administra las categorías y subcategorías del sistema", Font = new Font("Segoe UI", 9f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(178, 22) });

            // ── PANEL IZQUIERDO — lista categorías ─────────
            var pnlLista = new Panel { Size = new Size(290, 580), Location = new Point(0, 60), BackColor = BG_PANEL };
            pnlLista.Paint += (s, e) => { using (var pen = new Pen(BORDER, 1)) e.Graphics.DrawLine(pen, 289, 0, 289, 580); };
            this.Controls.Add(pnlLista);

            pnlLista.Controls.Add(new Label { Text = "CATEGORÍAS", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(15, 12) });

            // Filtro tipo
            cboFiltroTipo = new ComboBox { Size = new Size(160, 26), Location = new Point(15, 32), BackColor = BG_CARD, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9f) };
            cboFiltroTipo.Items.Add(new ComboItem("", "Todos los tipos"));
            cboFiltroTipo.Items.Add(new ComboItem("ingreso", "💰 Ingresos"));
            cboFiltroTipo.Items.Add(new ComboItem("gasto",   "💸 Gastos"));
            cboFiltroTipo.Items.Add(new ComboItem("ahorro",  "🏦 Ahorros"));
            cboFiltroTipo.SelectedIndex = 0;
            cboFiltroTipo.SelectedIndexChanged += (s, e) => CargarCategorias();
            pnlLista.Controls.Add(cboFiltroTipo);

            lstCategorias = new ListBox
            {
                Size          = new Size(270, 440),
                Location      = new Point(10, 65),
                BackColor     = BG_CARD,
                ForeColor     = TEXT_PRIMARY,
                BorderStyle   = BorderStyle.None,
                Font          = new Font("Segoe UI", 10f),
                DrawMode      = DrawMode.OwnerDrawFixed,
                ItemHeight    = 52,
                SelectionMode = SelectionMode.One
            };
            lstCategorias.DrawItem += LstCategorias_DrawItem;
            lstCategorias.SelectedIndexChanged += LstCategorias_SelectedIndexChanged;
            pnlLista.Controls.Add(lstCategorias);

            // Botones categorías
            btnNuevaCat    = CrearBtn(pnlLista, "+ Nueva",    10,  518, 78,  ACCENT,  Color.FromArgb(10,12,18));
            btnEditarCat   = CrearBtn(pnlLista, "✎ Editar",  98,  518, 80,  BG_CARD, TEXT_PRIMARY);
            btnEliminarCat = CrearBtn(pnlLista, "✕ Borrar",  188, 518, 80,  BG_CARD, COLOR_GAS);
            btnEditarCat.FlatAppearance.BorderColor   = BORDER;
            btnEliminarCat.FlatAppearance.BorderColor = COLOR_GAS;
            btnNuevaCat.Click    += (s, e) => AbrirDialogCategoria(null);
            btnEditarCat.Click   += (s, e) => { if (_catSeleccionada != null) AbrirDialogCategoria(_catSeleccionada); };
            btnEliminarCat.Click += BtnEliminarCat_Click;

            // ── PANEL DERECHO — subcategorías ───────────────
            var pnlDer = new Panel { Size = new Size(755, 580), Location = new Point(290, 60), BackColor = BG_DARK };
            this.Controls.Add(pnlDer);

            lblSinSeleccion = new Label { Text = "← Selecciona una categoría para ver sus subcategorías", Font = new Font("Segoe UI", 11f), ForeColor = TEXT_MUTED, AutoSize = false, Size = new Size(680, 40), Location = new Point(35, 270), TextAlign = ContentAlignment.MiddleCenter };
            pnlDer.Controls.Add(lblSinSeleccion);

            lblNombreCat = new Label { Text = "", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(20, 15), Visible = false };
            pnlDer.Controls.Add(lblNombreCat);

            lblTipoCat = new Label { Text = "", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), AutoSize = true, Location = new Point(20, 45), Visible = false };
            pnlDer.Controls.Add(lblTipoCat);

            lblDescCat = new Label { Text = "", Font = new Font("Segoe UI", 9f), ForeColor = TEXT_MUTED, AutoSize = false, Size = new Size(700, 20), Location = new Point(20, 65), Visible = false };
            pnlDer.Controls.Add(lblDescCat);

            var sep = new Label { Size = new Size(715, 1), Location = new Point(20, 90), BackColor = BORDER, Visible = false, Name = "sep" };
            pnlDer.Controls.Add(sep);

            var lblSubTit = new Label { Text = "SUBCATEGORÍAS", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(20, 100), Visible = false, Name = "lblSubTit" };
            pnlDer.Controls.Add(lblSubTit);

            btnNuevaSub    = CrearBtn(pnlDer, "+ Nueva Sub",  20,  122, 110, ACCENT,  Color.FromArgb(10,12,18));
            btnEditarSub   = CrearBtn(pnlDer, "✎ Editar",    140, 122, 90,  BG_CARD, TEXT_PRIMARY);
            btnEliminarSub = CrearBtn(pnlDer, "✕ Desactivar",240, 122, 110, BG_CARD, COLOR_GAS);
            btnEditarSub.FlatAppearance.BorderColor   = BORDER;
            btnEliminarSub.FlatAppearance.BorderColor = COLOR_GAS;
            btnNuevaSub.Visible = btnEditarSub.Visible = btnEliminarSub.Visible = false;
            btnNuevaSub.Click    += (s, e) => AbrirDialogSubcategoria(null);
            btnEditarSub.Click   += BtnEditarSub_Click;
            btnEliminarSub.Click += BtnEliminarSub_Click;

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 9f), ForeColor = COLOR_GAS, AutoSize = true, Location = new Point(360, 130), Visible = false };
            pnlDer.Controls.Add(lblError);

            // Grid subcategorías
            dgvSubcategorias = new DataGridView
            {
                Size = new Size(715, 420), Location = new Point(20, 160),
                BackgroundColor = BG_CARD, BorderStyle = BorderStyle.None, GridColor = BORDER,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9.5f), Visible = false
            };
            dgvSubcategorias.DefaultCellStyle.BackColor          = BG_CARD;
            dgvSubcategorias.DefaultCellStyle.ForeColor          = TEXT_PRIMARY;
            dgvSubcategorias.DefaultCellStyle.SelectionBackColor = BG_CARD_SEL;
            dgvSubcategorias.DefaultCellStyle.SelectionForeColor = ACCENT;
            dgvSubcategorias.ColumnHeadersDefaultCellStyle.BackColor = BG_PANEL;
            dgvSubcategorias.ColumnHeadersDefaultCellStyle.ForeColor = TEXT_MUTED;
            dgvSubcategorias.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            dgvSubcategorias.ColumnHeadersBorderStyle  = DataGridViewHeaderBorderStyle.None;
            dgvSubcategorias.EnableHeadersVisualStyles = false;
            dgvSubcategorias.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(25, 32, 42);
            dgvSubcategorias.RowTemplate.Height = 30;

            dgvSubcategorias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",       HeaderText = "ID",           Visible = false });
            dgvSubcategorias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNombre",   HeaderText = "NOMBRE",       FillWeight = 30 });
            dgvSubcategorias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDesc",     HeaderText = "DESCRIPCIÓN",  FillWeight = 40 });
            dgvSubcategorias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDefecto",  HeaderText = "POR DEFECTO",  FillWeight = 15 });
            dgvSubcategorias.Columns.Add(new DataGridViewTextBoxColumn { Name = "colActiva",   HeaderText = "ESTADO",       FillWeight = 15 });

            dgvSubcategorias.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (dgvSubcategorias.Columns[e.ColumnIndex].Name == "colActiva")
                {
                    string val = dgvSubcategorias.Rows[e.RowIndex].Cells["colActiva"].Value?.ToString();
                    e.CellStyle.ForeColor = val == "Activa" ? COLOR_ING : COLOR_GAS;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                if (dgvSubcategorias.Columns[e.ColumnIndex].Name == "colDefecto")
                {
                    string val = dgvSubcategorias.Rows[e.RowIndex].Cells["colDefecto"].Value?.ToString();
                    e.CellStyle.ForeColor = val == "Sí" ? ACCENT : TEXT_MUTED;
                }
            };
            pnlDer.Controls.Add(dgvSubcategorias);
        }

        // ── Draw categorías con estilo ─────────────────────
        private void LstCategorias_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _categorias.Count) return;
            var cat = _categorias[e.Index];
            bool sel = (e.State & DrawItemState.Selected) != 0;

            Color accentColor = cat.Tipo == "ingreso" ? COLOR_ING : cat.Tipo == "gasto" ? COLOR_GAS : COLOR_AHO;
            Color bg = sel ? BG_CARD_SEL : BG_CARD;

            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            e.Graphics.FillRectangle(new SolidBrush(accentColor), e.Bounds.X, e.Bounds.Y, 3, e.Bounds.Height);

            e.Graphics.DrawString(cat.Nombre, new Font("Segoe UI", 10f, FontStyle.Bold), new SolidBrush(TEXT_PRIMARY), e.Bounds.X + 12, e.Bounds.Y + 7);
            e.Graphics.DrawString(cat.Tipo.ToUpper(), new Font("Segoe UI", 7.5f, FontStyle.Bold), new SolidBrush(accentColor), e.Bounds.X + 12, e.Bounds.Y + 28);
            if (!string.IsNullOrEmpty(cat.Descripcion))
                e.Graphics.DrawString(cat.Descripcion, new Font("Segoe UI", 8f), new SolidBrush(TEXT_MUTED), e.Bounds.X + 70, e.Bounds.Y + 29);

            e.Graphics.DrawLine(new Pen(BORDER, 1), e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private Button CrearBtn(Panel panel, string txt, int x, int y, int w, Color back, Color fore)
        {
            var btn = new Button { Text = txt, Size = new Size(w, 28), Location = new Point(x, y), FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            panel.Controls.Add(btn);
            return btn;
        }

        // ── Cargar lista de categorías ─────────────────────
        private void CargarCategorias()
        {
            try
            {
                string tipo = ((ComboItem)cboFiltroTipo.SelectedItem).Value;
                _categorias = string.IsNullOrEmpty(tipo) ? _dao.ObtenerTodas() : _dao.ObtenerPorTipo(tipo);

                lstCategorias.Items.Clear();
                foreach (var c in _categorias)
                    lstCategorias.Items.Add(c.Nombre);

                if (_categorias.Count > 0)
                    lstCategorias.SelectedIndex = 0;
                else
                    MostrarDetalle(false);
            }
            catch (Exception ex) { MostrarError("Error cargando categorías: " + ex.Message); }
        }

        // ── Al seleccionar categoría ───────────────────────
        private void LstCategorias_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstCategorias.SelectedIndex;
            if (idx < 0 || idx >= _categorias.Count) return;
            _catSeleccionada = _categorias[idx];
            MostrarDetalle(true);
            CargarSubcategorias();
        }

        private void MostrarDetalle(bool mostrar)
        {
            lblSinSeleccion.Visible = !mostrar;
            lblNombreCat.Visible = lblTipoCat.Visible = lblDescCat.Visible = mostrar;
            dgvSubcategorias.Visible = btnNuevaSub.Visible = btnEditarSub.Visible = btnEliminarSub.Visible = mostrar;
            foreach (Control c in dgvSubcategorias.Parent.Controls)
                if (c.Name == "sep" || c.Name == "lblSubTit") c.Visible = mostrar;
        }

        private void CargarSubcategorias()
        {
            if (_catSeleccionada == null) return;
            lblError.Visible = false;

            Color accentColor = _catSeleccionada.Tipo == "ingreso" ? COLOR_ING : _catSeleccionada.Tipo == "gasto" ? COLOR_GAS : COLOR_AHO;
            lblNombreCat.Text    = _catSeleccionada.Nombre;
            lblTipoCat.Text      = _catSeleccionada.Tipo.ToUpper();
            lblTipoCat.ForeColor = accentColor;
            lblDescCat.Text      = _catSeleccionada.Descripcion ?? "Sin descripción";

            try
            {
                _subcategorias = _dao.ObtenerSubcategorias(_catSeleccionada.IdCategoria);
                dgvSubcategorias.Rows.Clear();
                foreach (var s in _subcategorias)
                {
                    dgvSubcategorias.Rows.Add(
                        s.IdSubcategoria,
                        s.Nombre,
                        s.Descripcion ?? "",
                        s.SubPorDefecto == 1 ? "Sí" : "No",
                        s.Activa == 1 ? "Activa" : "Inactiva"
                    );
                }
            }
            catch (Exception ex) { MostrarError("Error cargando subcategorías: " + ex.Message); }
        }

        // ── Dialogs ────────────────────────────────────────
        private void AbrirDialogCategoria(Categoria existente)
        {
            var dlg = new FrmCategoriaDialog(existente);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarCategorias();
        }

        private void AbrirDialogSubcategoria(Subcategoria existente)
        {
            if (_catSeleccionada == null) return;
            var dlg = new FrmSubcategoriaDialog(_catSeleccionada, existente);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarSubcategorias();
        }

        private void BtnEliminarCat_Click(object sender, EventArgs e)
        {
            if (_catSeleccionada == null) { MostrarError("Selecciona una categoría."); return; }
            var confirm = MessageBox.Show($"¿Eliminar la categoría '{_catSeleccionada.Nombre}'?\nSolo es posible si no tiene subcategorías adicionales activas.", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            try { _dao.Eliminar(_catSeleccionada.IdCategoria); CargarCategorias(); }
            catch (Exception ex) { MostrarError("Error: " + ex.Message); }
        }

        private void BtnEditarSub_Click(object sender, EventArgs e)
        {
            if (dgvSubcategorias.SelectedRows.Count == 0) { MostrarError("Selecciona una subcategoría."); return; }
            string id = dgvSubcategorias.SelectedRows[0].Cells["colId"].Value.ToString();
            var sub = _subcategorias.Find(s => s.IdSubcategoria == id);
            if (sub != null) AbrirDialogSubcategoria(sub);
        }

        private void BtnEliminarSub_Click(object sender, EventArgs e)
        {
            if (dgvSubcategorias.SelectedRows.Count == 0) { MostrarError("Selecciona una subcategoría."); return; }
            string id     = dgvSubcategorias.SelectedRows[0].Cells["colId"].Value.ToString();
            string nombre = dgvSubcategorias.SelectedRows[0].Cells["colNombre"].Value.ToString();
            var confirm = MessageBox.Show($"¿Desactivar la subcategoría '{nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
            try { _dao.EliminarSubcategoria(id); CargarSubcategorias(); }
            catch (Exception ex) { MostrarError("Error: " + ex.Message); }
        }

        private void MostrarError(string msg) { lblError.Text = "⚠  " + msg; lblError.Visible = true; }
    }

    // ════════════════════════════════════════════════════════
    // Dialog: Nueva / Editar Categoría
    // ════════════════════════════════════════════════════════
    public class FrmCategoriaDialog : Form
    {
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT     = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color ERROR_COLOR  = Color.FromArgb(248,  81,  73);

        private readonly CategoriaDAO _dao = new CategoriaDAO();
        private readonly Categoria    _existente;
        private readonly bool         _modoEdicion;

        private TextBox  txtNombre, txtDescripcion;
        private ComboBox cboTipo;
        private Label    lblError;
        private Button   btnGuardar, btnCancelar;
        private Panel    pnlContenido;

        public FrmCategoriaDialog(Categoria existente)
        {
            _existente   = existente;
            _modoEdicion = existente != null;
            InicializarComponentes();
            if (_modoEdicion) PrecargarDatos();
        }

        private void InicializarComponentes()
        {
            this.Text = _modoEdicion ? "Editar Categoría" : "Nueva Categoría";
            this.Size = new Size(420, 340); this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BG_DARK; this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.5f);

            pnlContenido = new Panel { Size = new Size(380, 280), Location = new Point(20, 15), BackColor = BG_PANEL };
            pnlContenido.Paint += (s, e) => { using (var pen = new Pen(BORDER, 1)) e.Graphics.DrawRectangle(pen, 0, 0, pnlContenido.Width - 1, pnlContenido.Height - 1); };
            this.Controls.Add(pnlContenido);

            pnlContenido.Controls.Add(new Label { Text = _modoEdicion ? "Editar Categoría" : "Nueva Categoría", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(20, 18) });
            pnlContenido.Controls.Add(new Label { Size = new Size(320, 1), Location = new Point(20, 45), BackColor = BORDER });

            AgregarLabel("NOMBRE *", 20, 55); txtNombre = AgregarTxt(20, 73, 320);
            AgregarLabel("DESCRIPCIÓN", 20, 120); txtDescripcion = AgregarTxt(20, 138, 320);

            AgregarLabel("TIPO *", 20, 185);
            cboTipo = new ComboBox { Size = new Size(180, 34), Location = new Point(20, 203), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            cboTipo.Items.Add(new ComboItem("ingreso", "💰 Ingreso"));
            cboTipo.Items.Add(new ComboItem("gasto",   "💸 Gasto"));
            cboTipo.Items.Add(new ComboItem("ahorro",  "🏦 Ahorro"));
            cboTipo.SelectedIndex = 0;
            cboTipo.Enabled = !_modoEdicion; // No se puede cambiar el tipo
            pnlContenido.Controls.Add(cboTipo);

            if (_modoEdicion)
                pnlContenido.Controls.Add(new Label { Text = "El tipo no se puede cambiar", Font = new Font("Segoe UI", 8f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(210, 212) });

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = ERROR_COLOR, AutoSize = true, Location = new Point(20, 243), Visible = false };
            pnlContenido.Controls.Add(lblError);

            btnCancelar = new Button { Text = "Cancelar", Size = new Size(110, 34), Location = new Point(20, 235), FlatStyle = FlatStyle.Flat, BackColor = BG_INPUT, ForeColor = TEXT_MUTED, Cursor = Cursors.Hand };
            btnCancelar.FlatAppearance.BorderColor = BORDER; btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            pnlContenido.Controls.Add(btnCancelar);

            btnGuardar = new Button { Text = _modoEdicion ? "GUARDAR" : "CREAR", Size = new Size(210, 34), Location = new Point(140, 235), FlatStyle = FlatStyle.Flat, BackColor = ACCENT, ForeColor = Color.FromArgb(10, 12, 18), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            btnGuardar.FlatAppearance.BorderSize = 0; btnGuardar.Click += BtnGuardar_Click;
            pnlContenido.Controls.Add(btnGuardar);
        }

        private void AgregarLabel(string txt, int x, int y) => pnlContenido.Controls.Add(new Label { Text = txt, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(x, y) });

        private TextBox AgregarTxt(int x, int y, int w)
        {
            var t = new TextBox { Size = new Size(w, 34), Location = new Point(x, y), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10.5f) };
            t.GotFocus += (s, e) => ((TextBox)s).BackColor = Color.FromArgb(38, 46, 60);
            t.LostFocus += (s, e) => ((TextBox)s).BackColor = BG_INPUT;
            pnlContenido.Controls.Add(t); return t;
        }

        private void PrecargarDatos()
        {
            txtNombre.Text      = _existente.Nombre;
            txtDescripcion.Text = _existente.Descripcion ?? "";
            for (int i = 0; i < cboTipo.Items.Count; i++)
                if (((ComboItem)cboTipo.Items[i]).Value == _existente.Tipo) { cboTipo.SelectedIndex = i; break; }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { lblError.Text = "⚠  El nombre es obligatorio."; lblError.Visible = true; return; }
            try
            {
                btnGuardar.Enabled = false;
                if (_modoEdicion)
                {
                    _existente.Nombre      = txtNombre.Text.Trim();
                    _existente.Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim();
                    _dao.Actualizar(_existente);
                }
                else
                {
                    _dao.Insertar(new Categoria
                    {
                        Nombre      = txtNombre.Text.Trim(),
                        Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
                        Tipo        = ((ComboItem)cboTipo.SelectedItem).Value,
                        Orden       = 0
                    });
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { lblError.Text = "⚠  " + ex.Message; lblError.Visible = true; }
            finally { btnGuardar.Enabled = true; }
        }
    }

    // ════════════════════════════════════════════════════════
    // Dialog: Nueva / Editar Subcategoría
    // ════════════════════════════════════════════════════════
    public class FrmSubcategoriaDialog : Form
    {
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT     = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color ERROR_COLOR  = Color.FromArgb(248,  81,  73);

        private readonly CategoriaDAO  _dao = new CategoriaDAO();
        private readonly Categoria     _categoria;
        private readonly Subcategoria  _existente;
        private readonly bool          _modoEdicion;

        private TextBox txtNombre, txtDescripcion;
        private Label   lblError;
        private Button  btnGuardar, btnCancelar;
        private Panel   pnlContenido;

        public FrmSubcategoriaDialog(Categoria categoria, Subcategoria existente)
        {
            _categoria   = categoria;
            _existente   = existente;
            _modoEdicion = existente != null;
            InicializarComponentes();
            if (_modoEdicion) { txtNombre.Text = _existente.Nombre; txtDescripcion.Text = _existente.Descripcion ?? ""; }
        }

        private void InicializarComponentes()
        {
            this.Text = _modoEdicion ? "Editar Subcategoría" : $"Nueva Subcategoría — {_categoria.Nombre}";
            this.Size = new Size(440, 360); this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BG_DARK; this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9.5f);

            pnlContenido = new Panel { Size = new Size(380, 270), Location = new Point(25, 15), BackColor = BG_PANEL };
            pnlContenido.Paint += (s, e) => { using (var pen = new Pen(BORDER, 1)) e.Graphics.DrawRectangle(pen, 0, 0, pnlContenido.Width - 1, pnlContenido.Height - 1); };
            this.Controls.Add(pnlContenido);

            pnlContenido.Controls.Add(new Label { Text = _modoEdicion ? "Editar Subcategoría" : "Nueva Subcategoría", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(20, 18) });
            pnlContenido.Controls.Add(new Label { Text = $"Categoría: {_categoria.Nombre} ({_categoria.Tipo})", Font = new Font("Segoe UI", 9f), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(20, 42) });
            pnlContenido.Controls.Add(new Label { Size = new Size(340, 1), Location = new Point(20, 58), BackColor = BORDER });

            AgregarLabel("NOMBRE *", 20, 68); txtNombre = AgregarTxt(20, 86, 340);
            AgregarLabel("DESCRIPCIÓN", 20, 133); txtDescripcion = AgregarTxt(20, 151, 340);

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = ERROR_COLOR, AutoSize = true, Location = new Point(20, 198), Visible = false };
            pnlContenido.Controls.Add(lblError);

            btnCancelar = new Button { Text = "Cancelar", Size = new Size(110, 36), Location = new Point(20, 222), FlatStyle = FlatStyle.Flat, BackColor = BG_INPUT, ForeColor = TEXT_MUTED, Cursor = Cursors.Hand };
            btnCancelar.FlatAppearance.BorderColor = BORDER; btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            pnlContenido.Controls.Add(btnCancelar);

            btnGuardar = new Button { Text = _modoEdicion ? "GUARDAR" : "CREAR", Size = new Size(228, 36), Location = new Point(140, 222), FlatStyle = FlatStyle.Flat, BackColor = ACCENT, ForeColor = Color.FromArgb(10, 12, 18), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            btnGuardar.FlatAppearance.BorderSize = 0; btnGuardar.Click += BtnGuardar_Click;
            pnlContenido.Controls.Add(btnGuardar);
        }

        private void AgregarLabel(string txt, int x, int y) => pnlContenido.Controls.Add(new Label { Text = txt, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(x, y) });

        private TextBox AgregarTxt(int x, int y, int w)
        {
            var t = new TextBox { Size = new Size(w, 34), Location = new Point(x, y), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10.5f) };
            t.GotFocus += (s, e) => ((TextBox)s).BackColor = Color.FromArgb(38, 46, 60);
            t.LostFocus += (s, e) => ((TextBox)s).BackColor = BG_INPUT;
            pnlContenido.Controls.Add(t); return t;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { lblError.Text = "⚠  El nombre es obligatorio."; lblError.Visible = true; return; }
            try
            {
                btnGuardar.Enabled = false;
                if (_modoEdicion)
                {
                    _existente.Nombre      = txtNombre.Text.Trim();
                    _existente.Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim();
                    _dao.ActualizarSubcategoria(_existente);
                }
                else
                {
                    _dao.InsertarSubcategoria(new Subcategoria
                    {
                        IdCategoria  = _categoria.IdCategoria,
                        Nombre       = txtNombre.Text.Trim(),
                        Descripcion  = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim()
                    });
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { lblError.Text = "⚠  " + ex.Message; lblError.Visible = true; }
            finally { btnGuardar.Enabled = true; }
        }
    }
}
