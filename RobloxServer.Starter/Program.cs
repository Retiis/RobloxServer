using System;
using System.Threading;
using System.Windows.Forms;

namespace RobloxServer.Starter
{
    /// <summary>
    /// IniciarServidor.exe [--port 8080] [--minimized]
    /// Starts the RobloxServer website on this PC with IIS Express and keeps the computer awake.
    /// </summary>
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int port = 8080;
            bool minimized = false;
            for (int i = 0; i < args.Length; i++)
            {
                int number;
                if (args[i] == "--port" && i + 1 < args.Length && int.TryParse(args[i + 1], out number))
                {
                    port = number;
                    i++;
                }
                else if (args[i] == "--minimized")
                {
                    minimized = true;
                }
                else if (int.TryParse(args[i], out number))
                {
                    // Same as iniciar-servidor.bat [porta].
                    port = number;
                }
            }
            if (port <= 0 || port > 65535)
            {
                port = 8080;
            }

            bool firstInstance;
            using (var mutex = new Mutex(true, @"Local\RobloxServer.Starter", out firstInstance))
            {
                if (!firstInstance)
                {
                    MessageBox.Show("O RobloxServer já está aberto (veja a área de notificação, perto do relógio).",
                        "RobloxServer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 1;
                }

                string site = SiteRunner.FindSite();
                if (site == null)
                {
                    MessageBox.Show("A pasta RobloxServer.Web não foi encontrada.\r\n\r\nDeixe o IniciarServidor.exe na mesma pasta que o RobloxServer.Web.",
                        "RobloxServer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }

                Application.Run(new StarterForm(site, port, minimized));
                return 0;
            }
        }
    }
}
