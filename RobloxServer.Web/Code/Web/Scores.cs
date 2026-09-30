using System.Collections.Generic;

namespace RobloxServer.Web
{
    /// <summary>Wipeouts / Knockouts counters (the old in-memory "score" object in index.js).</summary>
    public static class Scores
    {
        public class Score
        {
            public int Wipeouts;
            public int Knockouts;
        }

        static readonly Dictionary<string, Score> Table = new Dictionary<string, Score>();

        public static void Add(string userId, int wipeouts, int knockouts)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            lock (Table)
            {
                Score score;
                if (!Table.TryGetValue(userId, out score))
                {
                    score = new Score();
                    Table[userId] = score;
                }
                score.Wipeouts += wipeouts;
                score.Knockouts += knockouts;
                Logging.Log(LogType.Backend, "Score for " + userId + ": " + score.Wipeouts + " wipeouts, " + score.Knockouts + " knockouts");
            }
        }
    }
}
