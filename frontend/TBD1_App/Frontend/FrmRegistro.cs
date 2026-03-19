using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmRegistro : Form
    {
        // ── Controles ──────────────────────────────────────
        private Panel      pnlContenido;
        private Label      lblTitulo;
        private Label      lblSubtitulo;
        private Label      lblPrimerNombre,    lblSegundoNombre;
        private Label      lblPrimerApellido,  lblSegundoApellido;
        private Label      lblCorreo,          lblPassword;
        private Label      lblConfirmar,       lblSalario;
        private TextBox    txtPrimerNombre,    txtSegundoNombre;
        private TextBox    txtPrimerApellido,  txtSegundoApellido;
        private TextBox    txtCorreo,          txtPassword;
        private TextBox    txtConfirmar,       txtSalario;
        private Button     btnRegistrar;
        private Button     btnCancelar;
        private Label      lblError;
        private Label      lblExito;

        // ── Colores ────────────────────────────────────────
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT     = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color ERROR_COLOR  = Color.FromArgb(248,  81,  73);
        private readonly Color SUCCESS_COLOR= Color.FromArgb(63,  185, 120);

        private readonly UsuarioDAO _dao = new UsuarioDAO();

        // Expone el correo registrado para pre-llenarlo en Login
        public string CorreoRegistrado { get; private set; } = "";

        public FrmRegistro()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Crear cuenta nueva";
            this.Size            = new Size(620, 660);
            this.MinimumSize     = new Size(620, 660);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── Panel contenedor ───────────────────────────
            pnlContenido = new Panel
            {
                Size      = new Size(560, 600),
                Location  = new Point(30, 20),
                BackColor = BG_PANEL
            };
            pnlContenido.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(48, 54, 61), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlContenido.Width - 1, pnlContenido.Height - 1);
            };
            this.Controls.Add(pnlContenido);

            // ── Encabezado ─────────────────────────────────
            lblTitulo = new Label
            {
                Text      = "Crear cuenta",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(30, 25)
            };
            pnlContenido.Controls.Add(lblTitulo);



            // ── Separador ──────────────────────────────────
            var sep = new Label
            {
                Size      = new Size(500, 1),
                Location  = new Point(30, 80),
                BackColor = Color.FromArgb(48, 54, 61)
            };
            pnlContenido.Controls.Add(sep);

            // ── Fila 1: Primer Nombre / Segundo Nombre ─────
            AgregarLabel("PRIMER NOMBRE *",  30, 95,  out lblPrimerNombre);
            AgregarLabel("SEGUNDO NOMBRE",   295, 95, out lblSegundoNombre);
            txtPrimerNombre    = AgregarTextBox(30,  117, 235);
            txtSegundoNombre   = AgregarTextBox(295, 117, 235);

            // ── Fila 2: Primer Apellido / Segundo Apellido ──
            AgregarLabel("PRIMER APELLIDO *",  30,  180, out lblPrimerApellido);
            AgregarLabel("SEGUNDO APELLIDO",   295, 180, out lblSegundoApellido);
            txtPrimerApellido  = AgregarTextBox(30,  202, 235);
            txtSegundoApellido = AgregarTextBox(295, 202, 235);

            // ── Fila 3: Correo ─────────────────────────────
            AgregarLabel("CORREO ELECTRÓNICO *", 30, 265, out lblCorreo);
            txtCorreo = AgregarTextBox(30, 287, 500);


            // ── Fila 4: Password / Confirmar ───────────────
            AgregarLabel("CONTRASEÑA *",          30,  350, out lblPassword);
            AgregarLabel("CONFIRMAR CONTRASEÑA *", 295, 350, out lblConfirmar);
            txtPassword  = AgregarTextBox(30,  372, 235);
            txtConfirmar = AgregarTextBox(295, 372, 235);
            txtPassword.PasswordChar  = '•';
            txtConfirmar.PasswordChar = '•';

            // ── Fila 5: Salario Base ───────────────────────
            AgregarLabel("SALARIO BASE MENSUAL (L) *", 30, 435, out lblSalario);
            txtSalario = AgregarTextBox(30, 457, 235);


            // ── Mensajes ───────────────────────────────────
            lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = ERROR_COLOR,
                AutoSize  = false,
                Size      = new Size(500, 22),
                Location  = new Point(30, 508),
                Visible   = false
            };
            pnlContenido.Controls.Add(lblError);

            lblExito = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = SUCCESS_COLOR,
                AutoSize  = false,
                Size      = new Size(500, 22),
                Location  = new Point(30, 508),
                Visible   = false
            };
            pnlContenido.Controls.Add(lblExito);

            // ── Botones ────────────────────────────────────
            btnCancelar = new Button
            {
                Text      = "Cancelar",
                Font      = new Font("Segoe UI", 10f),
                Size      = new Size(150, 42),
                Location  = new Point(30, 540),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 37, 48),
                ForeColor = TEXT_MUTED,
                Cursor    = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(48, 54, 61);
            btnCancelar.Click += (s, e) => this.Close();
            pnlContenido.Controls.Add(btnCancelar);

            btnRegistrar = new Button
            {
                Text      = "CREAR CUENTA",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Size      = new Size(330, 42),
                Location  = new Point(200, 540),
                FlatStyle = FlatStyle.Flat,
                BackColor = ACCENT,
                ForeColor = Color.FromArgb(10, 12, 18),
                Cursor    = Cursors.Hand
            };
            btnRegistrar.FlatAppearance.BorderSize = 0;
            btnRegistrar.Click += BtnRegistrar_Click;
            pnlContenido.Controls.Add(btnRegistrar);

            this.ActiveControl = txtPrimerNombre;
        }

        // ── Helpers para crear controles ───────────────────
        private void AgregarLabel(string texto, int x, int y, out Label lbl)
        {
            lbl = new Label
            {
                Text      = texto,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(x, y)
            };
            pnlContenido.Controls.Add(lbl);
        }

        private TextBox AgregarTextBox(int x, int y, int width)
        {
            var txt = new TextBox
            {
                Size        = new Size(width, 42),
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

        // ── Validaciones y registro ────────────────────────
        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            lblExito.Visible = false;

            // Validaciones
            if (string.IsNullOrWhiteSpace(txtPrimerNombre.Text))
            { MostrarError("El primer nombre es obligatorio."); txtPrimerNombre.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtPrimerApellido.Text))
            { MostrarError("El primer apellido es obligatorio."); txtPrimerApellido.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
            { MostrarError("El correo es obligatorio."); txtCorreo.Focus(); return; }

            if (!txtCorreo.Text.Contains("@"))
            { MostrarError("El correo no tiene un formato válido."); txtCorreo.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            { MostrarError("La contraseña es obligatoria."); txtPassword.Focus(); return; }

            if (txtPassword.Text.Length < 6)
            { MostrarError("La contraseña debe tener al menos 6 caracteres."); txtPassword.Focus(); return; }

            if (txtPassword.Text != txtConfirmar.Text)
            { MostrarError("Las contraseñas no coinciden."); txtConfirmar.Focus(); return; }

            decimal salario = 0;
            if (!string.IsNullOrWhiteSpace(txtSalario.Text))
            {
                if (!decimal.TryParse(txtSalario.Text, out salario) || salario < 0)
                { MostrarError("El salario debe ser un número válido mayor o igual a 0."); txtSalario.Focus(); return; }
            }

            try
            {
                btnRegistrar.Enabled = false;
                btnRegistrar.Text    = "Registrando...";

                var usuario = new Usuario
                {
                    PrimerNombre    = txtPrimerNombre.Text.Trim(),
                    SegundoNombre   = txtSegundoNombre.Text.Trim(),
                    PrimerApellido  = txtPrimerApellido.Text.Trim(),
                    SegundoApellido = txtSegundoApellido.Text.Trim(),
                    Correo          = txtCorreo.Text.Trim(),
                    Password        = txtPassword.Text,
                    SalarioBase     = salario
                };

                string nuevoId = _dao.Insertar(usuario);

                CorreoRegistrado = usuario.Correo;

                lblExito.Text    = $"✓  Cuenta creada exitosamente. ID: {nuevoId}";
                lblExito.Visible = true;

                // Cerrar automáticamente después de 1.5 segundos
                var timer = new System.Windows.Forms.Timer { Interval = 1500 };
                timer.Tick += (ts, te) => { timer.Stop(); this.Close(); };
                timer.Start();
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Contains("ORA-20001")
                    ? "Ya existe una cuenta con ese correo."
                    : "Error al crear la cuenta: " + ex.Message;
                MostrarError(msg);
            }
            finally
            {
                btnRegistrar.Enabled = true;
                btnRegistrar.Text    = "CREAR CUENTA";
            }
        }

        private void MostrarError(string msg)
        {
            lblError.Text    = "⚠  " + msg;
            lblError.Visible = true;
            lblExito.Visible = false;
        }
    }
}
