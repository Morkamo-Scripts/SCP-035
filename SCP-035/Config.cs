using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using SCP_035.Components;
using SCP_035.Handlers;
using UnityEngine;

namespace SCP_035
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        
        public Scp035Handler Scp035Item { get; set; } = new();
        public Scp035Role Scp035Role { get; set; } = new();
        public Keycard035Contain Scp035KeycardContain { get; set; } = new(); 
    }

    public class Scp035Role
    {
        public float Health { get; set; } = 2000;

        public HashSet<ItemType> NotAllowedItems { get; set; } =
        [
            ItemType.MicroHID,
            ItemType.Coin,
            ItemType.SCP1344,
            ItemType.SCP330,
            ItemType.AntiSCP207,
            ItemType.Medkit,
            ItemType.Adrenaline,
            ItemType.Painkillers,
            ItemType.SCP500
        ];
    }
}