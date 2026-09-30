using System;
using System.Xml.Serialization;

namespace RobloxServer.Data
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastOnline { get; set; }
        public string LastIp { get; set; }
        public bool IsAdmin { get; set; }

        /// <summary>2015 "Under 13" accounts were forced into SuperSafeChat.</summary>
        public bool SuperSafeChat { get; set; }

        public bool IsBanned { get; set; }
        public string BanReason { get; set; }

        /// <summary>Null for a permanent ban ("Account Deleted").</summary>
        public DateTime? BanExpires { get; set; }

        public string BodyColors { get; set; }

        /// <summary>"Male", "Female" or null (2013 landing page signup).</summary>
        public string Gender { get; set; }

        /// <summary>Latest status update ("What are you up to?" on the home page), shown on the profile.</summary>
        public string Status { get; set; }

        /// <summary>Comma separated ids of the places this user joined last, newest first (Recently Played).</summary>
        public string RecentPlaces { get; set; }

        [XmlIgnore]
        public bool IsCurrentlyBanned
        {
            get { return IsBanned && (!BanExpires.HasValue || BanExpires.Value > DateTime.UtcNow); }
        }

        [XmlIgnore]
        public int AccountAgeDays
        {
            get { return Math.Max(0, (int)(DateTime.UtcNow - Created).TotalDays); }
        }
    }

    public class Place
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long CreatorId { get; set; }
        public string CreatorName { get; set; }

        /// <summary>Novetus client the place is played with (2012M, 2009E...).</summary>
        public string Client { get; set; }

        /// <summary>2013 Games page genre (see Genres). Null for places published before genres existed.</summary>
        public string Genre { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int Version { get; set; }
        public long Visits { get; set; }
        public bool IsPublic { get; set; }
        public bool FilteringEnabled { get; set; }
        public int MaxPlayers { get; set; }
        public long Size { get; set; }
        public string Md5 { get; set; }

        /// <summary>rbxl (binary or XML) or rbxlx.</summary>
        public string FileExtension { get; set; }
    }

    /// <summary>A status update ("What are you up to?") shown in My Feed on the 2013 home page.</summary>
    public class FeedPost
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
    }

    public class IpBan
    {
        public string Ip { get; set; }
        public string Reason { get; set; }
        public DateTime Created { get; set; }
    }

    /// <summary>A running game server ("job"). Kept in memory only, like RCC jobs.</summary>
    public class GameServer
    {
        public string JobId { get; set; }
        public long PlaceId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string SourceIp { get; set; }
        public int Port { get; set; }
        public string Client { get; set; }
        public string Version { get; set; }
        public long HostUserId { get; set; }
        public string HostUserName { get; set; }
        public string ServerKey { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }
        public DateTime Started { get; set; }
        public DateTime LastHeartbeat { get; set; }

        /// <summary>True for servers registered through the legacy Novetus list.php (no heartbeat).</summary>
        public bool IsLegacy { get; set; }
    }
}
