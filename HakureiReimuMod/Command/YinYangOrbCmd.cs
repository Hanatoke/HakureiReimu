using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Node;
using MegaCrit.Sts2.Core.AutoSlay.Handlers.Screens;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Command
{
    public static class YinYangOrbCmd
    {
        public static async Task Spawn(PlayerChoiceContext choiceContext,Player player,int amount)
        {
            if (CombatManager.Instance.IsOverOrEnding)return;
            for (var i = 0; i < amount; i++)
            {
                await Spawn(choiceContext,player);
            }
        }
        public static async Task Spawn(PlayerChoiceContext choiceContext,Player player)
        {
            if (CombatManager.Instance.IsOverOrEnding)return;
            YinYangOrb orb=ModelDb.Orb<YinYangOrb>().ToMutable() as YinYangOrb;
            if (orb == null)return;
            orb.Owner = player;
            YinYangOrbManager manager = player.PlayerCombatState.YinYangOrbManager();
            if (manager != null)
            {
                if (manager.Orbs.Count>=manager.Capacity)
                {
                    await Evoke(choiceContext,player);
                }
                CombatManager.Instance.History.OrbChanneled(player.Creature.CombatState, orb);
                manager.Add(orb);
                NYinYangOrbManager nm = NCombatRoom.Instance?.GetCreatureNode(player.Creature)
                    .YinYangOrbManager(manager);
                if (nm != null)
                {
                    nm.AddOrb([orb]);
                }
            }
        }

        public static async Task Evoke(PlayerChoiceContext choiceContext,Player player,Creature target=null)
        {
            if (CombatManager.Instance.IsOverOrEnding)
                return;
            YinYangOrbManager manager = player.PlayerCombatState.YinYangOrbManager();
            if (manager != null&&manager.Orbs.Count>0)
            {
                YinYangOrb orb = manager.Pop();
                Vector2 pos=Vector2.Zero;
                NOrb nOrb=NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.YinYangOrbManager(manager).PopOrb(out pos);
                choiceContext.PushModel(orb);
                await orb.Attack(choiceContext,target,nOrb,pos);
                choiceContext.PopModel(orb);
                orb.RemoveInternal();
            }
        }
    }
}