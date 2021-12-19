// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class ProductionEndpointConfiguration : EndpointConfiguration
    {
        public ProductionEndpointConfiguration()
        {
            WebsiteRootUrl = APIEndpointUrl = @"https://osu.hikaru.pw";
            APIClientSecret = @"FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk";
            APIClientID = "5";
            SpectatorEndpointUrl = "https://spectator.hikaru.pw/spectator";
            MultiplayerEndpointUrl = "https://spectator.hikaru.pw/multiplayer";
        }
    }
}
