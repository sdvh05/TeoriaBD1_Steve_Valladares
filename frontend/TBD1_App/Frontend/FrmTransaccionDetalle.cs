using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    /// <summary>
    /// Dialog modal para crear o editar una transacción.
    /// Si transaccionExistente es null → modo INSERT
    /// Si transaccionExistente tiene valor → modo UPDATE
    /// </summary>
    public class FrmTransaccionDetalle : Form
    {
        // ── Colores ────────────────────────────────────────
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT     = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color COLOR_ING    = Color.FromArgb(63,  185, 120);
        private readonly Color COLOR_GAS    = Color.FromArgb(248,  81,  73);
        private readonly Color COLOR_AHO    = Color.FromArgb(251, 191,  36);
        private readonly Color ERROR_COLOR  = Color.FromArgb(248,  81,  73);

        // ── DAOs ───────────────────────────────────────────
        private readonly TransaccionDAO _dao    = new TransaccionDAO();
        private readonly CategoriaDAO   _catDAO = new CategoriaDAO();

        // ── Estado ─────────────────────────────────────────
        private readonly Usuario     _usuario;
        private readonly string      _idPresupuesto;
        private readonly int         _anio;
        private readonly int         _mes;
        private readonly Transaccion _transaccionExistente;
        private readonly bool        _modoEdicion;

        private List<Subcategoria> _subcategorias = new List<Subcategoria>();

        // ── Controles ──────────────────────────────────────
        private ComboBox  cboTipo;
        private ComboBox  cboSubcategoria;
        private TextBox   txtDescripcion;
        private TextBox   txtMonto;
        private DateTimePicker dtpFecha;
        private ComboBox  cboMetodoPago;
        private TextBox   txtNumFactura;
        private TextBox   txtObservaciones;
        private Label     lblError;
        private Button    btnGuardar;
        private Button    btnCancelar;
        private Panel     pnlContenido;

        public FrmTransaccionDetalle(Usuario usuario, string idPresupuesto, int anio, int mes, Transaccion transaccionExistente)
        {
            _usuario              = usuario;
            _idPresupuesto        = idPresupuesto;
            _anio                 = anio;
            _mes                  = mes;
            _transaccionExistente = transaccionExistente;
            _modoEdicion          = transaccionExistente != null;

            InicializarComponentes();
            CargarSubcategorias();

            if (_modoEdicion)
                PrecargarDatos();
        }

        private void InicializarComponentes()
        {
            this.Text            = _modoEdicion ? "Editar Transacción" : "Nueva Transacción";
            this.Size            = new Size(520, 580);
            this.MinimumSize     = new Size(520, 580);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            pnlContenido = new Panel
            {
                Size      = new Size(460, 510),
                Location  = new Point(30, 20),
                BackColor = BG_PANEL
            };
            pnlContenido.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlContenido.Width - 1, pnlContenido.Height - 1);
            };
            this.Controls.Add(pnlContenido);

            // Título
            var lblTitulo = new Label
            {
                Text      = _modoEdicion ? "Editar Transacción" : "Nueva Transacción",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(20, 20)
            };
            pnlContenido.Controls.Add(lblTitulo);

            var sep = new Label { Size = new Size(420, 1), Location = new Point(20, 52), BackColor = BORDER };
            pnlContenido.Controls.Add(sep);

            int y = 65;

            // ── Tipo ───────────────────────────────────────
            AgregarLabel("TIPO *", 20, y);
            cboTipo = AgregarCombo(20, y + 18, 200);
            cboTipo.Items.Add(new ComboItem("ingreso", "💰 Ingreso"));
            cboTipo.Items.Add(new ComboItem("gasto",   "💸 Gasto"));
            cboTipo.Items.Add(new ComboItem("ahorro",  "🏦 Ahorro"));
            cboTipo.SelectedIndex = 0;
            cboTipo.SelectedIndexChanged += (s, e) => CargarSubcategorias();

            // ── Subcategoría ───────────────────────────────
            AgregarLabel("SUBCATEGORÍA *", 240, y);
            cboSubcategoria = AgregarCombo(240, y + 18, 200);

            y += 75;

            // ── Descripción ────────────────────────────────
            AgregarLabel("DESCRIPCIÓN *", 20, y);
            txtDescripcion = AgregarTextBox(20, y + 18, 420);

            y += 70;

            // ── Monto / Fecha ──────────────────────────────
            AgregarLabel("MONTO (L) *", 20, y);
            txtMonto = AgregarTextBox(20, y + 18, 190);

            AgregarLabel("FECHA *", 230, y);
            dtpFecha = new DateTimePicker
            {
                Size        = new Size(210, 34),
                Location    = new Point(230, y + 18),
                Format      = DateTimePickerFormat.Short,
                Value       = new DateTime(_anio, _mes, Math.Min(DateTime.Now.Day, DateTime.DaysInMonth(_anio, _mes))),
                CalendarForeColor  = TEXT_PRIMARY,
                CalendarMonthBackground = BG_PANEL,
                Font        = new Font("Segoe UI", 10f)
            };
            pnlContenido.Controls.Add(dtpFecha);

            y += 70;

            // ── Método de pago ─────────────────────────────
            AgregarLabel("MÉTODO DE PAGO *", 20, y);
            cboMetodoPago = AgregarCombo(20, y + 18, 200);
            cboMetodoPago.Items.Add(new ComboItem("efectivo",        "Efectivo"));
            cboMetodoPago.Items.Add(new ComboItem("tarjeta_debito",   "Tarjeta Débito"));
            cboMetodoPago.Items.Add(new ComboItem("tarjeta_credito",  "Tarjeta Crédito"));
            cboMetodoPago.Items.Add(new ComboItem("transferencia",    "Transferencia"));
            cboMetodoPago.SelectedIndex = 0;

            // ── N° Factura ─────────────────────────────────
            AgregarLabel("N° FACTURA", 230, y);
            txtNumFactura = AgregarTextBox(230, y + 18, 210);

            y += 70;

            // ── Observaciones ──────────────────────────────
            AgregarLabel("OBSERVACIONES", 20, y);
            txtObservaciones = new TextBox
            {
                Size        = new Size(420, 55),
                Location    = new Point(20, y + 18),
                BackColor   = BG_INPUT,
                ForeColor   = TEXT_PRIMARY,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 10f),
                Multiline   = true
            };
            pnlContenido.Controls.Add(txtObservaciones);

            y += 85;

            // ── Error ──────────────────────────────────────
            lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = ERROR_COLOR,
                AutoSize  = false,
                Size      = new Size(420, 20),
                Location  = new Point(20, y),
                Visible   = false
            };
            pnlContenido.Controls.Add(lblError);

            y += 28;

            // ── Botones ────────────────────────────────────
            btnCancelar = new Button
            {
                Text      = "Cancelar",
                Size      = new Size(130, 38),
                Location  = new Point(20, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = BG_INPUT,
                ForeColor = TEXT_MUTED,
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 10f)
            };
            btnCancelar.FlatAppearance.BorderColor = BORDER;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            pnlContenido.Controls.Add(btnCancelar);

            btnGuardar = new Button
            {
                Text      = _modoEdicion ? "GUARDAR CAMBIOS" : "REGISTRAR",
                Size      = new Size(270, 38),
                Location  = new Point(170, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = ACCENT,
                ForeColor = Color.FromArgb(10, 12, 18),
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            pnlContenido.Controls.Add(btnGuardar);
        }

        // ── Helpers UI ─────────────────────────────────────
        private void AgregarLabel(string texto, int x, int y)
        {
            pnlContenido.Controls.Add(new Label
            {
                Text      = texto,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(x, y)
            });
        }

        private TextBox AgregarTextBox(int x, int y, int width)
        {
            var txt = new TextBox
            {
                Size        = new Size(width, 34),
                Location    = new Point(x, y),
                BackColor   = BG_INPUT,
                ForeColor   = TEXT_PRIMARY,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 10.5f)
            };
            txt.GotFocus  += (s, e) => ((TextBox)s).BackColor = Color.FromArgb(38, 46, 60);
            txt.LostFocus += (s, e) => ((TextBox)s).BackColor = BG_INPUT;
            pnlContenido.Controls.Add(txt);
            return txt;
        }

        private ComboBox AgregarCombo(int x, int y, int width)
        {
            var cbo = new ComboBox
            {
                Size          = new Size(width, 34),
                Location      = new Point(x, y),
                BackColor     = BG_INPUT,
                ForeColor     = TEXT_PRIMARY,
                FlatStyle     = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10f)
            };
            pnlContenido.Controls.Add(cbo);
            return cbo;
        }

        // ── Cargar subcategorías filtradas por tipo ────────
        private void CargarSubcategorias()
        {
            if (cboTipo.SelectedItem == null) return;
            string tipo = ((ComboItem)cboTipo.SelectedItem).Value;

            try
            {
                _subcategorias = _catDAO.ObtenerTodasSubcategorias();
                cboSubcategoria.Items.Clear();

                foreach (var s in _subcategorias)
                {
                    if (s.TipoCategoria.Equals(tipo, StringComparison.OrdinalIgnoreCase))
                        cboSubcategoria.Items.Add(new ComboItem(s.IdSubcategoria, $"{s.NombreCategoria} › {s.Nombre}"));
                }

                if (cboSubcategoria.Items.Count > 0)
                    cboSubcategoria.SelectedIndex = 0;
            }
            catch { /* Si falla, el combo queda vacío */ }
        }

        // ── Precargar datos en modo edición ───────────────
        private void PrecargarDatos()
        {
            var t = _transaccionExistente;

            // Seleccionar tipo
            for (int i = 0; i < cboTipo.Items.Count; i++)
                if (((ComboItem)cboTipo.Items[i]).Value == t.Tipo)
                { cboTipo.SelectedIndex = i; break; }

            CargarSubcategorias();

            // Seleccionar subcategoría
            for (int i = 0; i < cboSubcategoria.Items.Count; i++)
                if (((ComboItem)cboSubcategoria.Items[i]).Value == t.IdSubcategoria)
                { cboSubcategoria.SelectedIndex = i; break; }

            txtDescripcion.Text   = t.Descripcion;
            txtMonto.Text         = t.Monto.ToString("N2");
            dtpFecha.Value        = t.FechaTransaccion;
            txtNumFactura.Text    = t.NumFactura    ?? "";
            txtObservaciones.Text = t.Observaciones ?? "";

            // Seleccionar método de pago
            for (int i = 0; i < cboMetodoPago.Items.Count; i++)
                if (((ComboItem)cboMetodoPago.Items[i]).Value == t.MetodoPago)
                { cboMetodoPago.SelectedIndex = i; break; }

            // En edición el tipo está bloqueado (el trigger lo valida)
            cboTipo.Enabled = false;
        }

        // ── Guardar ────────────────────────────────────────
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            // Validaciones
            if (cboTipo.SelectedItem == null)
            { MostrarError("Selecciona el tipo."); return; }

            if (cboSubcategoria.SelectedItem == null || cboSubcategoria.Items.Count == 0)
            { MostrarError("Selecciona una subcategoría."); return; }

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            { MostrarError("La descripción es obligatoria."); txtDescripcion.Focus(); return; }

            if (!decimal.TryParse(txtMonto.Text.Replace(",", ""), out decimal monto) || monto <= 0)
            { MostrarError("El monto debe ser un número mayor a 0."); txtMonto.Focus(); return; }

            if (cboMetodoPago.SelectedItem == null)
            { MostrarError("Selecciona el método de pago."); return; }

            try
            {
                btnGuardar.Enabled = false;
                btnGuardar.Text    = "Guardando...";

                string tipo        = ((ComboItem)cboTipo.SelectedItem).Value;
                string idSubcat    = ((ComboItem)cboSubcategoria.SelectedItem).Value;
                string metodoPago  = ((ComboItem)cboMetodoPago.SelectedItem).Value;

                if (_modoEdicion)
                {
                    // UPDATE — solo campos editables
                    var t = _transaccionExistente;
                    t.Descripcion   = txtDescripcion.Text.Trim();
                    t.Monto         = monto;
                    t.MetodoPago    = metodoPago;
                    t.NumFactura    = string.IsNullOrWhiteSpace(txtNumFactura.Text)    ? null : txtNumFactura.Text.Trim();
                    t.Observaciones = string.IsNullOrWhiteSpace(txtObservaciones.Text) ? null : txtObservaciones.Text.Trim();

                    _dao.Actualizar(t);
                }
                else
                {
                    // INSERT
                    var nueva = new Transaccion
                    {
                        IdUser          = _usuario.IdUser,
                        IdPresupuesto   = _idPresupuesto,
                        IdSubcategoria  = idSubcat,
                        Anio            = dtpFecha.Value.Year,
                        Mes             = dtpFecha.Value.Month,
                        Tipo            = tipo,
                        Descripcion     = txtDescripcion.Text.Trim(),
                        Monto           = monto,
                        FechaTransaccion= dtpFecha.Value.Date,
                        MetodoPago      = metodoPago,
                        NumFactura      = string.IsNullOrWhiteSpace(txtNumFactura.Text)    ? null : txtNumFactura.Text.Trim(),
                        Observaciones   = string.IsNullOrWhiteSpace(txtObservaciones.Text) ? null : txtObservaciones.Text.Trim()
                    };

                    _dao.Insertar(nueva);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Contains("ORA-20")
                    ? ex.Message.Substring(ex.Message.IndexOf("ORA-20"))
                    : ex.Message;
                MostrarError("Error: " + msg);
            }
            finally
            {
                btnGuardar.Enabled = true;
                btnGuardar.Text    = _modoEdicion ? "GUARDAR CAMBIOS" : "REGISTRAR";
            }
        }

        private void MostrarError(string msg)
        {
            lblError.Text    = "⚠  " + msg;
            lblError.Visible = true;
        }
    }
}
