using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace RobloxServer.Data
{
    public static class Db
    {
        public static readonly XmlTable<User> Users = new XmlTable<User>("Users.xml");
        public static readonly XmlTable<Place> Places = new XmlTable<Place>("Places.xml");
        public static readonly XmlTable<IpBan> IpBans = new XmlTable<IpBan>("IpBans.xml");
        public static readonly XmlTable<FeedPost> Feed = new XmlTable<FeedPost>("Feed.xml");

        /// <summary>Status updates kept in Feed.xml; older ones are dropped.</summary>
        const int MaxFeedPosts = 500;

        /// <summary>Places kept in User.RecentPlaces.</summary>
        const int MaxRecentPlaces = 6;

        public static string PlacesPath
        {
            get { return Path.Combine(Config.DataPath, "Places"); }
        }

        /// <summary>Admin supplied asset overrides: App_Data/Assets/{id} is served before falling back to Roblox.</summary>
        public static string AssetsPath
        {
            get { return Path.Combine(Config.DataPath, "Assets"); }
        }

        public static User FindUser(long id)
        {
            return Users.Find(u => u.Id == id);
        }

        public static User FindUser(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            return Users.Find(u => string.Equals(u.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Saves a status update: User.Status and a new post in My Feed.</summary>
        public static FeedPost PostStatus(User user, string text)
        {
            DateTime now = DateTime.UtcNow;
            Users.Update(u => u.Id == user.Id, u => u.Status = text);
            FeedPost post = Feed.InsertWithId(p => p.Id, 1, id => new FeedPost { Id = id, UserId = user.Id, UserName = user.Name, Text = text, Created = now });
            List<FeedPost> all = Feed.All();
            if (all.Count > MaxFeedPosts)
            {
                long oldest = all.OrderByDescending(p => p.Id).Skip(MaxFeedPosts - 1).First().Id;
                Feed.Delete(p => p.Id < oldest);
            }
            return post;
        }

        /// <summary>Puts the place first in the user's Recently Played list.</summary>
        public static void AddRecentPlace(long userId, long placeId)
        {
            Users.Update(u => u.Id == userId, u =>
            {
                var ids = RecentPlaceIds(u).Where(id => id != placeId).Take(MaxRecentPlaces - 1).ToList();
                ids.Insert(0, placeId);
                u.RecentPlaces = string.Join(",", ids);
            });
        }

        public static List<long> RecentPlaceIds(User user)
        {
            var ids = new List<long>();
            foreach (string part in (user.RecentPlaces ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                long id;
                if (long.TryParse(part, out id))
                {
                    ids.Add(id);
                }
            }
            return ids;
        }

        public static Place FindPlace(long id)
        {
            return Places.Find(p => p.Id == id);
        }

        public static string PlaceFilePath(Place place)
        {
            return Path.Combine(PlacesPath, place.Id + "." + (place.FileExtension ?? "rbxl"));
        }

        /// <summary>Writes a new version of the place file and updates size/hash/version.</summary>
        public static void SavePlaceFile(long placeId, byte[] data, string extension)
        {
            Directory.CreateDirectory(PlacesPath);
            string md5;
            using (var hasher = MD5.Create())
            {
                md5 = BitConverter.ToString(hasher.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
            }

            Places.Update(p => p.Id == placeId, p =>
            {
                string oldPath = PlaceFilePath(p);
                p.FileExtension = extension;
                string path = PlaceFilePath(p);
                if (!string.Equals(oldPath, path, StringComparison.Ordinal) && File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
                File.WriteAllBytes(path, data);
                p.Size = data.LongLength;
                p.Md5 = md5;
                p.Version++;
                p.Updated = DateTime.UtcNow;
            });
        }

        public static bool IsAdmin(User user)
        {
            if (user == null)
            {
                return false;
            }
            return user.IsAdmin || Config.Administrators.Any(a => string.Equals(a, user.Name, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Running game servers, in memory. Servers registered by the launcher heartbeat every minute;
    /// legacy Novetus list.php servers cannot heartbeat so they are kept for longer.
    /// </summary>
    public static class GameServers
    {
        static readonly object Sync = new object();
        static readonly List<GameServer> Servers = new List<GameServer>();

        public static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromMinutes(3);
        public static readonly TimeSpan LegacyTimeout = TimeSpan.FromHours(12);

        static void Prune()
        {
            DateTime now = DateTime.UtcNow;
            Servers.RemoveAll(s => now - s.LastHeartbeat > (s.IsLegacy ? LegacyTimeout : HeartbeatTimeout));
        }

        public static List<GameServer> All()
        {
            lock (Sync)
            {
                Prune();
                return Servers.ToList();
            }
        }

        public static List<GameServer> ForPlace(long placeId)
        {
            return All().Where(s => s.PlaceId == placeId).ToList();
        }

        public static GameServer Find(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                return null;
            }
            lock (Sync)
            {
                Prune();
                return Servers.FirstOrDefault(s => string.Equals(s.JobId, jobId, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static void Add(GameServer server)
        {
            lock (Sync)
            {
                Servers.RemoveAll(s => string.Equals(s.JobId, server.JobId, StringComparison.OrdinalIgnoreCase));
                Servers.Add(server);
            }
        }

        public static bool Remove(string jobId)
        {
            lock (Sync)
            {
                return Servers.RemoveAll(s => string.Equals(s.JobId, jobId, StringComparison.OrdinalIgnoreCase)) > 0;
            }
        }

        /// <summary>
        /// Address a player should connect to. A server hosted inside the LAN is shown with its LAN
        /// address to LAN players and with PublicGameAddress (the Raspberry Pi's WAN side) to everyone else.
        /// </summary>
        public static string AddressFor(GameServer server, string requesterIp)
        {
            return AddressFor(server, requesterIp, null);
        }

        /// <param name="siteAddress">
        /// The site's own address on the connection the player used (LOCAL_ADDR). A server hosted on the
        /// same PC as the site is registered as 127.0.0.1; players on other PCs get this address instead.
        /// </param>
        public static string AddressFor(GameServer server, string requesterIp, string siteAddress)
        {
            if (Security.ClientIp.IsLoopback(server.Address) && !Security.ClientIp.IsLoopback(requesterIp))
            {
                string local = Security.ClientIp.ToGameAddress(siteAddress);
                IPAddress parsed;
                if (!string.IsNullOrEmpty(local) && IPAddress.TryParse(local, out parsed)
                    && parsed.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(parsed))
                {
                    return Security.ClientIp.IsPrivate(requesterIp) || string.IsNullOrEmpty(Config.PublicGameAddress)
                        ? local
                        : Config.PublicGameAddress;
                }
            }

            if (Security.ClientIp.IsPrivate(server.Address)
                && !Security.ClientIp.IsPrivate(requesterIp)
                && !string.IsNullOrEmpty(Config.PublicGameAddress))
            {
                return Config.PublicGameAddress;
            }
            return server.Address;
        }

        public static bool Touch(string jobId, Action<GameServer> change)
        {
            lock (Sync)
            {
                GameServer server = Servers.FirstOrDefault(s => string.Equals(s.JobId, jobId, StringComparison.OrdinalIgnoreCase));
                if (server == null)
                {
                    return false;
                }
                server.LastHeartbeat = DateTime.UtcNow;
                if (change != null)
                {
                    change(server);
                }
                return true;
            }
        }
    }
}
