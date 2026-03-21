using System;
using System.Windows.Forms;
using TBD1_App.Backend;
using TBD1_App.Frontend;

namespace TBD1_App
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Verificar conexión a Oracle antes de arrancar
            if (!OracleHelper.TestConnection())
            {
                MessageBox.Show(
                    "No se pudo conectar a Oracle Database.\n\n" +
                    "Verifica que:\n" +
                    "  • Oracle XE esté iniciado\n" +
                    "  • El listener esté activo (lsnrctl start)\n" +
                    "  • Host: localhost  Puerto: 1521  SID: XE",
                    "Error de Conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FrmLogin());
        }
    }
}
