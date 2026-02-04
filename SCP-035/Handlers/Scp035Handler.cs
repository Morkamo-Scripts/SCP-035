using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Toys;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp0492;
using Exiled.Events.EventArgs.Scp939;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp173Events;
using LabApi.Events.Arguments.ServerEvents;
using MapGeneration.Distributors;
using MEC;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Serializable;
using RueI.API;
using RueI.API.Elements;
using SCP_035.Components;
using SCP_035.Extensions;
using SCP_035.Features;
using UnityEngine;
using events = Exiled.Events.Handlers;
using Object = UnityEngine.Object;
using Pickup = Exiled.API.Features.Pickups.Pickup;
using Player = Exiled.API.Features.Player;
using levents = LabApi.Events.Handlers;
using Random = UnityEngine.Random;

namespace SCP_035.Handlers
{
    public class Scp035Handler : Scp035Component
    {
        public override uint Id { get; set; } = 9;
        public override string Name { get; set; } = "SCP-035";
        public override string Description { get; set; } = "<size=35><b><color=#D90202>Ты надел маску SCP-035, теперь ты единое целое с ним.\n" +
                                                           "Твоё тело стремительно разлогается.\n" +
                                                           "Медикаменты лишь отсрочат неизбежное...\n" +
                                                           "<color=#d97102>Ты получаешь на 35% больше урона!\n" +
                                                           "<color=#d400d4>Все стандартные SCP твои союзники! (Кроме Длани Змея)\n" +
                                                           "<color=#006dd4>'Зов Доктора' у SCP-049 отсрочит твою смерть!</color></b></size>";
        public override float Weight { get; set; } = 1;
        
        protected override void SubscribeEvents()
        {
            events.Player.UsingItemCompleted += OnScp1344Equipping;
            events.Player.ChangingItem += OnChangingItem;
            events.Player.ChangedItem += OnChangedItem;
            events.Player.DroppingItem += OnDroppingItem;
            events.Player.Escaping += OnEscaping;
            events.Player.Died += OnDied;
            events.Player.Hurting += OnHurting;
            events.Player.ItemAdded += OnItemAdded;
            events.Player.PickingUpItem += OnPickingUp;
            events.Player.DroppedItem += OnDroppedItem;
            events.Scp049.Attacking += On049Attack;
            events.Scp106.Attacking += On106Attack;
            events.Player.ReceivingEffect += OnReceivingEffect;
            events.Scp0492.TriggeringBloodlust += On0492TriggerBloodlust;
            events.Scp939.ValidatingVisibility += On939ValidatingVisibility;
            levents.Scp096Events.AddingTarget += On096AddTarget;
            levents.Scp173Events.AddingObserver += On173AddObserver;
            levents.Scp049Events.UsingSense += On049UsingSense;
            levents.ServerEvents.PickupCreated += OnPickupCreated;
            levents.PlayerEvents.ChangedRole += OnChangedRole;
        }
        
        protected override void UnsubscribeEvents()
        {
            events.Player.UsingItemCompleted -= OnScp1344Equipping;
            events.Player.ChangingItem -= OnChangingItem;
            events.Player.ChangedItem -= OnChangedItem;
            events.Player.DroppingItem -= OnDroppingItem;
            events.Player.Escaping -= OnEscaping;
            events.Player.Died -= OnDied;
            events.Player.Hurting -= OnHurting;
            events.Player.ItemAdded -= OnItemAdded;
            events.Player.PickingUpItem -= OnPickingUp;
            events.Player.DroppedItem -= OnDroppedItem;
            events.Scp049.Attacking -= On049Attack;
            events.Scp106.Attacking -= On106Attack;
            events.Player.ReceivingEffect -= OnReceivingEffect;
            events.Scp0492.TriggeringBloodlust -= On0492TriggerBloodlust;
            events.Scp939.ValidatingVisibility -= On939ValidatingVisibility;
            levents.Scp096Events.AddingTarget -= On096AddTarget;
            levents.Scp173Events.AddingObserver -= On173AddObserver;
            levents.Scp049Events.UsingSense -= On049UsingSense;
            levents.ServerEvents.PickupCreated -= OnPickupCreated;
            levents.PlayerEvents.ChangedRole -= OnChangedRole;
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
                    
                    ev.Player.Scp035Properties().PlayerProps.Scp035ProcessorCoroutine = 
                        CoroutineRunner.Run(Scp035LifeProcessor(ev.Player));
                    
                    ev.Player.Scp035Properties().PlayerProps.ScpLabelHintProcessorCoroutine = 
                        CoroutineRunner.Run(ScpLabelHintProcessor(ev.Player));
                    
                    ev.Player.Scp035Properties().PlayerProps.HumeShieldProcessorCoroutine = 
                        CoroutineRunner.Run(HumeShieldProcessor(ev.Player));
                });
            }
        }
        
        private void OnPickupCreated(PickupCreatedEventArgs ev) => HighlightItem(Pickup.Get(ev.Pickup.GameObject));
        private void OnDroppedItem(DroppedItemEventArgs ev) => HighlightItem(ev.Pickup);
        
        private void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsNPC)
                return;
            
            if (Check(ev.Player.CurrentItem) && ev.Player.Scp035Properties().PlayerProps.IsEquipping035Item)
                ev.IsAllowed = false;
        }

        private void OnChangedItem(ChangedItemEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsNPC)
                return;
            
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
            if (ev.Player == null || ev.Player.IsNPC)
                return;
            
            if (Check(ev.Item) && ev.Player.Scp035Properties().PlayerProps.IsEquipping035Item)
                ev.IsAllowed = false;
        }

        private void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            try
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (ev.Player == null || ev.Player.IsNpc)
                    return;
                
                var player = Player.Get(ev.Player);
            
                var properties = player.Scp035Properties()?.PlayerProps;
            
                if (properties == null || !properties.IsPlayScp035)
                    return;

                RemoveRole(ev.Player);
            }
            catch  { /*ignored*/ }
        }

        private void OnReceivingEffect(ReceivingEffectEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsNPC)
                return;
            
            if (ev.Player.Scp035Properties().PlayerProps.IsPlayScp035)
            {
                if (ev.Effect.GetEffectType() == EffectType.CardiacArrest && ev.Intensity != 0)
                    ev.IsAllowed = false;
            }
        }
        
        private void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsNPC || !Check(ev.Player))
                return;
            
            var properties = ev.Player.Scp035Properties().PlayerProps;

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
            if (ev.Player == null || ev.Player.IsNPC)
                return;
            
            var properties = ev.Player.Scp035Properties().PlayerProps;
            
            if (properties.IsPlayScp035)
                RemoveRole(ev.Player);
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Player == null || ev.Player.IsNPC)
                return;
            
            if (ev.Attacker?.IsScp == true && ev.Player?.Scp035Properties()?.PlayerProps.IsPlayScp035 == true)
            {
                ev.IsAllowed = false;
                return;
            }
        
            if (ev.Attacker?.Scp035Properties()?.PlayerProps.IsPlayScp035 == true && ev.Player?.IsScp == true)
            {
                ev.IsAllowed = false;
                return;
            }

            if (ev.Player?.Scp035Properties()?.PlayerProps.IsPlayScp035 == true && 
                ev.DamageHandler.CustomBase.Base.DeathScreenText != "Коррозия SCP-035 разрушила тело носителя.")
            {
                ev.Amount *= 1.35f;
                return;
            }
        }

        private new void OnPickingUp(PickingUpItemEventArgs ev)
        {
            if (ev.Player.IsNPC)
                return;
            
            if (ev.Player.Scp035Properties().PlayerProps.IsPlayScp035 &&
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
            if (ev.Player == null || ev.Player.IsNPC || ev.Item == null || ev.Pickup == null)
                return;
            
            if (ev.Player.Scp035Properties().PlayerProps.IsPlayScp035 &&
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
        
        private void On096AddTarget(Scp096AddingTargetEventArgs ev)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (ev.Target == null || ev.Target.IsNpc)
                return;
            
            if (Player.Get(ev.Target).Scp035Properties().PlayerProps.IsPlayScp035)
                ev.IsAllowed = false;
        }
        
        private void On173AddObserver(Scp173AddingObserverEventArgs ev)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (ev.Target == null || ev.Target.IsNpc)
                return;
            
            if (Player.Get(ev.Target).Scp035Properties().PlayerProps.IsPlayScp035)
                ev.IsAllowed = false;
        }
    
        private void On049UsingSense(Scp049UsingSenseEventArgs ev)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (ev.Target == null || ev.Target.IsNpc)
                return;
            
            if (Player.Get(ev.Target).Scp035Properties().PlayerProps.IsPlayScp035)
                ev.IsAllowed = false;
        }
        
        private void On049Attack(AttackingEventArgs ev)
        {
            if (ev.Target == null || ev.Target.IsNPC)
                return;
            
            if (ev.Player.Scp035Properties().PlayerProps.IsPlayScp035)
                ev.IsAllowed = false;
        }
    
        private void On106Attack(Exiled.Events.EventArgs.Scp106.AttackingEventArgs ev)
        {
            if (ev.Target == null || ev.Target.IsNPC)
                return;
            
            if (ev.Player.Scp035Properties().PlayerProps.IsPlayScp035)
                ev.IsAllowed = false;
        }
    
        private void On0492TriggerBloodlust(TriggeringBloodlustEventArgs ev)
        {
            if (ev.Target == null || ev.Target.IsNPC)
                return;
            
            if (ev.Target.Scp035Properties().PlayerProps.IsPlayScp035)
                ev.IsAllowed = false;
        }
    
        private void On939ValidatingVisibility(ValidatingVisibilityEventArgs ev)
        {
            if (ev.Target == null || ev.Target.IsNPC)
                return;
            
            if (Player.Get(ev.Target).Scp035Properties().PlayerProps.IsPlayScp035)
            {
                ev.IsLateSeen = true;
                ev.IsAllowed = true;
            }
        }

        private IEnumerator Scp035LifeProcessor(Player player)
        {
            player.Role.Set(RoleTypeId.Tutorial, RoleSpawnFlags.None);
            player.MaxHealth = Plugin.Instance.Config.Scp035Role.Health;
            player.Health = Plugin.Instance.Config.Scp035Role.Health;
            player.MaxHumeShield = Plugin.Instance.Config.Scp035Role.MaxHumeShield;
            player.HumeShield = Plugin.Instance.Config.Scp035Role.MaxHumeShield;
            player.DisableAllEffects();

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
            ProceduralParticles(player, properties.HighlightPrefab, Color.red, 0, 0.025f,
                new(3f, 3f, 3f), 0.1f, 8, 8, 60, 1f);
            
            RueDisplay.Get(player).Show(
                new Tag(),
                new BasicElement(300, Description), 20);

            Timing.CallDelayed(20.1f, () => RueDisplay.Get(player).Update());
            
            foreach (var spec in player.CurrentSpectatingPlayers)
            {
                RueDisplay.Get(spec).Show(
                    new Tag(),
                    new BasicElement(300, Description), 20);
                    
                Timing.CallDelayed(20.1f, () => RueDisplay.Get(spec).Update());
            }
            
            while (player.IsConnected && player.IsAlive && properties.IsPlayScp035)
            {
                yield return new WaitForSeconds(1f);
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

        private IEnumerator HumeShieldProcessor(Player player)
        {
            var properties = player.Scp035Properties().PlayerProps;
            
            while (player.IsConnected && player.IsAlive && properties.IsPlayScp035)
            {
                var scp049 = Player.List.FirstOrDefault(pl => pl.Role == RoleTypeId.Scp049);
                
                if (scp049 == null || 
                    !scp049.Role.As<Scp049Role>().IsCallActive ||
                    Vector3.Distance(player.Position, scp049.Position) > 5)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
                
                var maxHs = Plugin.Instance.Config.Scp035Role.MaxHumeShield;
                
                if (player.HumeShield < maxHs)
                    player.HumeShield += 1f;
                
                yield return new WaitForSeconds(0.1f);
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
            
            LabApi.Features.Wrappers.Cassie.Message(
                "SCP 0 3 5 Successfully terminated by .G5 Time lost", true, true, true,
                "Scp-035 [<color=red>Keter</color>] успешно уничтожен. Носитель погиб."
            );
        }
        
        public static void ProceduralParticles(
            Player player,
            GameObject gameObject,
            Color particleColor,
            float duration = 0f,
            float spawnRate = 0.01f,
            Vector3 fieldLocalScale = default,
            float particleSize = 0.1f,
            ushort intensity = 80,
            float appearSpeed = 3f,
            float idleRotateSpeed = 30f,
            float disappearSpeed = 3f)
        
        {
            if (fieldLocalScale == default)
                fieldLocalScale = Vector3.one * 3f;
            
            switch (Room.Get(gameObject.transform.position).Zone)
            {
                case(ZoneType.LightContainment): intensity = (ushort)(intensity * 0.55f);
                    break;
                    
                case (ZoneType.HeavyContainment): intensity = (ushort)(intensity * 0.45f);
                    break;

                case (ZoneType.Surface): intensity = (ushort)(intensity * 1.5f);
                    break;
                
                case (ZoneType.Pocket): intensity = (ushort)(intensity * 0.3f);
                    break;
            }
            
            Timing.RunCoroutine(SpawnParticleField(
                player, gameObject, particleColor, duration, spawnRate, fieldLocalScale, particleSize,
                intensity, appearSpeed, idleRotateSpeed, disappearSpeed));
        }
        
        private static IEnumerator<float> SpawnParticleField(
            Player player,
            GameObject playerObject,
            Color particleColor,
            float duration,
            float spawnRate,
            Vector3 localScale,
            float particleSize,
            ushort intensity,
            float appearSpeed,
            float idleRotateSpeed,
            float disappearSpeed)
        
        {
            GameObject anchor = new GameObject("ParticleAnchor");
            anchor.transform.SetParent(playerObject.transform);
            anchor.transform.localPosition = Vector3.zero;
            anchor.transform.localScale = localScale;

            bool ended = false;
            if (duration != 0)
                Timing.CallDelayed(duration, () => ended = true);

            while (!ended && playerObject != null)
            {
                if (player.IsEffectActive<Invisible>())
                {
                    yield return Timing.WaitForSeconds(1f);
                    continue;
                }
                
                yield return Timing.WaitForSeconds(spawnRate);

                // Локальные координаты (в пределах anchor.localScale / 2)
                Vector3 localOffset = new Vector3(
                    Random.Range(-localScale.x / 2f, localScale.x / 2f),
                    Random.Range(-localScale.y / 2f, localScale.y / 2f),
                    Random.Range(-localScale.z / 2f, localScale.z / 2f)
                );

                Vector3 spawnPos = anchor.transform.position + anchor.transform.rotation * localOffset;

                Primitive particle = Primitive.Create(PrimitiveType.Cube);
                particle.Base.syncInterval = 0;
                
                if (Room.Get(anchor.transform.position).Type == RoomType.Surface)
                {
                    particle.Color = particleColor with { a = 0.8f };
                }
                else
                {
                    particle.Color = (particleColor * intensity) with { a = 0.5f };
                }
                
                particle.Position = spawnPos;
                particle.Scale = Vector3.zero;
                particle.Visible = true;
                particle.IsStatic = false;
                particle.Collidable = false;

                Quaternion baseRotation = Random.rotation;
                particle.Rotation = baseRotation;
                particle.Spawn();

                float totalLife = (1f / appearSpeed) + (1f / disappearSpeed); // оценка общего времени

                Timing.RunCoroutine(ParticleLifeCycleHandler(
                    particle, particleSize, appearSpeed, idleRotateSpeed, disappearSpeed, baseRotation, totalLife));
            }

            Object.Destroy(anchor);
        }
        
        private static IEnumerator<float> ParticleLifeCycleHandler(
            Primitive particle,
            float maxScale,
            float appearSpeed,
            float rotationSpeed,
            float disappearSpeed,
            Quaternion baseRotation,
            float estimatedLifetime)
        
{
float appearTime = 1f / appearSpeed;
float disappearTime = 1f / disappearSpeed;
float idleTime = estimatedLifetime - appearTime - disappearTime;

float time = 0f;

// Плавное появление
while (time < appearTime && particle != null)
{
    float t = time / appearTime;
    float scale = Mathf.Lerp(0f, maxScale, t);
    particle.Scale = Vector3.one * scale;
    particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * time, 0f);

    time += Time.deltaTime;
    yield return 0f;
}

time = 0f;

// Idle состояние (вращение, scale остаётся)
while (time < idleTime && particle != null)
{
    particle.Scale = Vector3.one * maxScale;
    particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * (appearTime + time), 0f);

    time += Time.deltaTime;
    yield return 0f;
}

time = 0f;

// Плавное исчезновение
while (time < disappearTime && particle != null)
{
    float t = time / disappearTime;
    float scale = Mathf.Lerp(maxScale, 0f, t);
    particle.Scale = Vector3.one * scale;
    particle.Rotation = baseRotation * Quaternion.Euler(0f, rotationSpeed * (appearTime + idleTime + time), 0f);

    time += Time.deltaTime;
    yield return 0f;
}

particle?.Destroy();
}
    }
}