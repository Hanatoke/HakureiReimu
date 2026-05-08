using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class BulletMoneyBonusPackets : AbstractCard
    {
        private static LocString _tip;
        public LocString Tip => _tip ??= new LocString("cards", this.Id.Entry + ".tip");
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [new HoverTip(TitleLocString, Tip)];

        public BulletMoneyBonusPackets(
            ) : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target is not {IsPlayer:true})return;
            //移除
            List<CardModel> origin = Owner.PlayerCombatState.Hand.Cards.ToList();
            await CardPileCmd.RemoveFromCombat(origin);
            //复制
            List<CardModel> targetCards=cardPlay.Target.Player.PlayerCombatState.Hand.Cards.ToList();
            List<CardModel> copes = [];
            foreach (CardModel card in targetCards)
            {
                CardModel copy = card.CreateClone();
                copy.Owner = null;
                copy.Owner = Owner;
                copes.Add(copy);
                await CardPileCmd.Add(copy, PileType.Hand);
            }
            //存储
            BulletMoneyBonusPacketsPower power = (BulletMoneyBonusPacketsPower)ModelDb.Power<BulletMoneyBonusPacketsPower>().MutableClone();
            power.Origin = origin;
            power.Replace = copes;
            power.Applier = Owner.Creature;
            int index=Owner.Creature.Powers.FirstIndex(p=>p is BulletMoneyBonusPacketsPower);
            power.ApplyInternal(Owner.Creature,1,true);
            if (index>=0)
            {
                if (AccessTools.Field(typeof(Creature),"_powers").GetValue(Owner.Creature) is List<PowerModel> powers)
                {
                    powers.Remove(power);
                    index = Math.Min(index, powers.Count);
                    powers.Insert(index,power);
                }
            }
            await power.AfterApplied(Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
