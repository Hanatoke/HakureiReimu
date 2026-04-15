using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class SprinkleStarSeal : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellFastA;
        public SprinkleStarSeal(
            ) : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy) {
        }

        private static HashSet<Type> _halfBuff;
        public static HashSet<Type> HalfBuff=>_halfBuff??=PowerHelper.Mutually.Add(PowerHelper.CanModify).Sub(PowerHelper.LessIsMore);
        private static HashSet<Type> _doubleBuff;
        public static HashSet<Type> DoubleBuff => _doubleBuff ??= PowerHelper.CanModify.And(PowerHelper.LessIsMore);
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.LoseBlock(cardPlay.Target, cardPlay.Target.Block / 2);
            foreach (PowerModel p in cardPlay.Target.Powers.ToList())
            {
                if (!p.IsVisible||p.Type==PowerType.None)continue;
                decimal n=0;
                if (p.Type==PowerType.Buff&&HalfBuff.Contains(p.GetType()))
                {
                    n = -(int)MathF.Floor(p.Amount / 2f);
                }
                else if (p.Type==PowerType.Debuff||DoubleBuff.Contains(p.GetType()))
                {
                    n = p.Amount;
                }
                if (p.IsInstanced&&p.StackType==PowerStackType.Counter)
                {
                    await PowerCmd.ModifyAmount(p, n, Owner.Creature, this);
                }
                else
                {
                    await PowerCmd.Apply((PowerModel)p.ClonePreservingMutability(), cardPlay.Target, n, Owner.Creature,
                        this);
                }
            }
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
