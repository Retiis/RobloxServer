using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Caching;

namespace RobloxServer.Security
{
    public class AuthTicket
    {
        public string Value { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long PlaceId { get; set; }
        public string JobId { get; set; }
        public DateTime Expires { get; set; }
        public bool NegotiateUsed { get; set; }
        public bool ServerUsed { get; set; }
    }

    /// <summary>
    /// One-time, short-lived authentication tickets (Game/GetAuthTicket, PlaceLauncher.ashx).
    /// A ticket can be redeemed once by the client (Login/Negotiate.ashx) and once by the game
    /// server (Game/ValidateTicket.ashx). Stored in the ASP.NET cache, so an app restart voids them.
    /// </summary>
    public static class AuthTickets
    {
        static readonly object Sync = new object();

        static string CacheKey(string ticket)
        {
            return "authticket:" + ticket;
        }

        public static AuthTicket Issue(long userId, string userName, long placeId, string jobId)
        {
            byte[] bytes = new byte[48];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            var ticket = new AuthTicket
            {
                Value = BitConverter.ToString(bytes).Replace("-", ""),
                UserId = userId,
                UserName = userName,
                PlaceId = placeId,
                JobId = jobId ?? "",
                Expires = DateTime.UtcNow.AddMinutes(Config.AuthTicketMinutes)
            };

            HttpRuntime.Cache.Insert(CacheKey(ticket.Value), ticket, null, ticket.Expires, Cache.NoSlidingExpiration);
            return ticket;
        }

        static AuthTicket Lookup(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 256)
            {
                return null;
            }
            var ticket = HttpRuntime.Cache[CacheKey(value.Trim())] as AuthTicket;
            if (ticket == null || ticket.Expires < DateTime.UtcNow)
            {
                return null;
            }
            return ticket;
        }

        /// <summary>Client-side redemption (Login/Negotiate.ashx?suggest=).</summary>
        public static AuthTicket RedeemForNegotiate(string value)
        {
            lock (Sync)
            {
                AuthTicket ticket = Lookup(value);
                if (ticket == null || ticket.NegotiateUsed)
                {
                    return null;
                }
                ticket.NegotiateUsed = true;
                return ticket;
            }
        }

        /// <summary>
        /// Server-side redemption. The ticket must have been issued for this job, or for this place
        /// when it was issued before the player picked a server.
        /// </summary>
        public static AuthTicket RedeemForServer(string value, string jobId, long placeId)
        {
            lock (Sync)
            {
                AuthTicket ticket = Lookup(value);
                if (ticket == null || ticket.ServerUsed)
                {
                    return null;
                }

                bool jobMatches = !string.IsNullOrEmpty(ticket.JobId) && string.Equals(ticket.JobId, jobId, StringComparison.OrdinalIgnoreCase);
                bool placeMatches = string.IsNullOrEmpty(ticket.JobId) && ticket.PlaceId == placeId;
                if (!jobMatches && !placeMatches)
                {
                    return null;
                }

                ticket.ServerUsed = true;
                return ticket;
            }
        }
    }
}
