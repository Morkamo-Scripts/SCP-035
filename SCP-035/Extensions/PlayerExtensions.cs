using CommandSystem;
using Exiled.API.Features;
using SCP_035.Features;

namespace SCP_035.Extensions;

public static class PlayerExtensions
{
    public static Scp035PlayerComponent Scp035Properties(this Player player)
        => player.ReferenceHub.GetComponent<Scp035PlayerComponent>();
}