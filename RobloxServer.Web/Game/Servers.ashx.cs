using System;
using System.Security.Cryptography;
using RobloxServer.Data;
using RobloxServer.Security;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>
    /// Game server ("job") registry used by the launcher when hosting a published game.
    ///   POST action=register  (cookie + X-CSRF-TOKEN) placeId, port, client, name, maxPlayers, address, version
    ///   action=heartbeat|unregister|playerleft  jobId + serverKey (GET works, for old Lua HttpGet)
    /// </summary>
    public class Servers : HandlerBase
    {
        protected override void Handle()
        {
            string action = (Param("action") ?? "").ToLowerInvariant();
            switch (action)
            {
                case "register":
                    Register();
                    break;
                case "heartbeat":
                case "unregister":
                case "playerleft":
                    Update(action);
                    break;
                default:
                    WriteStatus(400, "Unknown action");
                    break;
            }
        }

        void Register()
        {
            if (Request.HttpMethod != "POST")
            {
                WriteStatus(405, "POST required");
                return;
            }

            User user = RequireUser();
            if (user == null || !Auth.ValidateCsrf(Context))
            {
                return;
            }

            long placeId;
            long.TryParse(Param("placeId"), out placeId);
            Place place = Db.FindPlace(placeId);
            if (place == null)
            {
                WriteJson(new { success = false, message = "Place not found." });
                return;
            }
            if (!CanHost(user, place))
            {
                WriteJson(new { success = false, message = "You are not allowed to host this place." });
                return;
            }

            int port;
            if (!int.TryParse(Param("port"), out port) || port <= 0 || port > 65535)
            {
                WriteJson(new { success = false, message = "Invalid port." });
                return;
            }

            int maxPlayers;
            if (!int.TryParse(Param("maxPlayers"), out maxPlayers) || maxPlayers <= 0)
            {
                maxPlayers = place.MaxPlayers > 0 ? place.MaxPlayers : 12;
            }

            string address = Param("address");
            if (string.IsNullOrWhiteSpace(address) || address.Length > 255)
            {
                address = Ip;
            }

            byte[] key = new byte[24];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            var server = new GameServer
            {
                JobId = Guid.NewGuid().ToString(),
                PlaceId = place.Id,
                Name = WordFilter.Filter(string.IsNullOrWhiteSpace(Param("name")) ? place.Name : Param("name").Trim()),
                // "::1" when hosting on the same PC as the site: the 2012M/2013M clients need IPv4.
                Address = ClientIp.ToGameAddress(address.Trim()),
                SourceIp = Ip,
                Port = port,
                Client = PlaceService.ClientFor(place),
                Version = Param("version") ?? "",
                HostUserId = user.Id,
                HostUserName = user.Name,
                ServerKey = BitConverter.ToString(key).Replace("-", ""),
                MaxPlayers = Math.Min(maxPlayers, 100),
                Started = DateTime.UtcNow,
                LastHeartbeat = DateTime.UtcNow
            };
            GameServers.Add(server);
            Logging.Log(LogType.Backend, user.Name + " started job " + server.JobId + " for place " + place.Id + " at " + server.Address + ":" + server.Port);

            WriteJson(new
            {
                success = true,
                jobId = server.JobId,
                serverKey = server.ServerKey,
                placeId = place.Id,
                requireAuthTickets = Config.RequireAuthTickets,
                filteringEnabled = place.FilteringEnabled,
                baseUrl = Config.BaseUrl,
                heartbeatSeconds = 60
            });
        }

        void Update(string action)
        {
            string jobId = Param("jobId");
            GameServer server = GameServers.Find(jobId);
            if (server == null || !PasswordHasher.ConstantTimeEquals(server.ServerKey, Param("serverKey") ?? ""))
            {
                WriteStatus(404, "ERROR|Unknown game server");
                return;
            }

            if (action == "unregister")
            {
                GameServers.Remove(server.JobId);
                Logging.Log(LogType.Backend, "Job " + server.JobId + " closed");
                WriteText("OK");
                return;
            }

            int players;
            bool hasPlayers = int.TryParse(Param("players"), out players);
            GameServers.Touch(server.JobId, s =>
            {
                if (action == "playerleft")
                {
                    s.PlayerCount = Math.Max(0, s.PlayerCount - 1);
                }
                else if (hasPlayers && players >= 0)
                {
                    s.PlayerCount = players;
                }
            });
            WriteText("OK");
        }
    }
}
