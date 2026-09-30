using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Keys
{
    /// <summary>The script signing public key, to patch into the clients. ?format=xml for the RSA XML form.</summary>
    public class PublicKey : HandlerBase
    {
        protected override void Handle()
        {
            if (string.Equals(Request.QueryString["format"], "xml", System.StringComparison.OrdinalIgnoreCase))
            {
                Response.ContentType = "text/xml";
                Response.Write(ScriptSigner.PublicKeyXml);
                return;
            }
            WriteText(ScriptSigner.PublicKeyBlobBase64);
        }
    }
}
