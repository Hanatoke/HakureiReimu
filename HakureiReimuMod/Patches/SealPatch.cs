using System.Collections;
using System.Collections.Generic;
using HakureiReimu.HakureiReimuMod.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class SealPatch
    {
        [HarmonyPatch(typeof(CreatureCmd),nameof(CreatureCmd.Damage),typeof(PlayerChoiceContext),typeof(IEnumerable<Creature>),typeof(decimal),typeof(ValueProp),
            typeof(Creature),typeof(CardModel))]
        public static class DamageCmdPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets,
                ref decimal amount, ref ValueProp props, Creature dealer, CardModel cardSource)
            {
                if (dealer!=null && dealer.IsAlive)
                {
                    foreach (PowerModel p in new List<PowerModel>(dealer.Powers))
                    {
                        if (p is SealPower seal)
                        {
                            seal.ModifyDamage(ref amount, ref props,dealer,cardSource,targets);
                        }
                    }
                }
                return true;
            }
        }
    }
}