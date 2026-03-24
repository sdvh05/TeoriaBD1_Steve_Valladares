using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmPresupuestos : Form
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
        private readonly Color COLOR_ACT    = Color.FromArgb(56,  189, 248);
        private readonly Color COLOR_CER    = Color.FromArgb(110, 118, 129);

        // ── DAOs ───────────────────────────────────────────
        private readonly PresupuestoDAO _dao    = new PresupuestoDAO();
        private readonly CategoriaDAO   _catDAO = new CategoriaDAO();

        // ── Estado ─────────────────────────────────────────
        private readonly Usuario _usuario;
        private List<Presupuesto> _presupuestos = new List<Presupuesto>();
        private Presupuesto _seleccionado = null;

        // ── Panel izquierdo ────────────────────────────────
        private Panel       pnlLista;
        private ListBox     lstPresupuestos;
        private Button      btnNuevo;
        private Button      btnRenombrar;
        private Button      btnCerrar;

        // ── Panel derecho ──────────────────────────────────
        private Panel       pnlDetalle;
        private Label       lblNombrePre;
        private Label       lblPeriodoPre;
        private Label       lblEstadoPre;
        private Label       lblTotalesInfo;
        private DataGridView dgvDetalles;
        private Button      btnAgregarDetalle;
        private Button      btnEditarDetalle;
        private Button      btnEliminarDetalle;
        private Label       lblSinSeleccion;
        private Label       lblError;

        public FrmPresupuestos(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarPresupuestos();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Presupuestos";
            this.Size            = new Size(1100, 660);
            this.MinimumSize     = new Size(1100, 660);
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
                using (var brush = new SolidBrush(ACCENT))
                    e.Graphics.FillRectangle(brush, 0, 0, 4, 60);
            };
            this.Controls.Add(pnlHeader);

            pnlHeader.Controls.Add(new Label
            {
                Text      = "Presupuestos",
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(18, 16)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "Define y gestiona tus presupuestos por período",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(190, 22)
            });

            // ── PANEL IZQUIERDO — lista ────────────────────
            pnlLista = new Panel
            {
                Size      = new Size(280, 580),
                Location  = new Point(0, 60),
                BackColor = BG_PANEL
            };
            pnlLista.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawLine(pen, 279, 0, 279, 580);
            };
            this.Controls.Add(pnlLista);

            pnlLista.Controls.Add(new Label
            {
                Text      = "MIS PRESUPUESTOS",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(15, 15)
            });

            lstPresupuestos = new ListBox
            {
                Size             = new Size(260, 440),
                Location         = new Point(10, 38),
                BackColor        = BG_CARD,
                ForeColor        = TEXT_PRIMARY,
                BorderStyle      = BorderStyle.None,
                Font             = new Font("Segoe UI", 10f),
                DrawMode         = DrawMode.OwnerDrawFixed,
                ItemHeight       = 58,
                SelectionMode    = SelectionMode.One
            };
            lstPresupuestos.DrawItem         += LstPresupuestos_DrawItem;
            lstPresupuestos.SelectedIndexChanged += LstPresupuestos_SelectedIndexChanged;
            pnlLista.Controls.Add(lstPresupuestos);

            // Botones lista
            btnNuevo = CrearBotonPanel(pnlLista, "+ Nuevo", 10, 490, 80, ACCENT, Color.FromArgb(10, 12, 18));
            btnNuevo.Click += BtnNuevo_Click;

            btnRenombrar = CrearBotonPanel(pnlLista, "✎ Renombrar", 100, 490, 90, BG_CARD, TEXT_PRIMARY);
            btnRenombrar.FlatAppearance.BorderColor = BORDER;
            btnRenombrar.Click += BtnRenombrar_Click;

            btnCerrar = CrearBotonPanel(pnlLista, "⊘ Cerrar", 200, 490, 70, BG_CARD, COLOR_GAS);
            btnCerrar.FlatAppearance.BorderColor = COLOR_GAS;
            btnCerrar.Click += BtnCerrar_Click;

            // ── PANEL DERECHO — detalle ────────────────────
            pnlDetalle = new Panel
            {
                Size      = new Size(815, 580),
                Location  = new Point(280, 60),
                BackColor = BG_DARK
            };
            this.Controls.Add(pnlDetalle);

            // Mensaje cuando no hay selección
            lblSinSeleccion = new Label
            {
                Text      = "← Selecciona un presupuesto para ver sus detalles",
                Font      = new Font("Segoe UI", 12f),
                ForeColor = TEXT_MUTED,
                AutoSize  = false,
                Size      = new Size(700, 50),
                Location  = new Point(55, 260),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible   = true
            };
            pnlDetalle.Controls.Add(lblSinSeleccion);

            // Info del presupuesto seleccionado
            lblNombrePre = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(20, 15),
                Visible   = false
            };
            pnlDetalle.Controls.Add(lblNombrePre);

            lblPeriodoPre = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(20, 45),
                Visible   = false
            };
            pnlDetalle.Controls.Add(lblPeriodoPre);

            lblEstadoPre = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(20, 68),
                Visible   = false
            };
            pnlDetalle.Controls.Add(lblEstadoPre);

            lblTotalesInfo = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TEXT_MUTED,
                AutoSize  = false,
                Size      = new Size(780, 22),
                Location  = new Point(20, 92),
                Visible   = false
            };
            pnlDetalle.Controls.Add(lblTotalesInfo);

            // Separador
            var sep = new Label
            {
                Size      = new Size(775, 1),
                Location  = new Point(20, 120),
                BackColor = BORDER,
                Visible   = false,
                Name      = "sep"
            };
            pnlDetalle.Controls.Add(sep);

            // Sub-título detalles
            var lblDetallesTit = new Label
            {
                Text      = "DETALLE POR SUBCATEGORÍA",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(20, 130),
                Visible   = false,
                Name      = "lblDetallesTit"
            };
            pnlDetalle.Controls.Add(lblDetallesTit);

            // Botones detalle
            btnAgregarDetalle = CrearBotonPanel(pnlDetalle, "+ Agregar", 20, 152, 100, ACCENT, Color.FromArgb(10, 12, 18));
            btnAgregarDetalle.Visible = false;
            btnAgregarDetalle.Click  += BtnAgregarDetalle_Click;

            btnEditarDetalle = CrearBotonPanel(pnlDetalle, "✎ Editar", 130, 152, 90, BG_CARD, TEXT_PRIMARY);
            btnEditarDetalle.FlatAppearance.BorderColor = BORDER;
            btnEditarDetalle.Visible = false;
            btnEditarDetalle.Click  += BtnEditarDetalle_Click;

            btnEliminarDetalle = CrearBotonPanel(pnlDetalle, "✕ Eliminar", 230, 152, 90, BG_CARD, COLOR_GAS);
            btnEliminarDetalle.FlatAppearance.BorderColor = COLOR_GAS;
            btnEliminarDetalle.Visible = false;
            btnEliminarDetalle.Click  += BtnEliminarDetalle_Click;

            lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = COLOR_GAS,
                AutoSize  = true,
                Location  = new Point(330, 160),
                Visible   = false
            };
            pnlDetalle.Controls.Add(lblError);

            // Grid detalles
            dgvDetalles = new DataGridView
            {
                Size              = new Size(775, 355),
                Location          = new Point(20, 188),
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
                Font                  = new Font("Segoe UI", 9.5f),
                Visible               = false
            };
            dgvDetalles.DefaultCellStyle.BackColor            = BG_CARD;
            dgvDetalles.DefaultCellStyle.ForeColor            = TEXT_PRIMARY;
            dgvDetalles.DefaultCellStyle.SelectionBackColor   = BG_CARD_SEL;
            dgvDetalles.DefaultCellStyle.SelectionForeColor   = ACCENT;
            dgvDetalles.ColumnHeadersDefaultCellStyle.BackColor = BG_PANEL;
            dgvDetalles.ColumnHeadersDefaultCellStyle.ForeColor = TEXT_MUTED;
            dgvDetalles.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8f, FontStyle.Bold);
            dgvDetalles.ColumnHeadersBorderStyle    = DataGridViewHeaderBorderStyle.None;
            dgvDetalles.EnableHeadersVisualStyles   = false;
            dgvDetalles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(25, 32, 42);
            dgvDetalles.RowTemplate.Height = 30;

            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId",     HeaderText = "ID",           Visible = false });
            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTipo",   HeaderText = "TIPO",         FillWeight = 10 });
            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCat",    HeaderText = "CATEGORÍA",    FillWeight = 20 });
            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSubcat", HeaderText = "SUBCATEGORÍA", FillWeight = 20 });
            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMonto",  HeaderText = "MONTO (L)",    FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvDetalles.Columns.Add(new DataGridViewTextBoxColumn { Name = "colJust",   HeaderText = "JUSTIFICACIÓN", FillWeight = 35 });

            dgvDetalles.CellFormatting += DgvDetalles_CellFormatting;
            pnlDetalle.Controls.Add(dgvDetalles);
        }

        // ── Dibuja items del ListBox con estilo custom ─────
        private void LstPresupuestos_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _presupuestos.Count) return;
            var p   = _presupuestos[e.Index];
            bool sel = (e.State & DrawItemState.Selected) != 0;

            Color bg     = sel ? BG_CARD_SEL : BG_CARD;
            Color accent = p.EstadoPresupuesto == "activo" ? COLOR_ACT : COLOR_CER;

            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
            // Línea lateral de color
            e.Graphics.FillRectangle(new SolidBrush(accent), e.Bounds.X, e.Bounds.Y, 3, e.Bounds.Height);

            // Nombre
            e.Graphics.DrawString(p.NombreDescriptivo,
                new Font("Segoe UI", 10f, FontStyle.Bold),
                new SolidBrush(TEXT_PRIMARY),
                e.Bounds.X + 12, e.Bounds.Y + 8);

            // Período
            e.Graphics.DrawString(p.PeriodoDescriptivo,
                new Font("Segoe UI", 8.5f),
                new SolidBrush(TEXT_MUTED),
                e.Bounds.X + 12, e.Bounds.Y + 28);

            // Estado badge
            string badge = p.EstadoPresupuesto.ToUpper();
            e.Graphics.DrawString(badge,
                new Font("Segoe UI", 7.5f, FontStyle.Bold),
                new SolidBrush(accent),
                e.Bounds.X + 12, e.Bounds.Y + 42);

            // Separador
            e.Graphics.DrawLine(new Pen(BORDER, 1),
                e.Bounds.X, e.Bounds.Bottom - 1,
                e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        // ── Color por tipo en grid ─────────────────────────
        private void DgvDetalles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string tipo = dgvDetalles.Rows[e.RowIndex].Cells["colTipo"].Value?.ToString() ?? "";
            Color color = tipo == "ingreso" ? COLOR_ING : tipo == "gasto" ? COLOR_GAS : COLOR_AHO;

            if (dgvDetalles.Columns[e.ColumnIndex].Name == "colTipo")
            {
                e.CellStyle.ForeColor = color;
                e.CellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            }
            if (dgvDetalles.Columns[e.ColumnIndex].Name == "colMonto")
                e.CellStyle.ForeColor = color;
        }

        // ── Helper botón ───────────────────────────────────
        private Button CrearBotonPanel(Panel panel, string texto, int x, int y, int width, Color back, Color fore)
        {
            var btn = new Button
            {
                Text      = texto,
                Size      = new Size(width, 30),
                Location  = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            panel.Controls.Add(btn);
            return btn;
        }

        // ── Cargar lista de presupuestos ───────────────────
        private void CargarPresupuestos()
        {
            try
            {
                _presupuestos = _dao.ObtenerPorUsuario(_usuario.IdUser);
                lstPresupuestos.Items.Clear();
                foreach (var p in _presupuestos)
                    lstPresupuestos.Items.Add(p.NombreDescriptivo);

                if (_presupuestos.Count > 0)
                    lstPresupuestos.SelectedIndex = 0;
                else
                    MostrarPanelDetalle(false);
            }
            catch (Exception ex)
            {
                MostrarError("Error cargando presupuestos: " + ex.Message);
            }
        }

        // ── Al seleccionar presupuesto ─────────────────────
        private void LstPresupuestos_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstPresupuestos.SelectedIndex;
            if (idx < 0 || idx >= _presupuestos.Count) return;

            _seleccionado = _presupuestos[idx];
            MostrarPanelDetalle(true);
            CargarDetalles();
        }

        // ── Mostrar/ocultar panel de detalle ───────────────
        private void MostrarPanelDetalle(bool mostrar)
        {
            lblSinSeleccion.Visible = !mostrar;
            lblNombrePre.Visible    =  mostrar;
            lblPeriodoPre.Visible   =  mostrar;
            lblEstadoPre.Visible    =  mostrar;
            lblTotalesInfo.Visible  =  mostrar;
            dgvDetalles.Visible     =  mostrar;
            btnAgregarDetalle.Visible  = mostrar;
            btnEditarDetalle.Visible   = mostrar;
            btnEliminarDetalle.Visible = mostrar;

            foreach (Control c in pnlDetalle.Controls)
                if (c.Name == "sep" || c.Name == "lblDetallesTit")
                    c.Visible = mostrar;
        }

        // ── Cargar detalles del presupuesto seleccionado ───
        private void CargarDetalles()
        {
            if (_seleccionado == null) return;
            lblError.Visible = false;

            var p = _seleccionado;

            // Header info
            lblNombrePre.Text  = p.NombreDescriptivo;
            lblPeriodoPre.Text = $"Período: {p.PeriodoDescriptivo}   •   Creado: {p.FechaHoraCreacion:dd/MM/yyyy}";
            lblEstadoPre.Text  = p.EstadoPresupuesto.ToUpper();
            lblEstadoPre.ForeColor = p.EstadoPresupuesto == "activo" ? COLOR_ACT : COLOR_CER;

            // Bloquear edición si está cerrado
            bool activo = p.EstadoPresupuesto == "activo";
            btnAgregarDetalle.Enabled  = activo;
            btnEditarDetalle.Enabled   = activo;
            btnEliminarDetalle.Enabled = activo;
            btnCerrar.Enabled          = activo;
            btnRenombrar.Enabled       = activo;

            try
            {
                var detalles = _dao.ObtenerDetalles(p.IdPresupuesto);
                dgvDetalles.Rows.Clear();

                // Calcular totales desde los detalles reales
                decimal totalIngresos = 0, totalGastos = 0, totalAhorro = 0;

                foreach (var d in detalles)
                {
                    dgvDetalles.Rows.Add(
                        d.IdDetalle,
                        d.Tipo,
                        d.NombreCategoria,
                        d.NombreSubcategoria,
                        d.Monto.ToString("N2"),
                        d.Justificacion ?? ""
                    );

                    // Acumular por tipo
                    switch (d.Tipo?.ToLower())
                    {
                        case "ingreso": totalIngresos += d.Monto; break;
                        case "gasto":   totalGastos   += d.Monto; break;
                        case "ahorro":  totalAhorro   += d.Monto; break;
                    }
                }

                // Actualizar label con totales reales
                lblTotalesInfo.Text = $"Ingresos planeados: L {totalIngresos:N2}   |   " +
                                      $"Gastos planeados: L {totalGastos:N2}   |   " +
                                      $"Ahorro planeado: L {totalAhorro:N2}";
            }
            catch (Exception ex)
            {
                MostrarError("Error cargando detalles: " + ex.Message);
            }
        }

        // ── NUEVO PRESUPUESTO ──────────────────────────────
        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            var dlg = new FrmPresupuestoDetalle(_usuario, null);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarPresupuestos();
        }

        // ── RENOMBRAR ──────────────────────────────────────
        private void BtnRenombrar_Click(object sender, EventArgs e)
        {
            if (_seleccionado == null) { MostrarError("Selecciona un presupuesto primero."); return; }

            string nuevoNombre = Microsoft.VisualBasic.Interaction.InputBox(
                "Nuevo nombre del presupuesto:",
                "Renombrar presupuesto",
                _seleccionado.NombreDescriptivo);

            if (string.IsNullOrWhiteSpace(nuevoNombre)) return;

            try
            {
                _seleccionado.NombreDescriptivo = nuevoNombre.Trim();
                _dao.Actualizar(_seleccionado);
                CargarPresupuestos();
            }
            catch (Exception ex) { MostrarError("Error al renombrar: " + ex.Message); }
        }

        // ── CERRAR PRESUPUESTO ─────────────────────────────
        private void BtnCerrar_Click(object sender, EventArgs e)
        {
            if (_seleccionado == null) { MostrarError("Selecciona un presupuesto primero."); return; }

            var confirm = MessageBox.Show(
                $"¿Cerrar el presupuesto '{_seleccionado.NombreDescriptivo}'?\n\nUna vez cerrado no se podrán agregar más transacciones.",
                "Confirmar cierre",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _dao.Cerrar(_seleccionado.IdPresupuesto);
                CargarPresupuestos();
            }
            catch (Exception ex) { MostrarError("Error al cerrar: " + ex.Message); }
        }

        // ── AGREGAR DETALLE ────────────────────────────────
        private void BtnAgregarDetalle_Click(object sender, EventArgs e)
        {
            if (_seleccionado == null) return;
            var dlg = new FrmDetallePresupuesto(_seleccionado.IdPresupuesto, null);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarDetalles();
        }

        // ── EDITAR DETALLE ─────────────────────────────────
        private void BtnEditarDetalle_Click(object sender, EventArgs e)
        {
            if (dgvDetalles.SelectedRows.Count == 0) { MostrarError("Selecciona un detalle para editar."); return; }
            string idDetalle = dgvDetalles.SelectedRows[0].Cells["colId"].Value.ToString();

            // Recuperar el detalle completo
            var detalles = _dao.ObtenerDetalles(_seleccionado.IdPresupuesto);
            var detalle  = detalles.Find(d => d.IdDetalle == idDetalle);
            if (detalle == null) return;

            var dlg = new FrmDetallePresupuesto(_seleccionado.IdPresupuesto, detalle);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarDetalles();
        }

        // ── ELIMINAR DETALLE ───────────────────────────────
        private void BtnEliminarDetalle_Click(object sender, EventArgs e)
        {
            if (dgvDetalles.SelectedRows.Count == 0) { MostrarError("Selecciona un detalle para eliminar."); return; }
            string idDetalle = dgvDetalles.SelectedRows[0].Cells["colId"].Value.ToString();
            string subcat    = dgvDetalles.SelectedRows[0].Cells["colSubcat"].Value.ToString();

            var confirm = MessageBox.Show(
                $"¿Eliminar el detalle de '{subcat}'?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _dao.EliminarDetalle(idDetalle);
                CargarDetalles();
            }
            catch (Exception ex) { MostrarError("Error al eliminar: " + ex.Message); }
        }

        private void MostrarError(string msg)
        {
            lblError.Text    = "⚠  " + msg;
            lblError.Visible = true;
        }
    }

    // ════════════════════════════════════════════════════════
    // Dialog: Nuevo / Editar Presupuesto
    // ════════════════════════════════════════════════════════
    public class FrmPresupuestoDetalle : Form
    {
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT     = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color ERROR_COLOR  = Color.FromArgb(248,  81,  73);

        private readonly PresupuestoDAO _dao = new PresupuestoDAO();
        private readonly Usuario _usuario;

        private TextBox   txtNombre;
        private ComboBox  cboAnioInicio, cboMesInicio;
        private ComboBox  cboAnioFin,    cboMesFin;
        private Label     lblError;
        private Button    btnGuardar, btnCancelar;
        private Panel     pnlContenido;

        public FrmPresupuestoDetalle(Usuario usuario, Presupuesto existing)
        {
            _usuario = usuario;
            InicializarComponentes();
            if (existing != null) PrecargarDatos(existing);
        }

        private void InicializarComponentes()
        {
            this.Text            = "Nuevo Presupuesto";
            this.Size            = new Size(480, 380);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            pnlContenido = new Panel { Size = new Size(420, 320), Location = new Point(30, 20), BackColor = BG_PANEL };
            pnlContenido.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlContenido.Width - 1, pnlContenido.Height - 1);
            };
            this.Controls.Add(pnlContenido);

            pnlContenido.Controls.Add(new Label { Text = "Nuevo Presupuesto", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(20, 18) });
            pnlContenido.Controls.Add(new Label { Size = new Size(380, 1), Location = new Point(20, 45), BackColor = BORDER });

            // Nombre
            AgregarLabel("NOMBRE DESCRIPTIVO *", 20, 55);
            txtNombre = AgregarTextBox(20, 73, 380);

            // Inicio
            AgregarLabel("INICIO", 20, 125);
            cboAnioInicio = AgregarComboAnio(20, 143);
            cboMesInicio  = AgregarComboMes(115, 143);

            // Fin
            AgregarLabel("FIN", 230, 125);
            cboAnioFin = AgregarComboAnio(230, 143);
            cboMesFin  = AgregarComboMes(325, 143);

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = ERROR_COLOR, AutoSize = false, Size = new Size(380, 20), Location = new Point(20, 205), Visible = false };
            pnlContenido.Controls.Add(lblError);

            btnCancelar = new Button { Text = "Cancelar", Size = new Size(120, 36), Location = new Point(20, 265), FlatStyle = FlatStyle.Flat, BackColor = BG_INPUT, ForeColor = TEXT_MUTED, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f) };
            btnCancelar.FlatAppearance.BorderColor = BORDER;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            pnlContenido.Controls.Add(btnCancelar);

            btnGuardar = new Button { Text = "CREAR PRESUPUESTO", Size = new Size(240, 36), Location = new Point(160, 265), FlatStyle = FlatStyle.Flat, BackColor = ACCENT, ForeColor = Color.FromArgb(10, 12, 18), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            pnlContenido.Controls.Add(btnGuardar);
        }

        private void AgregarLabel(string texto, int x, int y)
        {
            pnlContenido.Controls.Add(new Label { Text = texto, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(x, y) });
        }

        private TextBox AgregarTextBox(int x, int y, int width)
        {
            var txt = new TextBox { Size = new Size(width, 36), Location = new Point(x, y), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10.5f) };
            txt.GotFocus  += (s, e) => ((TextBox)s).BackColor = Color.FromArgb(38, 46, 60);
            txt.LostFocus += (s, e) => ((TextBox)s).BackColor = BG_INPUT;
            pnlContenido.Controls.Add(txt);
            return txt;
        }

        private ComboBox AgregarComboAnio(int x, int y)
        {
            var cbo = new ComboBox { Size = new Size(88, 34), Location = new Point(x, y), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            for (int a = DateTime.Now.Year - 2; a <= DateTime.Now.Year + 5; a++)
                cbo.Items.Add(a);
            cbo.SelectedItem = DateTime.Now.Year;
            pnlContenido.Controls.Add(cbo);
            return cbo;
        }

        private ComboBox AgregarComboMes(int x, int y)
        {
            var cbo = new ComboBox { Size = new Size(108, 34), Location = new Point(x, y), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            for (int m = 1; m <= 12; m++)
                cbo.Items.Add(new ComboItem(m.ToString(), new DateTime(2000, m, 1).ToString("MMMM")));
            cbo.SelectedIndex = DateTime.Now.Month - 1;
            pnlContenido.Controls.Add(cbo);
            return cbo;
        }

        private void PrecargarDatos(Presupuesto p)
        {
            txtNombre.Text = p.NombreDescriptivo;
            cboAnioInicio.SelectedItem = p.AnioInicio;
            cboMesInicio.SelectedIndex = p.MesInicio - 1;
            cboAnioFin.SelectedItem    = p.AnioFin;
            cboMesFin.SelectedIndex    = p.MesFin - 1;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { lblError.Text = "⚠  El nombre es obligatorio."; lblError.Visible = true; return; }

            int anioIni = (int)cboAnioInicio.SelectedItem;
            int mesIni  = int.Parse(((ComboItem)cboMesInicio.SelectedItem).Value);
            int anioFin = (int)cboAnioFin.SelectedItem;
            int mesFin  = int.Parse(((ComboItem)cboMesFin.SelectedItem).Value);

            if (anioFin < anioIni || (anioFin == anioIni && mesFin < mesIni))
            { lblError.Text = "⚠  El período fin no puede ser anterior al inicio."; lblError.Visible = true; return; }

            try
            {
                btnGuardar.Enabled = false;
                var p = new Presupuesto
                {
                    IdUser            = _usuario.IdUser,
                    NombreDescriptivo = txtNombre.Text.Trim(),
                    AnioInicio        = anioIni,
                    MesInicio         = mesIni,
                    AnioFin           = anioFin,
                    MesFin            = mesFin
                };
                _dao.Insertar(p);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { lblError.Text = "⚠  " + ex.Message; lblError.Visible = true; }
            finally { btnGuardar.Enabled = true; }
        }
    }

    // ════════════════════════════════════════════════════════
    // Dialog: Agregar / Editar línea de detalle
    // ════════════════════════════════════════════════════════
    public class FrmDetallePresupuesto : Form
    {
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT     = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color ERROR_COLOR  = Color.FromArgb(248,  81,  73);

        private readonly PresupuestoDAO _dao    = new PresupuestoDAO();
        private readonly CategoriaDAO   _catDAO = new CategoriaDAO();

        private readonly string            _idPresupuesto;
        private readonly PresupuestoDetalle _existente;
        private readonly bool              _modoEdicion;

        private ComboBox cboSubcategoria;
        private TextBox  txtMonto;
        private TextBox  txtJustificacion;
        private Label    lblError;
        private Button   btnGuardar, btnCancelar;
        private Panel    pnlContenido;

        private readonly Color BG_INPUT_HOV = Color.FromArgb(38, 46, 60);

        public FrmDetallePresupuesto(string idPresupuesto, PresupuestoDetalle existente)
        {
            _idPresupuesto = idPresupuesto;
            _existente     = existente;
            _modoEdicion   = existente != null;
            InicializarComponentes();
            CargarSubcategorias();
            if (_modoEdicion) PrecargarDatos();
        }

        private void InicializarComponentes()
        {
            this.Text            = _modoEdicion ? "Editar Detalle" : "Agregar Detalle";
            this.Size            = new Size(460, 400);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            pnlContenido = new Panel { Size = new Size(410, 340), Location = new Point(20, 15), BackColor = BG_PANEL };
            pnlContenido.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlContenido.Width - 1, pnlContenido.Height - 1);
            };
            this.Controls.Add(pnlContenido);

            pnlContenido.Controls.Add(new Label { Text = _modoEdicion ? "Editar Detalle" : "Agregar Detalle", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = TEXT_PRIMARY, AutoSize = true, Location = new Point(20, 18) });
            pnlContenido.Controls.Add(new Label { Size = new Size(340, 1), Location = new Point(20, 45), BackColor = BORDER });

            // Subcategoría
            AgregarLabel("SUBCATEGORÍA *", 20, 55);
            cboSubcategoria = new ComboBox { Size = new Size(340, 34), Location = new Point(20, 73), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            cboSubcategoria.Enabled = !_modoEdicion; // En edición no se cambia la subcategoría
            pnlContenido.Controls.Add(cboSubcategoria);

            // Monto
            AgregarLabel("MONTO (L) *", 20, 120);
            txtMonto = AgregarTextBox(20, 138, 160);

            // Justificación
            AgregarLabel("JUSTIFICACIÓN", 20, 183);
            txtJustificacion = new TextBox { Size = new Size(340, 36), Location = new Point(20, 201), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f) };
            pnlContenido.Controls.Add(txtJustificacion);

            lblError = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = ERROR_COLOR, AutoSize = true, Location = new Point(20, 248), Visible = false };
            pnlContenido.Controls.Add(lblError);

            // Los botones van fuera del panel para que no se corten
            btnCancelar = new Button { Text = "Cancelar", Size = new Size(110, 36), Location = new Point(20, 292), FlatStyle = FlatStyle.Flat, BackColor = BG_INPUT, ForeColor = TEXT_MUTED, Cursor = Cursors.Hand };
            btnCancelar.FlatAppearance.BorderColor = BORDER;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            pnlContenido.Controls.Add(btnCancelar);

            btnGuardar = new Button { Text = _modoEdicion ? "GUARDAR" : "AGREGAR", Size = new Size(270, 36), Location = new Point(138, 292), FlatStyle = FlatStyle.Flat, BackColor = ACCENT, ForeColor = Color.FromArgb(10, 12, 18), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            pnlContenido.Controls.Add(btnGuardar);
        }

        private void AgregarLabel(string texto, int x, int y)
        {
            pnlContenido.Controls.Add(new Label { Text = texto, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = TEXT_MUTED, AutoSize = true, Location = new Point(x, y) });
        }

        private TextBox AgregarTextBox(int x, int y, int width)
        {
            var txt = new TextBox { Size = new Size(width, 36), Location = new Point(x, y), BackColor = BG_INPUT, ForeColor = TEXT_PRIMARY, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10.5f) };
            txt.GotFocus  += (s, e) => ((TextBox)s).BackColor = BG_INPUT_HOV;
            txt.LostFocus += (s, e) => ((TextBox)s).BackColor = BG_INPUT;
            pnlContenido.Controls.Add(txt);
            return txt;
        }

        private void CargarSubcategorias()
        {
            var subs = _catDAO.ObtenerTodasSubcategorias();
            cboSubcategoria.Items.Clear();
            foreach (var s in subs)
                cboSubcategoria.Items.Add(new ComboItem(s.IdSubcategoria, $"{s.TipoCategoria.ToUpper()} › {s.NombreCategoria} › {s.Nombre}"));
            if (cboSubcategoria.Items.Count > 0)
                cboSubcategoria.SelectedIndex = 0;
        }

        private void PrecargarDatos()
        {
            for (int i = 0; i < cboSubcategoria.Items.Count; i++)
                if (((ComboItem)cboSubcategoria.Items[i]).Value == _existente.IdSubcategoria)
                { cboSubcategoria.SelectedIndex = i; break; }

            txtMonto.Text         = _existente.Monto.ToString("N2");
            txtJustificacion.Text = _existente.Justificacion ?? "";
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            if (cboSubcategoria.SelectedItem == null) { lblError.Text = "⚠  Selecciona una subcategoría."; lblError.Visible = true; return; }
            if (!decimal.TryParse(txtMonto.Text.Replace(",", ""), out decimal monto) || monto <= 0)
            { lblError.Text = "⚠  El monto debe ser mayor a 0."; lblError.Visible = true; return; }

            try
            {
                btnGuardar.Enabled = false;
                if (_modoEdicion)
                {
                    _existente.Monto         = monto;
                    _existente.Justificacion = string.IsNullOrWhiteSpace(txtJustificacion.Text) ? null : txtJustificacion.Text.Trim();
                    _dao.ActualizarDetalle(_existente);
                }
                else
                {
                    var d = new PresupuestoDetalle
                    {
                        IdPresupuesto  = _idPresupuesto,
                        IdSubcategoria = ((ComboItem)cboSubcategoria.SelectedItem).Value,
                        Monto          = monto,
                        Justificacion  = string.IsNullOrWhiteSpace(txtJustificacion.Text) ? null : txtJustificacion.Text.Trim()
                    };
                    _dao.InsertarDetalle(d);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Contains("ORA-00001") ? "Esa subcategoría ya tiene un monto asignado en este presupuesto." : ex.Message;
                lblError.Text    = "⚠  " + msg;
                lblError.Visible = true;
            }
            finally { btnGuardar.Enabled = true; }
        }
    }
}
