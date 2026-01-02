using System;
using Exiled.API.Features;
using HarmonyLib;

namespace SCP_035
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SCP-035";
        public override string Prefix => Name;
        public override string Author => "Morkamo";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 12, 1);

        public static Plugin Instance { get; private set; }
        
        public override void OnEnabled()
        {
            Instance = this;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            base.OnDisabled();
        }
    }
}