using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.Handlers;
using PlayerRoles;
using SCP_035.Extensions;
using SerpentHands.Extensions;
using Player = Exiled.API.Features.Player;

namespace SCP_035.Handlers;

public class RoundHandler
{
    public void OnEndingRound(EndingRoundEventArgs ev)
    {
        try
        {
            var alives = Player.List.Where(pl => pl.IsAlive).ToHashSet();
            if (alives.IsEmpty())
                return;
        
            var target = alives.FirstOrDefault(pl => pl.Scp035Properties().PlayerProps.IsPlayScp035 ||
                                                     pl.SerpentHandsProperties().SerpentProps.SerpentRole != null);
            if (target == null)
            {
                Log.Info("TARGETS NOT FOUND!");
                return;
            }
        
            var aliveEnemies = alives.Where(pl => pl.Role.Team != Team.SCPs && 
                                                  pl != target && 
                                                  pl.Role.Type != target.Role.Type &&
                                                  pl.Role.Team != target.Role.Team).ToHashSet();
        
            if (aliveEnemies.IsEmpty())
            {
                Log.Info("SCP WIN!");
                ev.LeadingTeam = LeadingTeam.Anomalies;
                return;
            }
        
            Log.Info("NOT ALLOWED!");
            ev.IsAllowed = false;
        }
        catch  { /*ignored*/ }
    }
}