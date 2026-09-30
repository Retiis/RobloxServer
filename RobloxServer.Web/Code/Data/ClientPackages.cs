using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

namespace RobloxServer.Data
{
    /// <summary>
    /// Files RobloxPlayerLauncher installs from this site, like setup.roblox.com in 2013:
    ///   App_Data/Clients/2012M.zip, App_Data/Clients/2013M.zip  (one zip per client)
    ///   App_Data/Launcher/RobloxPlayerLauncher.exe            (optional, for self updates)
    /// The version of a package is the start of its SHA-256, so replacing a zip makes every
    /// launcher download the new one.
    /// </summary>
    public static class ClientPackages
    {
        public const string Launcher = "Launcher";
        public const string LauncherFileName = "RobloxPlayerLauncher.exe";

        public class Package
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string FileName { get; set; }
            public long Size { get; set; }
            public string Sha256 { get; set; }
            public DateTime Updated { get; set; }

            public string Version
            {
                get { return "version-" + Sha256.Substring(0, 16); }
            }
        }

        static readonly object Sync = new object();
        static readonly Dictionary<string, Package> Cache = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Launcher or one of Config.Clients, with the canonical spelling; null for anything else.</summary>
        public static string CleanName(string name)
        {
            name = (name ?? "").Trim();
            if (string.Equals(name, Launcher, StringComparison.OrdinalIgnoreCase))
            {
                return Launcher;
            }
            return Config.Clients.FirstOrDefault(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
        }

        static string PathFor(string name)
        {
            return name == Launcher
                ? System.IO.Path.Combine(Config.DataPath, "Launcher", LauncherFileName)
                : System.IO.Path.Combine(Config.DataPath, "Clients", name + ".zip");
        }

        /// <summary>
        /// Checks an uploaded file and makes it the package <paramref name="name"/> (replacing the old one).
        /// Returns an error message, or null on success. <paramref name="uploadedPath"/> is moved or deleted.
        /// </summary>
        public static string Install(string name, string uploadedPath)
        {
            name = CleanName(name);
            try
            {
                if (name == null)
                {
                    return "Unknown package.";
                }

                string error = name == Launcher ? CheckLauncher(uploadedPath) : CheckClientZip(uploadedPath);
                if (error != null)
                {
                    return error;
                }

                string path = PathFor(name);
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.Move(uploadedPath, path);
                return null;
            }
            finally
            {
                if (File.Exists(uploadedPath))
                {
                    File.Delete(uploadedPath);
                }
            }
        }

        static string CheckLauncher(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                // Every Windows exe starts with "MZ".
                return stream.Length > 2 && stream.ReadByte() == 'M' && stream.ReadByte() == 'Z'
                    ? null
                    : "That is not a Windows program. Upload RobloxPlayerLauncher.exe.";
            }
        }

        static string CheckClientZip(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var names = zip.Entries.Select(e => e.FullName.Replace('\\', '/')).ToList();
                    if (names.Any(n => n.StartsWith("/") || n.Split('/').Contains("..")))
                    {
                        return "The zip has unsafe paths (\"..\" or absolute). The launcher would refuse it.";
                    }
                    // The launcher looks for the exes at the top of the zip (or in client\ and server\).
                    bool exeAtTop = names.Any(n => n.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                        && (n.IndexOf('/') < 0 || n.StartsWith("client/", StringComparison.OrdinalIgnoreCase)
                            || n.StartsWith("server/", StringComparison.OrdinalIgnoreCase)));
                    if (exeAtTop)
                    {
                        return null;
                    }

                    var folders = names.Select(n => n.Split('/')[0]).Distinct().ToList();
                    if (folders.Count == 1 && names.All(n => n.Contains("/")))
                    {
                        return "Everything in the zip is inside the folder \"" + folders[0] + "\". Open that folder, select all the files "
                            + "in it and zip those instead (RobloxApp_client.exe must be at the top of the zip).";
                    }
                    return "The zip has no .exe at the top. Zip the contents of the client folder (RobloxApp_client.exe at the top).";
                }
            }
            catch (InvalidDataException)
            {
                return "That is not a .zip file.";
            }
        }

        /// <summary>The package, or null when it has not been uploaded to App_Data.</summary>
        public static Package Find(string name)
        {
            name = CleanName(name);
            if (name == null)
            {
                return null;
            }

            string path = PathFor(name);
            var info = new FileInfo(path);
            if (!info.Exists)
            {
                return null;
            }

            lock (Sync)
            {
                Package cached;
                if (Cache.TryGetValue(name, out cached) && cached.Size == info.Length && cached.Updated == info.LastWriteTimeUtc)
                {
                    return cached;
                }

                string hash;
                using (var sha = SHA256.Create())
                using (var stream = File.OpenRead(path))
                {
                    hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                }

                var package = new Package
                {
                    Name = name,
                    Path = path,
                    FileName = System.IO.Path.GetFileName(path),
                    Size = info.Length,
                    Sha256 = hash,
                    Updated = info.LastWriteTimeUtc
                };
                Cache[name] = package;
                return package;
            }
        }
    }
}
