using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;

namespace SCP_035.Components
{
    public abstract class Scp035Component : CustomItem
    {
        public override ItemType Type { get; set; } = ItemType.SCP1344;
        public override SpawnProperties SpawnProperties { get; set; } = null;
    }
}