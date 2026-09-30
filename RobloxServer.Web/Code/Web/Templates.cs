using System.Collections.Generic;
using System.IO;

namespace RobloxServer.Web
{
    /// <summary>Loads files from App_Data/Templates and replaces {Tokens}.</summary>
    public static class Templates
    {
        public static string Render(string name, IDictionary<string, string> values)
        {
            string text = File.ReadAllText(Path.Combine(Config.DataPath, "Templates", name)).Replace("\r\n", "\n");
            foreach (var pair in values)
            {
                text = text.Replace("{" + pair.Key + "}", pair.Value ?? "");
            }
            return text;
        }

        /// <summary>Makes a value safe to put inside a Lua [====[ long string ]====].</summary>
        public static string LuaLongString(string value)
        {
            return (value ?? "").Replace("]====]", "");
        }
    }
}
