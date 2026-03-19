using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmMenu : Form
    {
        // ── Colores ────────────────────────────────────────
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_CARD      = Color.FromArgb(30,  37,  48);
        private readonly Color BG_CARD_HOV  = Color.FromArgb(38,  46,  60);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color COLOR_ING    = Color.FromArgb(63,  185, 120);  // verde
        private readonly Color COLOR_GAS    = Color.FromArgb(248,  81,  73);  // rojo
        private readonly Color COLOR_OBL    = Color.FromArgb(251, 191,  36);  // amarillo

        // ── DAOs ───────────────────────────────────────────
        private readonly ObligacionDAO _oblDAO = new ObligacionDAO();

        // ── Usuario en sesión ──────────────────────────────
        private readonly Usuario _usuario;

        // ── Controles de datos ─────────────────────────────
        private Label lblBalanceVal;
        private Label lblIngresosVal;
        private Label lblGastosVal;
        private Label lblObligacionVal;
        private Label lblSaludo;

        public FrmMenu(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarDatos();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Presupuesto Personal — Menú Principal";
            this.Size            = new Size(1000, 680);
            this.MinimumSize     = new Size(1000, 680);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            // ════════════════════════════════════════════════
            // HEADER
            // ════════════════════════════════════════════════
            var pnlHeader = new Panel
            {
                Size      = new Size(1000, 90),
                Location  = new Point(0, 0),
                BackColor = BG_PANEL
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawLine(pen, 0, 89, 1000, 89);
                // línea de acento izquierda
                using (var brush = new SolidBrush(ACCENT))
                    e.Graphics.FillRectangle(brush, 0, 0, 4, 90);
            };
            this.Controls.Add(pnlHeader);

            // Título app
            var lblApp = new Label
            {
                Text      = "PRESUPUESTO PERSONAL",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize  = true,
                Location  = new Point(20, 18)
            };
            pnlHeader.Controls.Add(lblApp);

            // Saludo usuario
            lblSaludo = new Label
            {
                Text      = $"Bienvenido, {_usuario.PrimerNombre} {_usuario.PrimerApellido}",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(20, 50)
            };
            pnlHeader.Controls.Add(lblSaludo);

            // Fecha y salario
            var lblFecha = new Label
            {
                Text      = $"{DateTime.Now:dddd, dd MMMM yyyy}  •  Salario base: L {_usuario.SalarioBase:N2}",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(650, 38)
            };
            pnlHeader.Controls.Add(lblFecha);

            // Botón cerrar sesión
            var btnSalir = new Button
            {
                Text      = "Cerrar sesión",
                Font      = new Font("Segoe UI", 9f),
                Size      = new Size(110, 30),
                Location  = new Point(870, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 248, 81, 73),
                ForeColor = Color.FromArgb(248, 81, 73),
                Cursor    = Cursors.Hand
            };
            btnSalir.FlatAppearance.BorderColor = Color.FromArgb(248, 81, 73);
            btnSalir.Click += (s, e) =>
            {
                var frm = new FrmLogin();
                frm.Show();
                this.Close();
            };
            pnlHeader.Controls.Add(btnSalir);

            // ════════════════════════════════════════════════
            // SECCIÓN DATOS RÁPIDOS
            // ════════════════════════════════════════════════
            var lblSecDatos = new Label
            {
                Text      = $"RESUMEN — {DateTime.Now:MMMM yyyy}".ToUpper(),
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(30, 112)
            };
            this.Controls.Add(lblSecDatos);

            // 4 tarjetas de datos
            CrearTarjetaDato(30,  135, "INGRESOS DEL MES",  "Cargando...", COLOR_ING, out lblIngresosVal);
            CrearTarjetaDato(255, 135, "GASTOS DEL MES",    "Cargando...", COLOR_GAS, out lblGastosVal);
            CrearTarjetaDato(480, 135, "BALANCE",           "Cargando...", ACCENT,    out lblBalanceVal);
            CrearTarjetaDato(705, 135, "PRÓX. OBLIGACIÓN",  "Cargando...", COLOR_OBL, out lblObligacionVal);

            // ════════════════════════════════════════════════
            // SECCIÓN MÓDULOS
            // ════════════════════════════════════════════════
            var lblSecModulos = new Label
            {
                Text      = "MÓDULOS",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(30, 250)
            };
            this.Controls.Add(lblSecModulos);

            // Tarjetas de navegación — fila 1
            CrearTarjetaModulo(30,  270, "💰", "Presupuestos",
                "Crear y gestionar presupuestos\nmensuales o por período",
                ACCENT, () => AbrirForm("presupuesto"));

            CrearTarjetaModulo(265, 270, "📊", "Transacciones",
                "Registrar ingresos, gastos\ny ahorros del mes",
                COLOR_ING, () => AbrirForm("transaccion"));

            CrearTarjetaModulo(500, 270, "🗂️", "Categorías",
                "Administrar categorías\ny subcategorías",
                Color.FromArgb(167, 139, 250), () => AbrirForm("categoria"));

            CrearTarjetaModulo(735, 270, "📋", "Obligaciones",
                "Gestionar pagos fijos\nmensuales recurrentes",
                COLOR_OBL, () => AbrirForm("obligacion"));

            // Tarjeta perfil — fila 2 centrada
            CrearTarjetaModulo(265, 470, "👤", "Mi Perfil",
                "Ver y actualizar tu\ninformación personal",
                TEXT_MUTED, () => AbrirForm("perfil"));
        }

        // ── Crea tarjeta de dato rápido ────────────────────
        private void CrearTarjetaDato(int x, int y, string titulo, string valorInicial, Color accentColor, out Label lblValor)
        {
            var pnl = new Panel
            {
                Size      = new Size(210, 85),
                Location  = new Point(x, y),
                BackColor = BG_CARD
            };
            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var brush = new SolidBrush(accentColor))
                    e.Graphics.FillRectangle(brush, 0, 0, 3, pnl.Height);
            };
            this.Controls.Add(pnl);

            var lblTit = new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(12, 12)
            };
            pnl.Controls.Add(lblTit);

            lblValor = new Label
            {
                Text      = valorInicial,
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize  = false,
                Size      = new Size(190, 40),
                Location  = new Point(12, 35),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnl.Controls.Add(lblValor);
        }

        // ── Crea tarjeta de módulo de navegación ───────────
        private void CrearTarjetaModulo(int x, int y, string icono, string titulo, string descripcion, Color accentColor, Action alClick)
        {
            var pnl = new Panel
            {
                Size      = new Size(215, 170),
                Location  = new Point(x, y),
                BackColor = BG_CARD,
                Cursor    = Cursors.Hand
            };

            // Borde y línea superior de color
            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(BORDER, 1))
                    g.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var brush = new SolidBrush(accentColor))
                    g.FillRectangle(brush, 0, 0, pnl.Width, 3);
            };

            // Hover effect
            pnl.MouseEnter += (s, e) => { pnl.BackColor = BG_CARD_HOV; pnl.Invalidate(); };
            pnl.MouseLeave += (s, e) => { pnl.BackColor = BG_CARD;     pnl.Invalidate(); };
            pnl.Click      += (s, e) => alClick();

            this.Controls.Add(pnl);

            // Ícono
            var lblIcono = new Label
            {
                Text      = icono,
                Font      = new Font("Segoe UI Emoji", 28f),
                AutoSize  = true,
                Location  = new Point(15, 20),
                BackColor = Color.Transparent
            };
            lblIcono.Click += (s, e) => alClick();
            pnl.Controls.Add(lblIcono);

            // Título
            var lblTit = new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(15, 80),
                BackColor = Color.Transparent
            };
            lblTit.Click += (s, e) => alClick();
            pnl.Controls.Add(lblTit);

            // Descripción
            var lblDesc = new Label
            {
                Text      = descripcion,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = TEXT_MUTED,
                AutoSize  = false,
                Size      = new Size(185, 40),
                Location  = new Point(15, 112),
                BackColor = Color.Transparent
            };
            lblDesc.Click += (s, e) => alClick();
            pnl.Controls.Add(lblDesc);
        }

        // ── Carga datos desde Oracle ───────────────────────
        private void CargarDatos()
        {
            try
            {
                int anio = DateTime.Now.Year;
                int mes  = DateTime.Now.Month;

                // Balance del mes via SP_CALCULAR_BALANCE_MENSUAL
                var pIngresos = new Oracle.ManagedDataAccess.Client.OracleParameter("P_TOTAL_INGRESOS", Oracle.ManagedDataAccess.Client.OracleDbType.Decimal) { Direction = System.Data.ParameterDirection.Output };
                var pGastos   = new Oracle.ManagedDataAccess.Client.OracleParameter("P_TOTAL_GASTOS",   Oracle.ManagedDataAccess.Client.OracleDbType.Decimal) { Direction = System.Data.ParameterDirection.Output };
                var pAhorros  = new Oracle.ManagedDataAccess.Client.OracleParameter("P_TOTAL_AHORROS",  Oracle.ManagedDataAccess.Client.OracleDbType.Decimal) { Direction = System.Data.ParameterDirection.Output };
                var pBalance  = new Oracle.ManagedDataAccess.Client.OracleParameter("P_BALANCE",        Oracle.ManagedDataAccess.Client.OracleDbType.Decimal) { Direction = System.Data.ParameterDirection.Output };

                OracleHelper.ExecuteProcedure("SP_CALCULAR_BALANCE_MENSUAL",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("P_ID_USER", _usuario.IdUser),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("P_ANIO", anio),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("P_MES",  mes),
                    pIngresos, pGastos, pAhorros, pBalance);

                decimal ingresos = pIngresos.Value == DBNull.Value ? 0 : Convert.ToDecimal(pIngresos.Value.ToString());
                decimal gastos   = pGastos.Value   == DBNull.Value ? 0 : Convert.ToDecimal(pGastos.Value.ToString());
                decimal balance  = pBalance.Value  == DBNull.Value ? 0 : Convert.ToDecimal(pBalance.Value.ToString());

                lblIngresosVal.Text     = $"L {ingresos:N2}";
                lblGastosVal.Text       = $"L {gastos:N2}";
                lblBalanceVal.Text      = $"L {balance:N2}";
                lblBalanceVal.ForeColor = balance >= 0 ? COLOR_ING : COLOR_GAS;

                // Próxima obligación
                var obligaciones = _oblDAO.ObtenerActivas(_usuario.IdUser);
                if (obligaciones.Count > 0)
                {
                    obligaciones.Sort((a, b) => a.Dia.CompareTo(b.Dia));
                    var prox = obligaciones[0];
                    lblObligacionVal.Text = $"{prox.Nombre}\nDía {prox.Dia}  L {prox.Monto:N2}";
                    lblObligacionVal.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else
                {
                    lblObligacionVal.Text = "Sin obligaciones";
                }
            }
            catch
            {
                lblIngresosVal.Text   = "L 0.00";
                lblGastosVal.Text     = "L 0.00";
                lblBalanceVal.Text    = "L 0.00";
                lblObligacionVal.Text = "Sin datos";
            }
        }

        // ── Navegar a los Forms ────────────────────────────
        private void AbrirForm(string modulo)
        {
            switch (modulo)
            {
                case "transaccion":
                    new FrmTransacciones(_usuario).ShowDialog(this);
                    CargarDatos();
                    break;
                // TODO: descomentar cuando cada Form esté listo
                case "presupuesto": new FrmPresupuestos(_usuario).ShowDialog(this); CargarDatos(); break;
                case "categoria": new FrmCategorias(_usuario).ShowDialog(this); break;
                case "obligacion": new FrmObligaciones(_usuario).ShowDialog(this); CargarDatos(); break;
                case "perfil": new FrmPerfil(_usuario).ShowDialog(this); break;
                default:
                    MessageBox.Show($"Módulo '{modulo}' próximamente.",
                        "En construcción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
    }
}
