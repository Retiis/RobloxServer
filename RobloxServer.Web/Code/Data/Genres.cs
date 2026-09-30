using System;
using System.Collections.Generic;
using System.Linq;

namespace RobloxServer.Data
{
    /// <summary>The game genres of the 2013 Games page, with the icon ROBLOX showed next to each game.</summary>
    public static class Genres
    {
        public class Genre
        {
            public string Name { get; set; }
            public string Icon { get; set; }

            /// <summary>Value used in Games.aspx?genre= (no spaces).</summary>
            public string Key
            {
                get { return Name.Replace(" ", "").Replace("-", ""); }
            }
        }

        public const string Default = "All";

        public static readonly IList<Genre> All = new List<Genre>
        {
            new Genre { Name = "All", Icon = "Classic.png" },
            new Genre { Name = "Building", Icon = "Classic.png" },
            new Genre { Name = "Horror", Icon = "Cthulu.png" },
            new Genre { Name = "Town and City", Icon = "City.png" },
            new Genre { Name = "Military", Icon = "ModernMilitary.png" },
            new Genre { Name = "Comedy", Icon = "LOL.png" },
            new Genre { Name = "Medieval", Icon = "Castle.png" },
            new Genre { Name = "Adventure", Icon = "Adventure.png" },
            new Genre { Name = "Sci-Fi", Icon = "SciFi.png" },
            new Genre { Name = "Naval", Icon = "Pirate.png" },
            new Genre { Name = "FPS", Icon = "FPS.png" },
            new Genre { Name = "RPG", Icon = "RPG.png" },
            new Genre { Name = "Sports", Icon = "Sports.png" },
            new Genre { Name = "Fighting", Icon = "Ninja.png" },
            new Genre { Name = "Western", Icon = "WildWest.png" }
        }.AsReadOnly();

        /// <summary>Finds a genre by name or key; unknown and empty values are "All".</summary>
        public static Genre Find(string value)
        {
            value = (value ?? "").Trim();
            return All.FirstOrDefault(g => string.Equals(g.Name, value, StringComparison.OrdinalIgnoreCase)
                                           || string.Equals(g.Key, value, StringComparison.OrdinalIgnoreCase))
                   ?? All[0];
        }

        public static string Clean(string value)
        {
            return Find(value).Name;
        }
    }
}
