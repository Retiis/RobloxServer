using System.Linq;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Api
{
    /// <summary>Running game servers. ?placeId= to filter.</summary>
    public class Servers : HandlerBase
    {
        protected override void Handle()
        {
            long placeId = QueryLong("placeId");
            User user = CurrentUser;
            string ip = Ip;

            var list = GameServers.All()
                .Where(s => placeId == 0 || s.PlaceId == placeId)
                .Where(s => s.PlaceId == 0 || CanSee(user, Db.FindPlace(s.PlaceId)))
                .Select(s => new
                {
                    jobId = s.IsLegacy ? null : s.JobId,
                    placeId = s.PlaceId,
                    name = s.Name,
                    address = GameServers.AddressFor(s, ip, Request.ServerVariables["LOCAL_ADDR"]),
                    port = s.Port,
                    client = PlaceService.CleanClient(s.Client),
                    version = s.Version,
                    host = s.HostUserName,
                    players = s.PlayerCount,
                    maxPlayers = s.MaxPlayers,
                    started = ToIso(s.Started),
                    legacy = s.IsLegacy
                })
                .ToList();

            WriteJson(new { data = list });
        }
    }
}
