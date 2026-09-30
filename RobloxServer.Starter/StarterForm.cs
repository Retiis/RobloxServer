using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RobloxServer.Starter
{
    /// <summary>
    /// The IniciarServidor window: site status, Open / Stop buttons, "keep this computer on",
    /// "start with Windows" and the IIS Express log. Minimizes to the notification area.
    /// </summary>
    public class StarterForm : Form
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string RunValue = "RobloxServer";

        readonly SiteRunner runner;
        readonly bool startMinimized;
        readonly Label status;
        readonly Button startStop;
        readonly Button open;
        readonly CheckBox keepAwake;
        readonly CheckBox startWithWindows;
        readonly CheckBox allowRemote;
        readonly TextBox share;
        bool changingRemote;

        /// <summary>The port was still reserved for other PCs, so "Aceitar outros PCs" was switched back on.</summary>
        readonly bool remoteRestored;
        readonly TextBox log;
        readonly NotifyIcon tray;
        bool exiting;

        public StarterForm(string sitePath, int port, bool startMinimized)
        {
            this.startMinimized = startMinimized;
            // While http://*:port/ is reserved IIS Express cannot register http://localhost:port/
            // ("Acesso negado", 0x80070005), so a leftover reservation means the site has to accept other PCs.
            if (!NetworkAccess.AllowRemote && NetworkAccess.IsReserved(port))
            {
                NetworkAccess.AllowRemote = true;
                remoteRestored = true;
            }
            runner = new SiteRunner(sitePath, port) { AllowRemote = NetworkAccess.AllowRemote };

            Text = "RobloxServer";
            Icon = LoadIcon();
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(560, 430);
            MinimumSize = new Size(480, 370);
            Font = new Font("Segoe UI", 9f);
            BackColor = Color.White;

            var header = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(0x11, 0x40, 0x81) };
            header.Controls.Add(new Label
            {
                Text = "RobloxServer",
                ForeColor = Color.White,
                Font = new Font("Arial", 15f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 9)
            });

            status = new Label
            {
                Text = "Iniciando...",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(12, 54),
                Size = new Size(536, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            open = new Button { Text = "Abrir o site", Location = new Point(12, 82), Size = new Size(110, 28), Enabled = false };
            open.Click += (s, e) => OpenSite();

            startStop = new Button { Text = "Parar", Location = new Point(130, 82), Size = new Size(110, 28), Enabled = false };
            startStop.Click += (s, e) => ToggleSite();

            keepAwake = new CheckBox
            {
                Text = "Manter o computador ligado (não entrar em suspensão)",
                Checked = true,
                AutoSize = true,
                Location = new Point(12, 120),
                Enabled = KeepAwake.IsSupported
            };
            keepAwake.CheckedChanged += (s, e) => ApplyKeepAwake();

            startWithWindows = new CheckBox
            {
                Text = "Iniciar o site junto com o Windows",
                Checked = IsStartingWithWindows(),
                AutoSize = true,
                Location = new Point(12, 144)
            };
            startWithWindows.CheckedChanged += (s, e) => SetStartWithWindows(startWithWindows.Checked);

            allowRemote = new CheckBox
            {
                Text = "Aceitar outros PCs (Radmin VPN / rede local)",
                Checked = runner.AllowRemote,
                AutoSize = true,
                Location = new Point(12, 168)
            };
            allowRemote.CheckedChanged += (s, e) => ChangeAllowRemote();

            // A read-only text box, so the address can be copied and sent to a friend.
            share = new TextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(0x08, 0x52, 0xb7),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(30, 194),
                Size = new Size(518, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            log = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(0xf1, 0xf1, 0xf1),
                Font = new Font("Consolas", 8.5f),
                Location = new Point(12, 222),
                Size = new Size(536, 196),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(log);
            Controls.Add(share);
            Controls.Add(allowRemote);
            Controls.Add(startWithWindows);
            Controls.Add(keepAwake);
            Controls.Add(startStop);
            Controls.Add(open);
            Controls.Add(status);
            Controls.Add(header);

            var menu = new ContextMenuStrip();
            menu.Items.Add("Mostrar", null, (s, e) => ShowWindow());
            menu.Items.Add("Abrir o site", null, (s, e) => OpenSite());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Parar e sair", null, (s, e) => ExitApplication());
            tray = new NotifyIcon { Icon = Icon, Text = "RobloxServer", ContextMenuStrip = menu, Visible = true };
            tray.DoubleClick += (s, e) => ShowWindow();

            runner.Output += line => OnUi(() => AppendLog(line));
            runner.RunningChanged += running => OnUi(UpdateStatus);
        }

        static Icon LoadIcon()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RobloxServer.Starter.RobloxServer.ico"))
            {
                return stream != null ? new Icon(stream) : SystemIcons.Application;
            }
        }

        void OnUi(Action action)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }
            try
            {
                BeginInvoke(action);
            }
            catch (InvalidOperationException)
            {
                // Closing.
            }
        }

        void AppendLog(string line)
        {
            if (log.TextLength > 200000)
            {
                log.Text = log.Text.Substring(100000);
            }
            log.AppendText(line + Environment.NewLine);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyKeepAwake();
            AppendLog("Site: " + runner.SitePath);
            if (remoteRestored)
            {
                AppendLog("A porta " + runner.Port + " continua liberada para outros PCs no Windows, então \"Aceitar outros PCs\" foi marcado de novo."
                    + " Para só este PC acessar, desmarque (o Windows pede permissão de administrador para fechar a porta).");
            }
            if (!runner.AllowRemote)
            {
                AppendLog("O site só atende este PC. Para um amigo pelo Radmin VPN, marque \"Aceitar outros PCs\".");
            }
            else if (!NetworkAccess.IsConfigured(runner.Port))
            {
                AppendLog("A porta " + runner.Port + " não está liberada para outros PCs. Desmarque e marque \"Aceitar outros PCs\" de novo.");
            }
            if (startMinimized)
            {
                Hide();
            }
            StartSite();
        }

        void StartSite()
        {
            startStop.Enabled = false;
            status.Text = "Iniciando...";
            Task.Run(() =>
            {
                bool started = runner.Start();
                OnUi(() =>
                {
                    UpdateStatus();
                    if (started && !startMinimized)
                    {
                        OpenSite();
                    }
                });
            });
        }

        void ToggleSite()
        {
            if (runner.IsRunning)
            {
                startStop.Enabled = false;
                Task.Run(() => runner.Stop());
            }
            else
            {
                StartSite();
            }
        }

        void UpdateStatus()
        {
            bool running = runner.IsRunning;
            status.Text = running ? "Rodando em " + runner.Url : "Parado";
            status.ForeColor = running ? Color.FromArgb(0x36, 0xa4, 0x00) : Color.FromArgb(0xc0, 0x00, 0x00);
            startStop.Text = running ? "Parar" : "Iniciar";
            startStop.Enabled = true;
            open.Enabled = running;
            allowRemote.Enabled = !changingRemote;

            var addresses = NetworkAccess.Addresses();
            if (running && runner.AllowRemote && addresses.Count > 0)
            {
                var best = addresses[0];
                share.Text = "Outros PCs abrem: http://" + best.Key + ":" + runner.Port + "/" + (NetworkAccess.IsRadmin(best) ? "  (Radmin VPN)" : "  (" + best.Value + ")");
            }
            else
            {
                share.Text = runner.AllowRemote ? "" : "Só este PC acessa o site.";
            }
            tray.Text = running ? "RobloxServer - rodando na porta " + runner.Port : "RobloxServer - parado";
        }

        void ChangeAllowRemote()
        {
            if (changingRemote)
            {
                return;
            }
            bool enable = allowRemote.Checked;
            if (enable && !NetworkAccess.IsConfigured(runner.Port))
            {
                DialogResult answer = MessageBox.Show(this,
                    "Para outros PCs (Radmin VPN, rede local) entrarem no site, o Windows precisa liberar a porta " + runner.Port
                    + " (site) e a porta UDP " + NetworkAccess.GamePort + " (servidores de jogo) no firewall.\r\n\r\n"
                    + "O Windows vai pedir permissão de administrador uma vez. Continuar?",
                    "RobloxServer", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (answer != DialogResult.OK)
                {
                    SetAllowRemoteBox(false);
                    return;
                }
            }
            if (!enable && NetworkAccess.IsReserved(runner.Port))
            {
                DialogResult answer = MessageBox.Show(this,
                    "Para só este PC acessar o site, o Windows precisa fechar a porta " + runner.Port + " para os outros PCs.\r\n\r\n"
                    + "O Windows vai pedir permissão de administrador. Continuar?",
                    "RobloxServer", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (answer != DialogResult.OK)
                {
                    SetAllowRemoteBox(true);
                    return;
                }
            }

            changingRemote = true;
            allowRemote.Enabled = false;
            startStop.Enabled = false;
            Task.Run(() =>
            {
                string error = enable
                    ? (NetworkAccess.IsConfigured(runner.Port) ? null : NetworkAccess.Configure(runner.Port))
                    : (NetworkAccess.IsReserved(runner.Port) ? NetworkAccess.Remove(runner.Port) : null);
                if (error != null)
                {
                    OnUi(() =>
                    {
                        AppendLog(enable ? "Não foi possível liberar o acesso de outros PCs: " + error
                                         : "Não foi possível fechar a porta para outros PCs: " + error);
                        changingRemote = false;
                        SetAllowRemoteBox(!enable);
                        UpdateStatus();
                    });
                    return;
                }

                NetworkAccess.AllowRemote = enable;
                runner.AllowRemote = enable;
                bool wasRunning = runner.IsRunning;
                if (wasRunning)
                {
                    runner.Stop();
                    runner.Start();
                }
                OnUi(() =>
                {
                    changingRemote = false;
                    if (enable)
                    {
                        foreach (var address in NetworkAccess.Addresses())
                        {
                            AppendLog("Outros PCs: http://" + address.Key + ":" + runner.Port + "/  (" + address.Value + ")");
                        }
                    }
                    else
                    {
                        AppendLog("Agora só este PC acessa o site.");
                    }
                    UpdateStatus();
                });
            });
        }

        void SetAllowRemoteBox(bool value)
        {
            changingRemote = true;
            allowRemote.Checked = value;
            changingRemote = false;
        }

        void ApplyKeepAwake()
        {
            bool ok = KeepAwake.Set(keepAwake.Checked);
            if (keepAwake.Checked)
            {
                AppendLog(ok ? "O computador não vai entrar em suspensão enquanto este programa estiver aberto."
                             : "Não foi possível impedir a suspensão neste sistema.");
            }
        }

        void OpenSite()
        {
            try
            {
                Process.Start(runner.Url);
            }
            catch (Exception ex)
            {
                AppendLog("Não foi possível abrir o navegador: " + ex.Message);
            }
        }

        void ShowWindow()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        static bool IsStartingWithWindows()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey))
                {
                    return key != null && key.GetValue(RunValue) != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        void SetStartWithWindows(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (enabled)
                    {
                        key.SetValue(RunValue, "\"" + Application.ExecutablePath + "\" --port " + runner.Port + " --minimized");
                    }
                    else
                    {
                        key.DeleteValue(RunValue, false);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog("Não foi possível mudar a inicialização com o Windows: " + ex.Message);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                tray.ShowBalloonTip(3000, "RobloxServer", "O site continua rodando. Clique duas vezes aqui para abrir.", ToolTipIcon.Info);
            }
        }

        void ExitApplication()
        {
            exiting = true;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!exiting && e.CloseReason == CloseReason.UserClosing && runner.IsRunning)
            {
                DialogResult answer = MessageBox.Show(this, "Parar o RobloxServer e fechar?\r\n\r\nClique em Não para deixar o site rodando na área de notificação.",
                    "RobloxServer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.No)
                {
                    e.Cancel = true;
                    Hide();
                    return;
                }
                if (answer == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            runner.Stop();
            KeepAwake.Set(false);
            tray.Visible = false;
            tray.Dispose();
            base.OnFormClosing(e);
        }
    }
}
