using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace RobloxServer.Data
{
    /// <summary>
    /// Sincroniza os arquivos de App_Data (Users.xml, Places.xml, Keys/CsrfSecret.txt, ...) com o
    /// Vercel Blob, para sobreviver ao filesystem efemero da Vercel (cada instancia/cold start tem
    /// seu proprio disco local). Sem efeito nenhum se RS_BLOB_TOKEN nao estiver definido -- nesse
    /// caso o comportamento continua identico ao de antes (so disco local), o que mantem
    /// Windows/IIS e o Raspberry Pi funcionando sem nenhuma mudanca.
    ///
    /// Configuracao (variavel de ambiente):
    ///   RS_BLOB_TOKEN - o token BLOB_READ_WRITE_TOKEN do seu Vercel Blob store
    ///
    /// A API do Vercel Blob nao usa um caminho fixo e previsivel: cada PUT devolve uma URL publica
    /// unica, que precisa ser guardada para o proximo GET (nao da para simplesmente montar a URL a
    /// partir do nome do arquivo). Por isso mantemos um pequeno mapa nome-arquivo -> URL publica,
    /// tambem persistido como blob (RemoteBlobStore.Manifest.txt) para funcionar entre instancias.
    /// </summary>
    public static class RemoteBlobStore
    {
        const string ApiHost = "https://blob.vercel-storage.com";
        static readonly string Token = Environment.GetEnvironmentVariable("RS_BLOB_TOKEN");

        public static bool IsEnabled
        {
            get { return !string.IsNullOrEmpty(Token); }
        }

        /// <summary>
        /// Baixa o blob mais recente para o caminho local, se um blob com esse nome ja existir.
        /// Nunca lanca excecao: se a rede falhar, seguimos com o que ja estiver em disco local.
        /// </summary>
        public static void Pull(string fileName, string localPath)
        {
            if (!IsEnabled)
            {
                return;
            }

            try
            {
                string publicUrl = ManifestLookup(fileName);
                if (publicUrl == null)
                {
                    return;
                }

                var request = (HttpWebRequest)WebRequest.Create(publicUrl);
                request.Timeout = 5000;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var fileStream = File.Create(localPath))
                {
                    responseStream.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                Logging.Log(LogType.Error, "RemoteBlobStore.Pull falhou para " + fileName + ": " + ex.Message);
            }
        }

        /// <summary>
        /// Sobe o arquivo local para o Vercel Blob e atualiza o manifesto com a URL publica
        /// resultante. Loga o erro mas nao lanca: preferimos manter a escrita local valida a
        /// derrubar a requisicao do usuario por uma falha de rede na sincronizacao.
        /// </summary>
        public static void Push(string fileName, string localPath)
        {
            if (!IsEnabled)
            {
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(localPath);
                string publicUrl = Put(fileName, data);
                if (publicUrl != null)
                {
                    ManifestStore(fileName, publicUrl);
                }
            }
            catch (Exception ex)
            {
                Logging.Log(LogType.Error, "RemoteBlobStore.Push falhou para " + fileName + ": " + ex.Message);
            }
        }

        // -- Vercel Blob REST: PUT https://blob.vercel-storage.com/{pathname} --

        static string Put(string pathname, byte[] data)
        {
            var request = (HttpWebRequest)WebRequest.Create(ApiHost + "/" + Uri.EscapeDataString(pathname));
            request.Method = "PUT";
            request.Headers["Authorization"] = "Bearer " + Token;
            request.Headers["x-api-version"] = "7";
            // Sobrescreve sempre o mesmo pathname em vez de gerar um nome com sufixo aleatorio.
            request.Headers["x-add-random-suffix"] = "0";
            request.ContentType = "application/octet-stream";
            request.ContentLength = data.Length;
            request.Timeout = 8000;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                // Extrai "url":"..." sem depender de um parser JSON completo, so pra manter o
                // arquivo sem novas dependencias no projeto.
                var match = Regex.Match(json, "\"url\"\\s*:\\s*\"([^\"]+)\"");
                return match.Success ? match.Groups[1].Value : null;
            }
        }

        // -- Manifesto nome-arquivo -> URL publica, guardado como mais um blob de texto simples --

        static string ManifestLookup(string fileName)
        {
            string manifest = ManifestDownload();
            if (manifest == null)
            {
                return null;
            }
            foreach (string line in manifest.Split('\n'))
            {
                int sep = line.IndexOf('\t');
                if (sep > 0 && line.Substring(0, sep) == fileName)
                {
                    return line.Substring(sep + 1).Trim();
                }
            }
            return null;
        }

        static void ManifestStore(string fileName, string publicUrl)
        {
            string manifest = ManifestDownload() ?? "";
            var lines = new System.Collections.Generic.List<string>();
            bool replaced = false;
            foreach (string line in manifest.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int sep = line.IndexOf('\t');
                if (sep > 0 && line.Substring(0, sep) == fileName)
                {
                    lines.Add(fileName + "\t" + publicUrl);
                    replaced = true;
                }
                else
                {
                    lines.Add(line);
                }
            }
            if (!replaced)
            {
                lines.Add(fileName + "\t" + publicUrl);
            }

            string updated = string.Join("\n", lines);
            Put("RemoteBlobStore.Manifest.txt", System.Text.Encoding.UTF8.GetBytes(updated));
            manifestCache = updated;
        }

        static string manifestCache;

        static string ManifestDownload()
        {
            if (manifestCache != null)
            {
                return manifestCache;
            }

            try
            {
                // O manifesto sempre usa o mesmo pathname (x-add-random-suffix=0), entao a URL
                // publica segue um padrao estavel: baixamos direto do host publico do store.
                var request = (HttpWebRequest)WebRequest.Create(
                    "https://" + StoreHostFromToken() + ".public.blob.vercel-storage.com/RemoteBlobStore.Manifest.txt");
                request.Timeout = 5000;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    manifestCache = reader.ReadToEnd();
                    return manifestCache;
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // primeira escrita ainda nao aconteceu
                }
                Logging.Log(LogType.Error, "RemoteBlobStore: falha ao baixar manifesto: " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Logging.Log(LogType.Error, "RemoteBlobStore: falha ao baixar manifesto: " + ex.Message);
                return null;
            }
        }

        static string StoreHostFromToken()
        {
            // O token do Vercel Blob tem o formato vercel_blob_rw_<storeId>_<random>; o storeId
            // (em minusculas) e o subdominio publico do store.
            var match = Regex.Match(Token ?? "", @"vercel_blob_rw_([A-Za-z0-9]+)_");
            if (!match.Success)
            {
                throw new InvalidOperationException("RS_BLOB_TOKEN nao parece um token valido do Vercel Blob (formato esperado: vercel_blob_rw_...)");
            }
            return match.Groups[1].Value.ToLowerInvariant();
        }
    }
}
