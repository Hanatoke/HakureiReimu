using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Interface;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class YinYangOrbHook
    {
        public static decimal ModifyOrbValue(YinYangOrb orb,decimal result)
        {
            result+=orb.Owner.Creature.GetPowerAmount<StrengthPower>();
            foreach (AbstractModel i in orb.CombatState.IterateHookListeners())
            {
                if (i is IYinYangOrbListener listener)
                {
                    result = listener.ModifyEvokeVal(orb, result);
                }
            }
            return result;
        }

        public static async Task AfterEvokeOrb(PlayerChoiceContext choiceContext,YinYangOrb orb,Player player,Creature target,CardModel cardSource)
        {
            foreach (AbstractModel i in orb.CombatState.IterateHookListeners())
            {
                if (i is IYinYangOrbListener listener)
                {
                    await listener.AfterEvokeOrb(choiceContext, orb, player, target, cardSource);
                }
            }
        }

        public static async Task AfterOrbHit(PlayerChoiceContext choiceContext,YinYangOrb orb,IEnumerable<DamageResult> damageResult)
        {
            IEnumerable<DamageResult> results = damageResult.ToList();
            foreach (AbstractModel i in orb.CombatState.IterateHookListeners())
            {
                if (i is IYinYangOrbListener listener)
                {
                    await listener.AfterOrbHit(choiceContext, orb, results);
                }
            }
        }
    }
}