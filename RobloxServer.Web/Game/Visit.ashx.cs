using System;
using System.Collections.Generic;
using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Game
{
    /// <summary>
    /// Play Solo / Visit script (/Game/Visit.ashx?IsPlaySolo=1&amp;UserID=&amp;PlaceID=). Signed with --rbxsig.
    /// </summary>
    public class Visit : HandlerBase
    {
        static readonly Random GuestRandom = new Random();

        protected override void Handle()
        {
            User user = CurrentUser;
            if (user != null && user.IsCurrentlyBanned)
            {
                user = null;
            }

            long placeId = QueryLong("PlaceID");
            Place place = placeId > 0 ? Db.FindPlace(placeId) : null;
            bool loadPlace = place != null && CanSee(user, place);

            if (loadPlace)
            {
                Db.Places.Update(p => p.Id == place.Id, p => p.Visits++);
            }

            string guestName;
            lock (GuestRandom)
            {
                guestName = "Guest " + GuestRandom.Next(1000, 9999);
            }

            var values = new Dictionary<string, string>
            {
                { "BaseUrl", Config.BaseUrl },
                { "ApiUrl", Config.ApiUrl },
                { "PlaceId", loadPlace ? place.Id.ToString() : "0" },
                { "CreatorId", loadPlace ? place.CreatorId.ToString() : "0" },
                { "LoadPlace", loadPlace ? "true" : "false" },
                { "PlaceUrl", loadPlace ? Config.BaseUrl + "asset/?id=" + place.Id : "" },
                { "UserId", user != null ? user.Id.ToString() : "0" },
                { "UserName", Templates.LuaLongString(user != null ? user.Name : guestName) },
                { "SuperSafeChat", user == null || user.SuperSafeChat ? "true" : "false" },
                { "AccountAge", user != null ? user.AccountAgeDays.ToString() : "0" }
            };

            WriteSignedScript(Templates.Render("Visit.lua", values));
        }
    }
}
