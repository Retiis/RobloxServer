using System;
using System.Linq;
using RobloxServer.Data;

namespace RobloxServer.Security
{
    public static class Bans
    {
        static IpBan[] cache;
        static DateTime cacheTime;

        public static IpBan FindIpBan(string ip)
        {
            if (cache == null || DateTime.UtcNow - cacheTime > TimeSpan.FromSeconds(30))
            {
                cache = Db.IpBans.All().ToArray();
                cacheTime = DateTime.UtcNow;
            }
            return cache.FirstOrDefault(b => string.Equals(b.Ip, ip, StringComparison.OrdinalIgnoreCase));
        }

        public static void BanIp(string ip, string reason)
        {
            Db.IpBans.Delete(b => b.Ip == ip);
            Db.IpBans.Insert(new IpBan { Ip = ip, Reason = reason, Created = DateTime.UtcNow });
            cache = null;
        }

        public static void UnbanIp(string ip)
        {
            Db.IpBans.Delete(b => b.Ip == ip);
            cache = null;
        }

        public static void BanUser(long userId, string reason, int? days)
        {
            Db.Users.Update(u => u.Id == userId, u =>
            {
                u.IsBanned = true;
                u.BanReason = reason;
                u.BanExpires = days.HasValue ? DateTime.UtcNow.AddDays(days.Value) : (DateTime?)null;
            });
        }

        public static void UnbanUser(long userId)
        {
            Db.Users.Update(u => u.Id == userId, u =>
            {
                u.IsBanned = false;
                u.BanReason = null;
                u.BanExpires = null;
            });
        }
    }
}
