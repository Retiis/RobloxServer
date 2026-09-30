using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;

namespace RobloxServer.Starter
{
    /// <summary>
    /// "Allow other PCs": IIS Express only answers "localhost" unless the URL http://*:port/ is reserved
    /// for normal users (netsh http add urlacl) and Windows Firewall lets the port in. Both need
    /// administrator rights once, so they run in a small elevated script.
    /// </summary>
    public static class NetworkAccess
    {
        const string SettingsKey = @"Software\RobloxServer";
        const string AllowRemoteValue = "AllowRemote";

        /// <summary>UDP port RobloxPlayerLauncher uses for game servers (HostPort in its Settings.ini).</summary>
        public const int GamePort = 53640;

        public static bool AllowRemote
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(SettingsKey))
                    {
                        return key != null && Convert.ToInt32(key.GetValue(AllowRemoteValue, 0)) == 1;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(SettingsKey))
                    {
                        key.SetValue(AllowRemoteValue, value ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        static string UrlFor(int port)
        {
            return "http://*:" + port + "/";
        }

        static string SiteRuleName(int port)
        {
            return "RobloxServer site (TCP " + port + ")";
        }

        static string GameRuleName()
        {
            return "RobloxServer jogo (UDP " + GamePort + ")";
        }

        /// <summary>True when the URL reservation and both firewall rules exist.</summary>
        public static bool IsConfigured(int port)
        {
            return IsReserved(port) && RuleExists(SiteRuleName(port)) && RuleExists(GameRuleName());
        }

        /// <summary>
        /// True when http://*:port/ is reserved. While it is, IIS Express can no longer register
        /// http://localhost:port/ ("Access denied", 0x80070005), so the site must bind *:port: too.
        /// </summary>
        public static bool IsReserved(int port)
        {
            string urlacl = Run("netsh", "http show urlacl url=" + UrlFor(port));
            return urlacl != null && urlacl.IndexOf(UrlFor(port), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool RuleExists(string name)
        {
            return Run("netsh", "advfirewall firewall show rule name=\"" + name + "\"") != null;
        }

        /// <summary>
        /// Reserves the URL and opens the firewall, asking for administrator rights (UAC).
        /// Returns null on success, or what went wrong.
        /// </summary>
        public static string Configure(int port)
        {
            // D:(A;;GX;;;WD) = everyone may listen; unlike user=Everyone it works in every Windows language.
            string error = RunElevated(new[]
            {
                "netsh http delete urlacl url=" + UrlFor(port) + " >nul 2>&1",
                "netsh http add urlacl url=" + UrlFor(port) + " sddl=D:(A;;GX;;;WD)",
                "netsh advfirewall firewall delete rule name=\"" + SiteRuleName(port) + "\" >nul 2>&1",
                "netsh advfirewall firewall add rule name=\"" + SiteRuleName(port) + "\" dir=in action=allow protocol=TCP localport=" + port,
                "netsh advfirewall firewall delete rule name=\"" + GameRuleName() + "\" >nul 2>&1",
                "netsh advfirewall firewall add rule name=\"" + GameRuleName() + "\" dir=in action=allow protocol=UDP localport=" + GamePort
            });
            return error ?? (IsConfigured(port) ? null : "O netsh não conseguiu liberar a porta " + port + ".");
        }

        /// <summary>Undoes Configure (UAC again): only this PC can open the site afterwards.</summary>
        public static string Remove(int port)
        {
            string error = RunElevated(new[]
            {
                "netsh http delete urlacl url=" + UrlFor(port) + " >nul 2>&1",
                "netsh advfirewall firewall delete rule name=\"" + SiteRuleName(port) + "\" >nul 2>&1",
                "netsh advfirewall firewall delete rule name=\"" + GameRuleName() + "\" >nul 2>&1"
            });
            return error ?? (IsReserved(port) ? "O netsh não conseguiu remover a reserva da porta " + port + "." : null);
        }

        /// <summary>Runs the commands in a hidden elevated script. Returns null, or why it did not run.</summary>
        static string RunElevated(string[] commands)
        {
            string script = Path.Combine(Path.GetTempPath(), "robloxserver-rede.cmd");
            File.WriteAllText(script, "@echo off\r\n" + string.Join("\r\n", commands) + "\r\n");
            try
            {
                var info = new ProcessStartInfo(script)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(info))
                {
                    p.WaitForExit();
                }
                return null;
            }
            catch (Win32Exception ex)
            {
                // 1223: the user clicked No on the UAC prompt.
                return ex.NativeErrorCode == 1223 ? "A permissão de administrador foi negada." : ex.Message;
            }
            finally
            {
                try
                {
                    File.Delete(script);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>This PC's IPv4 addresses other PCs can use, Radmin VPN (26.x.x.x) first.</summary>
        public static List<KeyValuePair<string, string>> Addresses()
        {
            var list = new List<KeyValuePair<string, string>>();
            try
            {
                foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (adapter.OperationalStatus != OperationalStatus.Up || adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }
                    foreach (UnicastIPAddressInformation address in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork && !address.Address.ToString().StartsWith("169.254."))
                        {
                            list.Add(new KeyValuePair<string, string>(address.Address.ToString(), adapter.Name));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return list.OrderByDescending(a => IsRadmin(a)).ToList();
        }

        public static bool IsRadmin(KeyValuePair<string, string> address)
        {
            return address.Value.IndexOf("Radmin", StringComparison.OrdinalIgnoreCase) >= 0 || address.Key.StartsWith("26.");
        }

        /// <summary>Runs a command and returns its output, or null when it exits with an error.</summary>
        static string Run(string exe, string arguments)
        {
            try
            {
                var info = new ProcessStartInfo(exe, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                using (Process p = Process.Start(info))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return p.ExitCode == 0 ? output : null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
