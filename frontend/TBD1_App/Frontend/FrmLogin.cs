using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmLogin : Form
    {
        // ── Controles ──────────────────────────────────────
        private Panel pnlIzquierdo;
        private Panel pnlDerecho;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblBienvenida;
        private Label lblInstruccion;
        private Label lblCorreo;
        private Label lblPassword;
        private TextBox txtCorreo;
        private TextBox txtPassword;
        private Button btnLogin;
        private LinkLabel lnkRegistro;
        private Label lblError;
        private PictureBox picLogo;
        private Label lblVersion;

        // ── Colores del tema oscuro ────────────────────────
        private readonly Color BG_DARK       = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL      = Color.FromArgb(22,  27,  34);
        private readonly Color BG_INPUT      = Color.FromArgb(30,  37,  48);
        private readonly Color ACCENT        = Color.FromArgb(56, 189, 248);   // azul cielo
        private readonly Color ACCENT_HOVER  = Color.FromArgb(14, 165, 233);
        private readonly Color TEXT_PRIMARY  = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED    = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER        = Color.FromArgb(48,  54,  61);
        private readonly Color ERROR_COLOR   = Color.FromArgb(248,  81,  73);
        private readonly Color SUCCESS_COLOR = Color.FromArgb(63,  185, 120);

        private readonly UsuarioDAO _dao = new UsuarioDAO();

        public static Usuario UsuarioActual { get; private set; }

        public FrmLogin()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            // ── Configuración del Form ─────────────────────
            this.Text            = "Presupuesto Personal — Iniciar Sesión";
            this.Size            = new Size(900, 580);
            this.MinimumSize     = new Size(900, 580);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            // ── Panel izquierdo — branding ─────────────────
            pnlIzquierdo = new Panel
            {
                Size      = new Size(380, 580),
                Location  = new Point(0, 0),
                BackColor = Color.FromArgb(10, 12, 18)
            };
            pnlIzquierdo.Paint += PnlIzquierdo_Paint;
            this.Controls.Add(pnlIzquierdo);

            // Título grande
            lblTitulo = new Label
            {
                Text      = "PRESUPUESTO\nPERSONAL",
                Font      = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = false,
                Size      = new Size(320, 120),
                Location  = new Point(30, 180),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlIzquierdo.Controls.Add(lblTitulo);

            lblSubtitulo = new Label
            {
                Text      = "Steve Valladares\n22341344",
                Font      = new Font("Segoe UI", 11f),
                ForeColor = TEXT_MUTED,
                AutoSize  = false,
                Size      = new Size(320, 60),
                Location  = new Point(30, 310),
                TextAlign = ContentAlignment.TopLeft
            };
            pnlIzquierdo.Controls.Add(lblSubtitulo);

   

            // ── Panel derecho — formulario ─────────────────
            pnlDerecho = new Panel
            {
                Size      = new Size(520, 580),
                Location  = new Point(380, 0),
                BackColor = BG_PANEL
            };
            this.Controls.Add(pnlDerecho);

            lblBienvenida = new Label
            {
                Text      = "Teoria de Base de Datos I",
                Font      = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(60, 100)
            };
            pnlDerecho.Controls.Add(lblBienvenida);

            
            // ── Campo Correo ───────────────────────────────
            lblCorreo = new Label
            {
                Text      = "CORREO ELECTRÓNICO",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(60, 200)
            };
            pnlDerecho.Controls.Add(lblCorreo);

            txtCorreo = CrearTextBox(new Point(60, 222), 400);

            txtCorreo.KeyDown        += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };
            pnlDerecho.Controls.Add(txtCorreo);

            // ── Campo Password ─────────────────────────────
            lblPassword = new Label
            {
                Text      = "CONTRASEÑA",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(60, 292)
            };
            pnlDerecho.Controls.Add(lblPassword);

            txtPassword = CrearTextBox(new Point(60, 314), 400);

            txtPassword.PasswordChar    = '•';
            txtPassword.KeyDown        += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };
            pnlDerecho.Controls.Add(txtPassword);

            // ── Label de error ─────────────────────────────
            lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = ERROR_COLOR,
                AutoSize  = false,
                Size      = new Size(400, 24),
                Location  = new Point(60, 384),
                Visible   = false
            };
            pnlDerecho.Controls.Add(lblError);

            // ── Botón Login ────────────────────────────────
            btnLogin = new Button
            {
                Text      = "INICIAR SESIÓN",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Size      = new Size(400, 46),
                Location  = new Point(60, 414),
                FlatStyle = FlatStyle.Flat,
                BackColor = ACCENT,
                ForeColor = Color.FromArgb(10, 12, 18),
                Cursor    = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize  = 0;
            btnLogin.FlatAppearance.MouseOverBackColor   = ACCENT_HOVER;
            btnLogin.FlatAppearance.MouseDownBackColor   = Color.FromArgb(2, 132, 199);
            btnLogin.Click       += BtnLogin_Click;
            btnLogin.MouseEnter  += (s, e) => btnLogin.BackColor = ACCENT_HOVER;
            btnLogin.MouseLeave  += (s, e) => btnLogin.BackColor = ACCENT;
            EstiloRedondeado(btnLogin, 8);
            pnlDerecho.Controls.Add(btnLogin);

            // ── Link a registro ────────────────────────────
            lnkRegistro = new LinkLabel
            {
                Text      = "¿No tienes cuenta?  Crear cuenta nueva",
                Font      = new Font("Segoe UI", 9.5f),
                AutoSize  = true,
                Location  = new Point(60, 477),
                LinkColor = ACCENT,
                ActiveLinkColor = ACCENT_HOVER,
                VisitedLinkColor = ACCENT
            };
            lnkRegistro.LinkArea = new LinkArea(20, 20);
            lnkRegistro.Click   += LnkRegistro_Click;
            pnlDerecho.Controls.Add(lnkRegistro);

            // Focus inicial
            this.ActiveControl = txtCorreo;
        }

        // ── Paint del panel izquierdo — línea de acento ───
        private void PnlIzquierdo_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            // Línea vertical de acento
            using (var brush = new LinearGradientBrush(
                new Point(370, 0), new Point(370, 580),
                Color.FromArgb(0, ACCENT), ACCENT))
            {
                g.FillRectangle(brush, 373, 0, 4, 580);
            }

            // Círculo decorativo difuso en esquina superior
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(-100, -100, 400, 400);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor    = Color.FromArgb(20, ACCENT);
                    pgb.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }
        }

        // ── Helper: crea TextBox con estilo dark ──────────
        private TextBox CrearTextBox(Point location, int width)
        {
            var txt = new TextBox
            {
                Size            = new Size(width, 44),
                Location        = location,
                BackColor       = BG_INPUT,
                ForeColor       = TEXT_PRIMARY,
                BorderStyle     = BorderStyle.FixedSingle,
                Font            = new Font("Segoe UI", 11f),
                Padding         = new Padding(8, 0, 0, 0)
            };
            txt.GotFocus  += (s, e) => ((TextBox)s).BackColor = Color.FromArgb(38, 46, 60);
            txt.LostFocus += (s, e) => ((TextBox)s).BackColor = BG_INPUT;
            return txt;
        }

        // ── Helper: bordes redondeados ─────────────────────
        private void EstiloRedondeado(Control ctrl, int radio)
        {
            var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
            int d    = radio * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            ctrl.Region = new Region(path);
        }

        // ── Mostrar / ocultar error ────────────────────────
        private void MostrarError(string mensaje)
        {
            lblError.Text    = "⚠  " + mensaje;
            lblError.Visible = true;
        }

        private void OcultarError()
        {
            lblError.Text    = "";
            lblError.Visible = false;
        }

        // ── Evento Login ───────────────────────────────────
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            OcultarError();

            string correo   = txtCorreo.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(correo))
            {
                MostrarError("El correo es obligatorio.");
                txtCorreo.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MostrarError("La contraseña es obligatoria.");
                txtPassword.Focus();
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                btnLogin.Text    = "Verificando...";

                var usuario = _dao.Login(correo, password);

                if (usuario == null)
                {
                    MostrarError("Correo o contraseña incorrectos.");
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                // Login exitoso — abrir Menú Principal
                UsuarioActual = usuario;

                var menu = new FrmMenu(usuario);
                menu.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MostrarError("Error de conexión: " + ex.Message);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text    = "INICIAR SESIÓN";
            }
        }

        // ── Evento Registro ────────────────────────────────
        private void LnkRegistro_Click(object sender, EventArgs e)
        {
            var registro = new FrmRegistro();
            registro.ShowDialog(this);

            // Si el registro fue exitoso, pre-llenar el correo
            if (!string.IsNullOrEmpty(registro.CorreoRegistrado))
            {
                txtCorreo.Text = registro.CorreoRegistrado;
                txtPassword.Focus();
            }
        }

        // ── Al cerrar Dashboard, volver al Login ───────────
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }
    }
}
