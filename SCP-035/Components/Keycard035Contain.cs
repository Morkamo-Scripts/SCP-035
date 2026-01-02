using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using UnityEngine;

namespace SCP_035.Components;

public class Keycard035Contain : CustomKeycard
{
    public sealed class SerializableColor
    {
        public byte R { get; set; } = 255;
        public byte G { get; set; } = 255;
        public byte B { get; set; } = 255;
        public byte A { get; set; } = 255;

        public Color32 ToColor() => new(R, G, B, A);
    }
    
    public override uint Id { get; set; } = 10;
    public override string Name { get; set; } = "Ключ доступа к камере содержания SCP-035";
    public override string Description { get; set; } = "Ключ доступа к камере содержания SCP-035";
    public override float Weight { get; set; } = 1;
    public override SpawnProperties SpawnProperties { get; set; } = null;
    public override ItemType Type { get; set; } = ItemType.KeycardCustomManagement;
    public override string KeycardLabel { get; set; } = "SCP-035 KEYCARD";
    
    public SerializableColor KeycardLabelColorRaw { get; set; } = new() { R = 215, G = 215, B = 215, A = 255 };
    public SerializableColor KeycardPermissionsColorRaw { get; set; } = new() { R = 51, G = 51, B = 51, A = 255 };
    public SerializableColor TintColorRaw { get; set; } = new() { R = 51, G = 51, B = 51, A = 255 };

    public override Color32? KeycardLabelColor => KeycardLabelColorRaw?.ToColor();
    public override Color32? KeycardPermissionsColor => KeycardPermissionsColorRaw?.ToColor();
    public override Color32? TintColor => TintColorRaw?.ToColor();

    public override string KeycardName { get; set; } = "Dr. Major";
    public override string SerialNumber { get; set; } = "035053191076999";

    public override KeycardPermissions Permissions { get; set; } = KeycardPermissions.None;

    protected override void SetupKeycard(Keycard keycard)
    {
        base.SetupKeycard(keycard);
        keycard.Permissions = Permissions;
    }
}