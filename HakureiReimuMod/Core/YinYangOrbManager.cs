using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using HakureiReimu.HakureiReimuMod.Command;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class YinYangOrbManager :AbstractModel,ICustomModel
    {
        public int Capacity = 7;
        public List<YinYangOrb> Orbs { get; private set; }
        public Player Player{get; set;}

        public YinYangOrbManager()
        {
            Orbs = new List<YinYangOrb>(Capacity);
        }
        // public YinYangOrbManager(Player player)
        // {
        //     Player = player;
        //     Orbs = new List<YinYangOrb>(Capacity);
        // }
        public void Clear()=> Orbs.Clear();

        public void Add(YinYangOrb orb)
        {
            Orbs.Add(orb);
        }

        public YinYangOrb Pop()
        {
            YinYangOrb orb=Orbs[Orbs.Count - 1];
            Orbs.RemoveAt(Orbs.Count - 1);
            return orb;
        }

        public override bool ShouldReceiveCombatHooks => true;
        
        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props,
            Creature target, CardModel cardSource)
        {
            if (this.Orbs.Count > 0 && dealer == Player.Creature && props.IsPoweredAttack_() && cardSource != null) 
            {
                await YinYangOrbCmd.Evoke(choiceContext, Player, target,cardSource);
            }
        }
    }
}