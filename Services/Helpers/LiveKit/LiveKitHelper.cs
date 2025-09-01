using Livekit.Server.Sdk.Dotnet;

namespace Services.Helpers.LiveKit
{
    public static class LiveKitHelper
    {
        private static readonly string ApiKey = "enachatlivekitkey";      // LiveKit .env dosyandan
        private static readonly string ApiSecret = "izi-pizi-lemon-squeezy-oguz-celal-ankaraa"; // LiveKit .env dosyandan

        public static string GenerateToken(string roomName, string userId, string name)
        {
            // Token builder
            var grant = new VideoGrants
            {
                RoomJoin = true,
                Room = roomName
            };

            var at = new AccessToken(ApiKey, ApiSecret)
                .WithIdentity(userId.ToString())
                .WithName(name)
                .WithGrants(grant);

            // Token string
            return at.ToJwt();
        }
    }
}
