using System.Collections;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.Events.EventArgs.Player;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using MEC;
using PlayerRoles;
using RueI.API;
using RueI.API.Elements;
using SCP_035.Components;
using SCP_035.Extensions;
using SCP_035.Features;
using UnityEngine;
using events = Exiled.Events.Handlers;
using Pickup = Exiled.API.Features.Pickups.Pickup;
using Player = Exiled.API.Features.Player;

namespace SCP_035.Handlers
{
    public class Scp035Handler : Scp035Component
    {
        public override uint Id { get; set; } = 9;
        public override string Name { get; set; } = "SCP-035";
        public override string Description { get; set; } = "<size=35><b><color=#D90202>Ты надел маску SCP-035, теперь ты единое целое с ним.\n" +
                                                           "Твоё тело стремительно разлогается.\n" +
                                                           "Медикаменты лишь отсрочат неизбежное...</color></b></size>";
        public override float Weight { get; set; } = 1;
        
        protected override void SubscribeEvents()
        {
            events.Player.UsingItemCompleted += OnScp1344Equipping;
            events.Player.ChangingItem += OnChangingItem;
            events.Player.ChangedItem += OnChangedItem;
            events.Player.DroppingItem += OnDroppingItem;
            events.Player.Escaping += OnEscaping;
            events.Player.ReceivingEffect += OnReceivingEffect;
            events.Player.Died += OnDied;
            events.Player.Hurting += OnHurting;
            events.Player.ItemAdded += OnItemAdded;
            events.Player.PickingUpItem += OnPickingUp;
            events.Player.DroppedItem += OnDroppedItem;
            LabApi.Events.Handlers.ServerEvents.PickupCreated += OnPickupCreated;
            LabApi.Events.Handlers.PlayerEvents.ChangedRole += OnChangedRole;
        }
        
        protected override void UnsubscribeEvents()
        {
            events.Player.UsingItemCompleted -= OnScp1344Equipping;
            events.Player.ChangingItem -= OnChangingItem;
            events.Player.ChangedItem -= OnChangedItem;
            events.Player.DroppingItem -= OnDroppingItem;
            events.Player.Escaping -= OnEscaping;
            events.Player.ReceivingEffect -= OnReceivingEffect;
            events.Player.Died -= OnDied;
            events.Player.Hurting -= OnHurting;
            events.Player.ItemAdded -= OnItemAdded;
            events.Player.PickingUpItem -= OnPickingUp;
            events.Player.DroppedItem -= OnDroppedItem;
            LabApi.Events.Handlers.ServerEvents.PickupCreated -= OnPickupCreated;
            LabApi.Events.Handlers.PlayerEvents.ChangedRole -= OnChangedRole;
        }
        
        private void OnScp1344Equipping(UsingItemCompletedEventArgs ev)
        {
            if (!ev.Player.IsNPC && Check(ev.Usable))
            {
                ev.Player.Scp035Properties().PlayerProps.IsEquipping035Item = true;

                Timing.CallDelayed(1f, () =>
                {
                    ev.Player.CurrentItem.Destroy();
                    
                    ev.Player.Scp035Properties().PlayerProps.IsEquipping035Item = false;
                    ev.Player.EnableEffect(EffectType.Flashed, 1, 1f);
                    ev.Player.EnableEffect(EffectType.CardiacArrest, 255);
                    
                    ev.Player.Scp035Properties().PlayerProps.Scp035ProcessorCoroutine = 
                        CoroutineRunner.Run(Scp035LifeProcessor(ev.Player));
                    
                    ev.Player.Scp035Properties().PlayerProps.ScpLabelHintProcessorCoroutine = 
                        CoroutineRunner.Run(ScpLabelHintProcessor(ev.Player));
                });
            }
        }
        
        private void OnReceivingEffect(ReceivingEffectEventArgs ev)
        {
            if (!ev.Player.IsNPC && ev.Player.Scp035Properties().PlayerProps.IsPlayScp035 && ev.Effect.TryGetEffectType(out var effectType))
                if (effectType == EffectType.CardiacArrest || effectType == EffectType.Exhausted)
                    ev.IsAllowed = false;
        }
        
        private void OnPickupCreated(PickupCreatedEventArgs ev) => HighlightItem(Pickup.Get(ev.Pickup.GameObject));
        private void OnDroppedItem(DroppedItemEventArgs ev) => HighlightItem(ev.Pickup);
        
        private void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (!ev.Player.IsNPC && Check(ev.Player.CurrentItem) && ev.Player.Scp035Properties().PlayerProps.IsEquipping035Item)
                ev.IsAllowed = false;
        }

        private void OnChangedItem(ChangedItemEventArgs ev)
        {
            if (Check(ev.Item))
            {
                RueDisplay.Get(ev.Player).Show(
                    new Tag(),
                    new BasicElement(200, "<size=35><b><color=#D90202>Вы держите объект SCP-035!\n" +
                                          "Если надеть, обратного пути не будет...</color></b></size>"), 4);

                Timing.CallDelayed(4.1f, () => RueDisplay.Get(ev.Player).Update());
            
                foreach (var player in ev.Player.CurrentSpectatingPlayers)
                {
                    RueDisplay.Get(player).Show(
                        new Tag(),
                        new BasicElement(200, "<size=35><b><color=#D90202>Игрок держит объект SCP-035!\n" +
                                              "Если надеть, обратного пути не будет...</color></b></size>"), 4);
                    
                    Timing.CallDelayed(4.1f, () => RueDisplay.Get(player).Update());
                }
            }
        }
        
        private new void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (!ev.Player.IsNPC && Check(ev.Item) && ev.Player.Scp035Properties().PlayerProps.IsEquipping035Item)
                ev.IsAllowed = false;
        }

        private void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            var player = Player.Get(ev.Player);
            var properties = player.Scp035Properties().PlayerProps;
            
            if (player.IsNPC || !properties.IsPlayScp035)
                return;

            RemoveRole(ev.Player);
        }

        private void OnEscaping(EscapingEventArgs ev)
        {
            var properties = ev.Player.Scp035Properties().PlayerProps;
            
            if (ev.Player.IsNPC || !properties.IsPlayScp035)
                return;

            if (properties.IsEscapingHintActive)
            {
                ev.IsAllowed = false;
                return;
            }
            
            properties.IsEscapingHintActive = true;
            
            RueDisplay.Get(ev.Player).Show(
                new Tag(),
                new BasicElement(800, "<size=50><b><color=#C70000>Побег за SCP-035 невозможен!</color></b></size>"), 5);

            Timing.CallDelayed(5.1f, () =>
            {
                properties.IsEscapingHintActive = false;
                RueDisplay.Get(ev.Player).Update();
            });
            
            foreach (var player in ev.Player.CurrentSpectatingPlayers)
            {
                RueDisplay.Get(player).Show(
                    new Tag(),
                    new BasicElement(800, "<size=50><b><color=#C70000>Побег за SCP-035 невозможен!</color></b></size>"), 5);
                    
                Timing.CallDelayed(5.1f, () => RueDisplay.Get(player).Update());
            }
            
            ev.IsAllowed = false;
        }
        
        private void OnDied(DiedEventArgs ev)
        {
            var properties = ev.Player.Scp035Properties().PlayerProps;
            
            if (!ev.Player.IsNPC && properties.IsPlayScp035)
            {
                RemoveRole(ev.Player);
                
                LabApi.Features.Wrappers.Cassie.Message(
                    "SCP 0 3 5 Successfully terminated by .G5 Time lost", true, true, true,
                    "Scp-035 [<color=red>Keter</color>] успешно уничтожен. Носитель погиб!"
                );
            }
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker?.Role == RoleTypeId.Scp049)
            {
                ev.Player.Kill(ev.DamageHandler);
                return;
            }
            
            if (!ev.Player.IsNPC && ev.Player.Scp035Properties().PlayerProps.IsPlayScp035 &&
                ev.DamageHandler.Type == DamageType.CardiacArrest)
            {
                ev.IsAllowed = false;
            }
        }

        private new void OnPickingUp(PickingUpItemEventArgs ev)
        {
            if (!ev.Player.IsNPC && ev.Player.Scp035Properties().PlayerProps.IsPlayScp035 &&
                Plugin.Instance.Config.Scp035Role.NotAllowedItems.Contains(ev.Pickup.Type))
            {
                ev.IsAllowed = false;
                
                RueDisplay.Get(ev.Player).Show(
                    new Tag(),
                    new BasicElement(900, "<size=50><b><color=#C70000>Этот предмет запрещен для подбора у SCP-035!</color></b></size>"), 4);
                
                Timing.CallDelayed(4.1f, () => RueDisplay.Get(ev.Player).Update());

                foreach (var player in ev.Player.CurrentSpectatingPlayers)
                {
                    RueDisplay.Get(player).Show(
                        new Tag(),
                        new BasicElement(900, "<size=50><b><color=#C70000>Этот предмет запрещен для подбора у SCP-035!</color></b></size>"), 4);
                    
                    Timing.CallDelayed(4.1f, () => RueDisplay.Get(player).Update());
                }
            }
        }

        private void OnItemAdded(ItemAddedEventArgs ev)
        {
            if (!ev.Player.IsNPC && ev.Player.Scp035Properties().PlayerProps.IsPlayScp035 &&
                Plugin.Instance.Config.Scp035Role.NotAllowedItems.Contains(ev.Item.Type))
            {
                ev.Player.DropItem(ev.Item);
                
                RueDisplay.Get(ev.Player).Show(
                    new Tag(),
                    new BasicElement(900, "<size=50><b><color=#C70000>Этот предмет запрещен для подбора у SCP-035\n" +
                                          "и был выброшен автоматически!</color></b></size>"), 4);
                
                Timing.CallDelayed(4.1f, () => RueDisplay.Get(ev.Player).Update());

                foreach (var player in ev.Player.CurrentSpectatingPlayers)
                {
                    RueDisplay.Get(player).Show(
                        new Tag(),
                        new BasicElement(900, "<size=50><b><color=#C70000>Этот предмет запрещен для подбора у SCP-035\n" +
                                              "и был выброшен автоматически!</color></b></size>"), 4);
                    
                    Timing.CallDelayed(4.1f, () => RueDisplay.Get(player).Update());
                }
            }
        }

        private IEnumerator Scp035LifeProcessor(Player player)
        {
            player.MaxHealth = Plugin.Instance.Config.Scp035Role.Health;
            player.Health = Plugin.Instance.Config.Scp035Role.Health;
            player.DisableAllEffects();
            player.EnableEffect(EffectType.CardiacArrest);
            player.EnableEffect(EffectType.Exhausted);

            var properties = player.Scp035Properties().PlayerProps;
            properties.IsPlayScp035 = true;
            properties.HighlightPrefab = new GameObject("HighlightPrefab")
            {
                transform =
                {
                    position = player.Transform.position
                }
            };
            properties.HighlightPrefab.transform.SetParent(player.Transform);
            
            HighlightManager.MakeLight(properties.HighlightPrefab.transform.position, Color.red, LightShadows.None);
            HighlightManager.ProceduralParticles(properties.HighlightPrefab, Color.red, 0, 0.015f,
                new(3f, 3f, 3f), 0.1f, 8, 8, 60, 1f);
            
            RueDisplay.Get(player).Show(
                new Tag(),
                new BasicElement(250, Description), 10);

            Timing.CallDelayed(10.1f, () => RueDisplay.Get(player).Update());
            
            foreach (var spec in player.CurrentSpectatingPlayers)
            {
                RueDisplay.Get(spec).Show(
                    new Tag(),
                    new BasicElement(250, Description), 10);
                    
                Timing.CallDelayed(10.1f, () => RueDisplay.Get(spec).Update());
            }
            
            while (player.IsConnected && player.IsAlive && properties.IsPlayScp035)
            {
                yield return new WaitForSeconds(0.2f);
                player.Hurt(1f, "Коррозия SCP-035 разрушила тело носителя.");
            }
        }
        
        private IEnumerator ScpLabelHintProcessor(Player player)
        {
            var properties = player.Scp035Properties().PlayerProps;
            
            while (player.IsConnected && player.IsAlive && properties.IsPlayScp035)
            {
                foreach (var spec in player.CurrentSpectatingPlayers)
                {
                    RueDisplay.Get(spec).Show(
                        new Tag(),
                        new BasicElement(120, "<align=right><size=30><b><color=#D90202>Игрок играет за SCP-035</color></b></size>"), 1);
                    
                    Timing.CallDelayed(1.1f, () => RueDisplay.Get(spec).Update());
                }
                
                yield return new WaitForSeconds(1f);
            }
        }
        
        private void HighlightItem(Pickup pickup)
        {
            if (Check(pickup))
            {
                var anchor = HighlightManager.MakeLight(pickup.Position, Color.red,
                    LightShadows.None, 1, 3);
                
                HighlightManager.ProceduralParticles(pickup.GameObject, Color.red, 0, 0.03f,
                    new(0.7f, 0.7f, 0.7f), 0.1f, 15, 8, 60, 1f);
                
                anchor.Transform.SetParent(pickup.Transform);
                anchor.Spawn();
            }
        }

        private void RemoveRole(Player player)
        {
            var properties = player.Scp035Properties().PlayerProps;
            
            CoroutineRunner.Stop(properties.Scp035ProcessorCoroutine);
            Object.Destroy(properties.HighlightPrefab);
            
            properties.HighlightPrefab = null;
            properties.IsEscapingHintActive = false;
            properties.IsEquipping035Item = false;
            properties.IsPlayScp035 = false;
        }
    }
}