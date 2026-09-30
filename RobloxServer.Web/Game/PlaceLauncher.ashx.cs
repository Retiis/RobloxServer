using System.Linq;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>
    /// 2015 PlaceLauncher.ashx?request=RequestGame&amp;placeId= (or request=RequestGameJob&amp;gameId=).
    /// Status codes: 0 waiting, 2 joining, 4 error, 6 game full, 12 unauthorized.
    /// </summary>
    public class PlaceLauncher : HandlerBase
    {
        protected override void Handle()
        {
            User user = CurrentUser;
            if (user == null || user.IsCurrentlyBanned)
            {
                Reply(null, 12, null, user == null ? "You must be logged in to play." : "Your account has been moderated.", null);
                return;
            }

            GameServer server = null;
            long placeId = QueryLong("placeId");
            string jobId = Request.QueryString["gameId"] ?? Request.QueryString["jobId"];

            if (!string.IsNullOrEmpty(jobId))
            {
                server = GameServers.Find(jobId);
                if (server == null)
                {
                    Reply(null, 5, null, "This game has ended.", null);
                    return;
                }
                placeId = server.PlaceId;
            }

            Place place = Db.FindPlace(placeId);
            if (place == null || !CanSee(user, place))
            {
                Reply(null, 4, null, "This place does not exist or is private.", null);
                return;
            }

            if (server == null)
            {
                var servers = GameServers.ForPlace(placeId);
                server = servers.Where(s => s.MaxPlayers <= 0 || s.PlayerCount < s.MaxPlayers)
                                .OrderByDescending(s => s.PlayerCount)
                                .FirstOrDefault();
                if (server == null)
                {
                    Reply(null, servers.Count > 0 ? 6 : 0, null,
                        servers.Count > 0 ? "The game is full." : "No servers are running this game right now. Click Host Server to start one.", place);
                    return;
                }
            }
            else if (server.MaxPlayers > 0 && server.PlayerCount >= server.MaxPlayers)
            {
                Reply(server, 6, null, "The game is full.", place);
                return;
            }

            AuthTicket ticket = AuthTickets.Issue(user.Id, user.Name, place.Id, server.JobId);
            Reply(server, 2, ticket, null, place);
        }

        void Reply(GameServer server, int status, AuthTicket ticket, string message, Place place)
        {
            WriteJson(new
            {
                jobId = server != null ? server.JobId : null,
                status = status,
                placeId = place != null ? place.Id : 0,
                client = place != null ? PlaceService.ClientFor(place) : null,
                joinScriptUrl = server != null ? Config.BaseUrl + "Game/Join.ashx?jobId=" + server.JobId : null,
                authenticationUrl = Config.BaseUrl + "Login/Negotiate.ashx",
                authenticationTicket = ticket != null ? ticket.Value : null,
                message = message
            });
        }
    }
}
