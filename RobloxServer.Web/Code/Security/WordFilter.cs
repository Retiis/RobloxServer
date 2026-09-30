using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RobloxServer.Security
{
    /// <summary>
    /// 2015-style blacklist filter: replaces blacklisted words with hashes (####). Words are read
    /// from App_Data/FilteredWords.txt (one per line). Easy to bypass, exactly like the original.
    /// </summary>
    public static class WordFilter
    {
        static string[] words;
        static DateTime loadedAt;

        static string[] Words
        {
            get
            {
                string path = Path.Combine(Config.DataPath, "FilteredWords.txt");
                DateTime modified = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
                if (words == null || modified != loadedAt)
                {
                    words = File.Exists(path)
                        ? File.ReadAllLines(path).Select(w => w.Trim()).Where(w => w.Length > 0 && !w.StartsWith("#")).ToArray()
                        : new string[0];
                    loadedAt = modified;
                }
                return words;
            }
        }

        public static string Filter(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            foreach (string word in Words)
            {
                text = Regex.Replace(text, Regex.Escape(word), m => new string('#', m.Length), RegexOptions.IgnoreCase);
            }
            return text;
        }

        public static bool IsClean(string text)
        {
            return string.Equals(Filter(text), text, StringComparison.Ordinal);
        }

        /// <summary>2015 username rules: 3-20 characters, letters, digits and at most one underscore, not at the ends.</summary>
        public static string ValidateUserName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 20)
            {
                return "Usernames must be between 3 and 20 characters.";
            }
            if (!Regex.IsMatch(name, "^[A-Za-z0-9_]+$"))
            {
                return "Only letters, numbers and _ are allowed.";
            }
            if (name.StartsWith("_") || name.EndsWith("_") || name.Count(c => c == '_') > 1)
            {
                return "Usernames can have at most one _ and it cannot be at the start or end.";
            }
            if (!IsClean(name))
            {
                return "Username not appropriate for ROBLOX.";
            }
            return null;
        }
    }
}
