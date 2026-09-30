using System.Linq;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Api
{
    /// <summary>Published games for the launcher's game browser. ?id= returns one game.</summary>
    public class Games : HandlerBase
    {
        protected override void Handle()
        {
            User user = CurrentUser;
            var servers = GameServers.All();
            long onlyId = QueryLong("id");

            var games = Db.Places.All()
                .Where(p => CanSee(user, p) && (onlyId == 0 || p.Id == onlyId))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description ?? "",
                    creatorId = p.CreatorId,
                    creatorName = p.CreatorName,
                    client = PlaceService.ClientFor(p),
                    visits = p.Visits,
                    version = p.Version,
                    created = ToIso(p.Created),
                    updated = ToIso(p.Updated),
                    size = p.Size,
                    md5 = p.Md5 ?? "",
                    maxPlayers = p.MaxPlayers,
                    isPublic = p.IsPublic,
                    filteringEnabled = p.FilteringEnabled,
                    servers = servers.Count(s => s.PlaceId == p.Id),
                    playing = servers.Where(s => s.PlaceId == p.Id).Sum(s => s.PlayerCount),
                    canHost = CanHost(user, p),
                    placeUrl = Config.BaseUrl + "asset/?id=" + p.Id
                })
                .OrderByDescending(g => g.playing)
                .ThenByDescending(g => g.visits)
                .ToList();

            WriteJson(new { data = games });
        }
    }
}
