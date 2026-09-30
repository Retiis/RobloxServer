using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RobloxServer.Security
{
    /// <summary>
    /// 2015 script signing. Join/Visit scripts start with --rbxsig%BASE64%, where the signature is
    /// RSA PKCS#1 v1.5 over SHA-1 of "\r\n" + script using a 1024-bit key. The client only runs
    /// scripts signed with the key embedded in it, so clients must be patched with our public key
    /// (see /Keys/PublicKey.ashx and the README).
    /// </summary>
    public static class ScriptSigner
    {
        static readonly object Sync = new object();
        static RSACryptoServiceProvider rsa;

        static string KeyDirectory
        {
            get { return Path.Combine(Config.DataPath, "Keys"); }
        }

        static string PrivateKeyPath
        {
            get { return Path.Combine(KeyDirectory, "PrivateKey.xml"); }
        }

        static RSACryptoServiceProvider Key
        {
            get
            {
                lock (Sync)
                {
                    if (rsa == null)
                    {
                        rsa = LoadOrCreate();
                    }
                    return rsa;
                }
            }
        }

        static RSACryptoServiceProvider LoadOrCreate()
        {
            var provider = new RSACryptoServiceProvider(1024);
            provider.PersistKeyInCsp = false;

            if (File.Exists(PrivateKeyPath))
            {
                provider.FromXmlString(File.ReadAllText(PrivateKeyPath));
                return provider;
            }

            Directory.CreateDirectory(KeyDirectory);
            File.WriteAllText(PrivateKeyPath, provider.ToXmlString(true));
            File.WriteAllText(Path.Combine(KeyDirectory, "PublicKey.xml"), provider.ToXmlString(false));
            File.WriteAllText(Path.Combine(KeyDirectory, "PublicKeyBlob.txt"), Convert.ToBase64String(provider.ExportCspBlob(false)));
            Logging.Log(LogType.Security, "Generated a new 1024-bit script signing key in App_Data/Keys. Patch your clients with PublicKeyBlob.txt.");
            return provider;
        }

        public static void EnsureKey()
        {
            var unused = Key;
        }

        /// <summary>Returns the script prefixed with its --rbxsig% signature.</summary>
        public static string Sign(string script)
        {
            string body = "\r\n" + script;
            byte[] signature;
            lock (Sync)
            {
                using (var sha1 = new SHA1CryptoServiceProvider())
                {
                    signature = Key.SignData(Encoding.UTF8.GetBytes(body), sha1);
                }
            }
            return "--rbxsig%" + Convert.ToBase64String(signature) + "%" + body;
        }

        public static bool Verify(string signedScript)
        {
            if (signedScript == null || !signedScript.StartsWith("--rbxsig%"))
            {
                return false;
            }

            int end = signedScript.IndexOf('%', 9);
            if (end < 0)
            {
                return false;
            }

            byte[] signature;
            try
            {
                signature = Convert.FromBase64String(signedScript.Substring(9, end - 9));
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] body = Encoding.UTF8.GetBytes(signedScript.Substring(end + 1));
            lock (Sync)
            {
                using (var sha1 = new SHA1CryptoServiceProvider())
                {
                    return Key.VerifyData(body, sha1, signature);
                }
            }
        }

        /// <summary>CryptoAPI PUBLICKEYBLOB, the format embedded in RobloxPlayerBeta.exe / RobloxStudioBeta.exe.</summary>
        public static string PublicKeyBlobBase64
        {
            get
            {
                lock (Sync)
                {
                    return Convert.ToBase64String(Key.ExportCspBlob(false));
                }
            }
        }

        public static string PublicKeyXml
        {
            get
            {
                lock (Sync)
                {
                    return Key.ToXmlString(false);
                }
            }
        }
    }
}
