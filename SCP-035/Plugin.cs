using System;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using LabApi.Events;
using SCP_035.Features;

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
            Exiled.Events.Handlers.Player.Verified += OnVerifiedPlayer;
            Config.Scp035Item.Register();
            Config.Scp035KeycardContain.Register();
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            Config.Scp035KeycardContain.Unregister();
            Config.Scp035Item.Unregister();
            Exiled.Events.Handlers.Player.Verified -= OnVerifiedPlayer;
            Instance = null;
            base.OnDisabled();
        }
        
        private void OnVerifiedPlayer(VerifiedEventArgs ev)
        {
            if (ev.Player.ReferenceHub.gameObject.GetComponent<Scp035PlayerComponent>() != null)
                return;

            ev.Player.ReferenceHub.gameObject.AddComponent<Scp035PlayerComponent>();
        }
    }
}