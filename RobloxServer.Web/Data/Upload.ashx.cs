using RobloxServer.Data;
using RobloxServer.Web;

namespace RobloxServer.Handlers.Data
{
    /// <summary>
    /// Studio "Publish to ROBLOX": POST /Data/Upload.ashx?assetid=&amp;type=Place&amp;name=&amp;description=&amp;ispublic=
    /// with the place file as the body (optionally gzip). assetid=0 creates a new place.
    /// Returns the asset id, like 2015.
    /// </summary>
    public class Upload : HandlerBase
    {
        protected override void Handle()
        {
            if (Request.HttpMethod != "POST")
            {
                WriteStatus(405, "POST required");
                return;
            }

            User user = RequireUser();
            if (user == null)
            {
                return;
            }

            byte[] data = ReadBody();
            string error = PlaceService.Validate(data);
            if (error != null)
            {
                WriteStatus(400, error);
                return;
            }

            long assetId = QueryLong("assetid");
            if (assetId > 0)
            {
                Place place = Db.FindPlace(assetId);
                if (place == null || !PlaceService.CanEdit(user, place))
                {
                    WriteStatus(403, "You do not have permission to update this place.");
                    return;
                }

                Db.SavePlaceFile(place.Id, data, "rbxl");
                Logging.Log(LogType.Success, user.Name + " updated place " + place.Id + " from Studio");
                WriteText(place.Id.ToString());
                return;
            }

            string isPublic = Request.QueryString["ispublic"];
            Place created = PlaceService.Create(user,
                Request.QueryString["name"],
                Request.QueryString["description"],
                Request.QueryString["client"],
                isPublic == null || isPublic == "1" || isPublic.ToLowerInvariant() == "true",
                false,
                QueryInt("maxPlayers", 12),
                data);
            WriteText(created.Id.ToString());
        }
    }
}
