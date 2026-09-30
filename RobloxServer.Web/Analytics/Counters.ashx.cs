using RobloxServer.Web;

namespace RobloxServer.Handlers.Analytics
{
    /// <summary>/v1.1/Counters/Increment/?apiKey=&amp;counterName=&amp;amount= : accepted and ignored.</summary>
    public class Counters : HandlerBase
    {
        protected override void Handle()
        {
            Response.StatusCode = 204;
        }
    }
}
