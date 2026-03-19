using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmPerfil : Form
    {
        // ── Colores ────────────────────────────────────────
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT     = Color.FromArgb(30,  37,  48);
        private readonly Color BG_INPUT_HOV = Color.FromArgb(38,  46,  60);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color COLOR_OK     = Color.FromArgb(63,  185, 120);
        private readonly Color COLOR_ERR    = Color.FromArgb(248,  81,  73);

        // ── DAO ────────────────────────────────────────────
        private readonly UsuarioDAO _dao = new UsuarioDAO();

        // ── Estado ─────────────────────────────────────────
        private Usuario _usuario;
        private bool    _modoEdicion = false;

        // ── Controles ──────────────────────────────────────
        private Label   lblAvatar;
        private Label   lblNombreCompleto;
        private Label   lblIdUsuario;
        private Label   lblFechaRegistro;

        // Campos de solo lectura / edición
        private TextBox txtPrimerNombre;
        private TextBox txtSegundoNombre;
        private TextBox txtPrimerApellido;
        private TextBox txtSegundoApellido;
        private TextBox txtCorreo;
        private TextBox txtSalario;

        private Button  btnEditar;
        private Button  btnGuardar;
        private Button  btnCancelar;
        private Label   lblMensaje;
        private Panel   pnlForm;

        public FrmPerfil(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarDatos();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Mi Perfil";
            this.Size            = new Size(680, 620);
            this.MinimumSize     = new Size(680, 620);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── HEADER con avatar ──────────────────────────
            var pnlHeader = new Panel
            {
                Size      = new Size(680, 130),
                Location  = new Point(0, 0),
                BackColor = BG_PANEL
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawLine(pen, 0, 129, 680, 129);
                using (var brush = new SolidBrush(ACCENT))
                    e.Graphics.FillRectangle(brush, 0, 0, 4, 130);
            };
            this.Controls.Add(pnlHeader);

            // Círculo avatar
            lblAvatar = new Label
            {
                Size      = new Size(72, 72),
                Location  = new Point(25, 28),
                BackColor = Color.FromArgb(38, 46, 60),
                ForeColor = ACCENT,
                Font      = new Font("Segoe UI", 24f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblAvatar.Paint += (s, e) =>
            {
                var g    = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(38, 46, 60)))
                    g.FillEllipse(brush, 0, 0, lblAvatar.Width - 1, lblAvatar.Height - 1);
                using (var pen = new Pen(ACCENT, 2))
                    g.DrawEllipse(pen, 1, 1, lblAvatar.Width - 3, lblAvatar.Height - 3);
                // Dibuja la inicial
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var brush = new SolidBrush(ACCENT))
                    g.DrawString(lblAvatar.Text, lblAvatar.Font, brush, new RectangleF(0, 0, lblAvatar.Width, lblAvatar.Height), sf);
            };
            pnlHeader.Controls.Add(lblAvatar);

            lblNombreCompleto = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(112, 35)
            };
            pnlHeader.Controls.Add(lblNombreCompleto);

            lblIdUsuario = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(113, 68)
            };
            pnlHeader.Controls.Add(lblIdUsuario);

            lblFechaRegistro = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(113, 92)
            };
            pnlHeader.Controls.Add(lblFechaRegistro);

            // ── FORMULARIO ─────────────────────────────────
            pnlForm = new Panel
            {
                Size      = new Size(620, 390),
                Location  = new Point(30, 148),
                BackColor = BG_PANEL
            };
            pnlForm.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlForm.Width - 1, pnlForm.Height - 1);
            };
            this.Controls.Add(pnlForm);

            var lblSecInfo = new Label
            {
                Text      = "INFORMACIÓN PERSONAL",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(20, 18)
            };
            pnlForm.Controls.Add(lblSecInfo);

            var sep = new Label { Size = new Size(580, 1), Location = new Point(20, 38), BackColor = BORDER };
            pnlForm.Controls.Add(sep);

            // Fila 1: Primer Nombre / Segundo Nombre
            AgregarCampo("PRIMER NOMBRE",    20,  50, 260, out txtPrimerNombre);
            AgregarCampo("SEGUNDO NOMBRE",   320, 50, 280, out txtSegundoNombre);

            // Fila 2: Primer Apellido / Segundo Apellido
            AgregarCampo("PRIMER APELLIDO",  20,  130, 260, out txtPrimerApellido);
            AgregarCampo("SEGUNDO APELLIDO", 320, 130, 280, out txtSegundoApellido);

            // Fila 3: Correo (solo lectura siempre)
            AgregarCampo("CORREO ELECTRÓNICO", 20, 210, 580, out txtCorreo);
            txtCorreo.ReadOnly  = true;
            txtCorreo.BackColor = Color.FromArgb(20, 25, 32);
            txtCorreo.ForeColor = TEXT_MUTED;

            // Fila 4: Salario
            AgregarCampo("SALARIO BASE MENSUAL (L)", 20, 290, 260, out txtSalario);

            // Nota correo
            var lblNotaCorreo = new Label
            {
                Text      = "El correo no se puede modificar",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(20, 268)
            };
            pnlForm.Controls.Add(lblNotaCorreo);

            // ── MENSAJE ────────────────────────────────────
            lblMensaje = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = COLOR_OK,
                AutoSize  = false,
                Size      = new Size(400, 22),
                Location  = new Point(30, 548),
                Visible   = false
            };
            this.Controls.Add(lblMensaje);

            // ── BOTONES ────────────────────────────────────
            btnEditar = new Button
            {
                Text      = "✎  Editar perfil",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Size      = new Size(160, 38),
                Location  = new Point(460, 544),
                FlatStyle = FlatStyle.Flat,
                BackColor = ACCENT,
                ForeColor = Color.FromArgb(10, 12, 18),
                Cursor    = Cursors.Hand
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Click += BtnEditar_Click;
            this.Controls.Add(btnEditar);

            btnGuardar = new Button
            {
                Text      = "✔  Guardar",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Size      = new Size(140, 38),
                Location  = new Point(480, 544),
                FlatStyle = FlatStyle.Flat,
                BackColor = COLOR_OK,
                ForeColor = Color.FromArgb(10, 12, 18),
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            btnCancelar = new Button
            {
                Text      = "Cancelar",
                Font      = new Font("Segoe UI", 10f),
                Size      = new Size(110, 38),
                Location  = new Point(360, 544),
                FlatStyle = FlatStyle.Flat,
                BackColor = BG_INPUT,
                ForeColor = TEXT_MUTED,
                Cursor    = Cursors.Hand,
                Visible   = false
            };
            btnCancelar.FlatAppearance.BorderColor = BORDER;
            btnCancelar.Click += BtnCancelar_Click;
            this.Controls.Add(btnCancelar);

            // Iniciar en modo solo lectura
            SetModoEdicion(false);
        }

        // ── Helper para agregar campo label + textbox ──────
        private void AgregarCampo(string etiqueta, int x, int y, int width, out TextBox txt)
        {
            pnlForm.Controls.Add(new Label
            {
                Text      = etiqueta,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(x, y)
            });

            txt = new TextBox
            {
                Size        = new Size(width, 36),
                Location    = new Point(x, y + 18),
                BackColor   = BG_INPUT,
                ForeColor   = TEXT_PRIMARY,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 10.5f),
                ReadOnly    = true
            };
            txt.GotFocus  += (s, e) => { if (!((TextBox)s).ReadOnly) ((TextBox)s).BackColor = BG_INPUT_HOV; };
            txt.LostFocus += (s, e) => ((TextBox)s).BackColor = ((TextBox)s).ReadOnly ? Color.FromArgb(20, 25, 32) : BG_INPUT;
            pnlForm.Controls.Add(txt);
        }

        // ── Cargar datos del usuario en los campos ─────────
        private void CargarDatos()
        {
            // Recargar desde BD para tener datos frescos
            var u = _dao.ObtenerPorId(_usuario.IdUser) ?? _usuario;

            // Avatar — inicial del nombre
            lblAvatar.Text = u.PrimerNombre.Substring(0, 1).ToUpper();
            lblAvatar.Invalidate();

            lblNombreCompleto.Text = $"{u.PrimerNombre} {u.SegundoNombre ?? ""} {u.PrimerApellido} {u.SegundoApellido ?? ""}".Trim();
            lblNombreCompleto.Text = System.Text.RegularExpressions.Regex.Replace(lblNombreCompleto.Text, @"\s+", " ");
            lblIdUsuario.Text      = $"ID: {u.IdUser}";
            lblFechaRegistro.Text  = $"Miembro desde: {u.FechaRegistro:dd MMMM yyyy}";

            txtPrimerNombre.Text    = u.PrimerNombre    ?? "";
            txtSegundoNombre.Text   = u.SegundoNombre   ?? "";
            txtPrimerApellido.Text  = u.PrimerApellido  ?? "";
            txtSegundoApellido.Text = u.SegundoApellido ?? "";
            txtCorreo.Text          = u.Correo          ?? "";
            txtSalario.Text         = u.SalarioBase.ToString("N2");
        }

        // ── Alternar modo edición / solo lectura ───────────
        private void SetModoEdicion(bool edicion)
        {
            _modoEdicion = edicion;

            // Campos editables
            foreach (var txt in new[] { txtPrimerNombre, txtSegundoNombre, txtPrimerApellido, txtSegundoApellido, txtSalario })
            {
                txt.ReadOnly  = !edicion;
                txt.BackColor = edicion ? BG_INPUT : Color.FromArgb(20, 25, 32);
                txt.ForeColor = edicion ? TEXT_PRIMARY : TEXT_MUTED;
            }

            btnEditar.Visible   = !edicion;
            btnGuardar.Visible  =  edicion;
            btnCancelar.Visible =  edicion;
            lblMensaje.Visible  = false;
        }

        // ── Botón Editar ───────────────────────────────────
        private void BtnEditar_Click(object sender, EventArgs e)
        {
            SetModoEdicion(true);
            txtPrimerNombre.Focus();
        }

        // ── Botón Cancelar ─────────────────────────────────
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            CargarDatos();
            SetModoEdicion(false);
        }

        // ── Botón Guardar ──────────────────────────────────
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            if (string.IsNullOrWhiteSpace(txtPrimerNombre.Text))
            { MostrarMensaje("El primer nombre es obligatorio.", false); txtPrimerNombre.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtPrimerApellido.Text))
            { MostrarMensaje("El primer apellido es obligatorio.", false); txtPrimerApellido.Focus(); return; }

            if (!decimal.TryParse(txtSalario.Text.Replace(",", ""), out decimal salario) || salario < 0)
            { MostrarMensaje("El salario debe ser un número válido mayor o igual a 0.", false); txtSalario.Focus(); return; }

            try
            {
                btnGuardar.Enabled = false;
                btnGuardar.Text    = "Guardando...";

                _usuario.PrimerNombre    = txtPrimerNombre.Text.Trim();
                _usuario.SegundoNombre   = string.IsNullOrWhiteSpace(txtSegundoNombre.Text)   ? null : txtSegundoNombre.Text.Trim();
                _usuario.PrimerApellido  = txtPrimerApellido.Text.Trim();
                _usuario.SegundoApellido = string.IsNullOrWhiteSpace(txtSegundoApellido.Text) ? null : txtSegundoApellido.Text.Trim();
                _usuario.SalarioBase     = salario;

                _dao.Actualizar(_usuario);

                CargarDatos();
                SetModoEdicion(false);
                MostrarMensaje("✓  Perfil actualizado correctamente.", true);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al guardar: " + ex.Message, false);
            }
            finally
            {
                btnGuardar.Enabled = true;
                btnGuardar.Text    = "✔  Guardar";
            }
        }

        private void MostrarMensaje(string msg, bool exito)
        {
            lblMensaje.Text      = msg;
            lblMensaje.ForeColor = exito ? COLOR_OK : COLOR_ERR;
            lblMensaje.Visible   = true;
        }
    }
}
