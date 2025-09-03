using Livekit.Server.Sdk.Dotnet;

namespace Services.Helpers.LiveKit
{
    public static class LiveKitHelper
    {
        private static readonly string ApiKey = "enachatlivekitkeyprod";
        private static readonly string ApiSecret = "izi-pizi-lemon-squeezy-oguz-celal-ankaraa-prod";

        public static string GenerateToken(string roomName, string userId, string name)
        {
            var grant = new VideoGrants
            {
                RoomJoin = true,
                Room = roomName
            };

            var at = new AccessToken(ApiKey, ApiSecret)
                .WithIdentity(userId.ToString())
                .WithName(name)
                .WithGrants(grant);

            return at.ToJwt();
        }
    }
}
