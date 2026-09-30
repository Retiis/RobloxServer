using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace RobloxServer.Data
{
    /// <summary>
    /// Usado apenas quando a PRÓPRIA pessoa, ao se cadastrar, confirma que o username escolhido é o
    /// username real dela no Roblox. Consulta só a API pública (username -&gt; id -&gt; foto do avatar);
    /// nunca busca senha, e-mail ou qualquer dado não público, e nunca é chamado em lote para outros
    /// usuários -- só para a conta que está sendo criada nesse exato cadastro, com esse consentimento.
    /// </summary>
    public static class RobloxLookup
    {
        /// <summary>Devolve a URL da foto de avatar publica, ou null se o username nao existir / a API falhar.</summary>
        public static string TryGetAvatarUrl(string username)
        {
            try
            {
                long? id = GetUserId(username);
                if (id == null)
                {
                    return null;
                }
                return GetAvatarUrl(id.Value);
            }
            catch (Exception ex)
            {
                Logging.Log(LogType.Error, "RobloxLookup falhou para '" + username + "': " + ex.Message);
                return null;
            }
        }

        static long? GetUserId(string username)
        {
            string body = "{\"usernames\":[\"" + username.Replace("\"", "") + "\"]}";
            var request = (HttpWebRequest)WebRequest.Create("https://users.roblox.com/v1/usernames/users");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Timeout = 5000;
            byte[] data = Encoding.UTF8.GetBytes(body);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                var match = Regex.Match(json, "\"id\"\\s*:\\s*(\\d+)");
                return match.Success ? (long?)long.Parse(match.Groups[1].Value) : null;
            }
        }

        static string GetAvatarUrl(long userId)
        {
            string url = "https://thumbnails.roblox.com/v1/users/avatar?userIds=" + userId
                + "&size=420x420&format=Png&isCircular=false";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 5000;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                var match = Regex.Match(json, "\"imageUrl\"\\s*:\\s*\"([^\"]+)\"");
                return match.Success ? match.Groups[1].Value.Replace("\\/", "/") : null;
            }
        }
    }
}
