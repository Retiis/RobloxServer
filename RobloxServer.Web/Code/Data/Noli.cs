using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RobloxServer.Data
{
    /// <summary>
    /// The Noli myth, first documented on the forums by YaleUniversity on 26 February 2010, and the ARG
    /// the Void Cult built around it. An account named Noli behaves like the myth wikis describe:
    ///   * completely black skin and no clothes (Asset/BodyColors.ashx, Asset/CharacterFetch.ashx);
    ///   * a question mark instead of an avatar and no "last online" data (Browse.aspx, Asset/Avatar.ashx);
    ///   * opening the profile sends you back to the home page (User.aspx);
    ///   * in game, a black aura and everything it stands on turns black (App_Data/Templates/GameServer.lua).
    /// The ARG: the broken profile leaves a base64 message that leads to /Void, which asks a question in
    /// ROT13; the answer reveals a binary message that tells players what to do inside a game.
    /// </summary>
    public static class Noli
    {
        public const string Name = "Noli";

        /// <summary>BrickColor "Really black".</summary>
        public const string ReallyBlack = "1003";

        /// <summary>The first clue, left where the profile should be.</summary>
        public static readonly string Clue = Base64("YOU HAVE ANGERED NOLI! FIFTY SLEEP IN THE VOID. /VOID");

        /// <summary>The question /Void asks, in ROT13.</summary>
        public static readonly string Riddle = Rot13("WHEN DID YALEUNIVERSITY FIRST SEE ME?");

        /// <summary>Shown in binary once the riddle is answered: say "noli" in the chat of a running game.</summary>
        public static readonly string Whisper = Binary("SAY MY NAME WHERE THE SERVERS RUN");

        /// <summary>The day YaleUniversity posted about the broken profile.</summary>
        static readonly DateTime FirstSeen = new DateTime(2010, 2, 26);

        static readonly string[] DateFormats =
        {
            "d/M/yyyy", "M/d/yyyy", "d-M-yyyy", "d.M.yyyy", "yyyy-M-d", "yyyy/M/d", "ddMMyyyy", "MMddyyyy", "yyyyMMdd",
            "d MMMM yyyy", "MMMM d yyyy", "MMMM d, yyyy", "d MMM yyyy", "MMM d yyyy", "MMM d, yyyy"
        };

        public static bool Is(string name)
        {
            return string.Equals((name ?? "").Trim(), Name, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Is(User user)
        {
            return user != null && Is(user.Name);
        }

        /// <summary>True when <paramref name="answer"/> is 26 February 2010, written in (almost) any way.</summary>
        public static bool IsAnswer(string answer)
        {
            string text = (answer ?? "").Trim().ToLowerInvariant();
            // "26 de fevereiro de 2010", "February 26th, 2010"
            text = text.Replace(" de ", " ").Replace("26th", "26");
            DateTime date;
            return DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date) && date == FirstSeen
                || DateTime.TryParseExact(text, DateFormats, new CultureInfo("pt-BR"), DateTimeStyles.AllowWhiteSpaces, out date) && date == FirstSeen;
        }

        static string Base64(string text)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(text));
        }

        static string Rot13(string text)
        {
            return new string(text.Select(c =>
                c >= 'A' && c <= 'Z' ? (char)('A' + (c - 'A' + 13) % 26) :
                c >= 'a' && c <= 'z' ? (char)('a' + (c - 'a' + 13) % 26) : c).ToArray());
        }

        static string Binary(string text)
        {
            return string.Join(" ", Encoding.ASCII.GetBytes(text).Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }
    }
}
