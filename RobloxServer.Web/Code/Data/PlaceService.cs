using System;
using System.Linq;
using System.Text;
using RobloxServer.Security;

namespace RobloxServer.Data
{
    public static class PlaceService
    {
        /// <summary>Accepts binary (&lt;roblox!) and XML (&lt;roblox ...) place files.</summary>
        public static string Validate(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return "The place file is empty.";
            }
            if (data.LongLength > Config.MaxPlaceSizeMegabytes * 1024L * 1024L)
            {
                return "The place file is larger than " + Config.MaxPlaceSizeMegabytes + " MB.";
            }

            string head = Encoding.UTF8.GetString(data, 0, Math.Min(data.Length, 1024));
            if (!head.Contains("<roblox"))
            {
                return "This is not a ROBLOX place file (.rbxl).";
            }
            return null;
        }

        public static string CleanName(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0)
            {
                name = "Place";
            }
            if (name.Length > 50)
            {
                name = name.Substring(0, 50);
            }
            return WordFilter.Filter(name);
        }

        public static string CleanDescription(string description)
        {
            description = (description ?? "").Trim();
            if (description.Length > 1000)
            {
                description = description.Substring(0, 1000);
            }
            return WordFilter.Filter(description);
        }

        public static string CleanClient(string client)
        {
            string match = Config.Clients.FirstOrDefault(c => string.Equals(c, (client ?? "").Trim(), StringComparison.OrdinalIgnoreCase));
            return match ?? Config.DefaultClient;
        }

        /// <summary>
        /// Client a place is played with. Places published for clients that are no longer supported
        /// (2006S...2011M from the Novetus days) are played with the default client.
        /// </summary>
        public static string ClientFor(Place place)
        {
            return CleanClient(place != null ? place.Client : null);
        }

        public static Place Create(User creator, string name, string description, string client, bool isPublic, bool filteringEnabled, int maxPlayers, byte[] data, string genre = null)
        {
            DateTime now = DateTime.UtcNow;
            Place place = Db.Places.InsertWithId(p => p.Id, Config.FirstLocalAssetId, id => new Place
            {
                Id = id,
                Name = CleanName(name),
                Description = CleanDescription(description),
                CreatorId = creator.Id,
                CreatorName = creator.Name,
                Client = CleanClient(client),
                Genre = Genres.Clean(genre),
                Created = now,
                Updated = now,
                IsPublic = isPublic,
                FilteringEnabled = filteringEnabled,
                MaxPlayers = Math.Max(1, Math.Min(maxPlayers <= 0 ? 12 : maxPlayers, 100)),
                FileExtension = "rbxl"
            });

            Db.SavePlaceFile(place.Id, data, "rbxl");
            Logging.Log(LogType.Success, creator.Name + " published place " + place.Id + " (" + place.Name + ")");
            return Db.FindPlace(place.Id);
        }

        /// <summary>Who may start a game server for a place (the HostPolicy setting).</summary>
        public static bool CanHost(User user, Place place)
        {
            if (user == null || place == null || user.IsCurrentlyBanned)
            {
                return false;
            }
            switch ((Config.HostPolicy ?? "").ToLowerInvariant())
            {
                case "admin":
                    return Db.IsAdmin(user);
                case "owner":
                    return place.CreatorId == user.Id || Db.IsAdmin(user);
                default:
                    return place.IsPublic || place.CreatorId == user.Id || Db.IsAdmin(user);
            }
        }

        public static bool CanEdit(User user, Place place)
        {
            return user != null && place != null && (place.CreatorId == user.Id || Db.IsAdmin(user));
        }
    }
}
