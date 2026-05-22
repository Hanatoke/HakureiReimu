using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Rare;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class BulletMoneyBonusPacketsPower : AbstractPower
    {
        public static readonly string ID = nameof(BulletMoneyBonusPacketsPower);
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
        public List<CardModel> Origin = [];
        public List<CardModel> Replace = [];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [HoverTipFactory.FromCard<BulletMoneyBonusPackets>()];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new StringVar("Replace"),
            new StringVar("Origin"),
        ];

        public void FormatReplace()
        {
            StringBuilder sb = new();
            for (var i = 0; i < Replace.Count; i++)
            {
                CardModel card = Replace[i];
                sb.Append($"\n[blue]{i + 1}[/blue]. ");
                if (card.Pile is not {Type:PileType.Hand})
                {
                    sb.Append($"[color=595959]{card.Title}[/color]");
                }
                else
                {
                    sb.Append(card.Title);
                }
            }
            ((StringVar)this.DynamicVars["Replace"]).StringValue = sb.ToString();
        }

        public void FormatOrigin()
        {
            StringBuilder sb = new();
            for (var i = 0; i < Origin.Count; i++)
            {
                sb.Append($"\n[blue]{i + 1}[/blue]. {Origin[i].Title}");
            }
            ((StringVar)this.DynamicVars["Origin"]).StringValue = sb.ToString();
        }

        public override Task AfterApplied(Creature applier, CardModel cardSource)
        {
            FormatReplace();
            FormatOrigin();
            return Task.CompletedTask;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (Replace.Contains(cardPlay.Card))
            {
                Replace.Remove(cardPlay.Card);
                FormatReplace();
            }
            return Task.CompletedTask;
        }

        public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel source)
        {
            if (Replace.Contains(card))
            {
                FormatReplace();
            }
            return Task.CompletedTask;
        }

        public override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (participants.Contains(Owner) && Owner.IsPlayer)
            {
                Flash();
                await PowerCmd.Remove(this);

                List<CardModel> toRemove = Replace.Where(c=>c.Pile is {Type:PileType.Hand}).ToList();
                await CardPileCmd.RemoveFromCombat(toRemove);
                //换回
                foreach (CardModel card in Origin)
                {
                    card.HasBeenRemovedFromState = false;
                    await CardPileCmd.Add(card, PileType.Hand);
                }
            }
        }

        protected override void DeepCloneFields()
        {
            base.DeepCloneFields();
            Origin = Origin.ToList();
            Replace = Replace.ToList();
        }
    }
}