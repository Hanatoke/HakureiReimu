using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class GensoIchijo : AbstractCard
    {
        public int? TargetIndex;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

        public GensoIchijo(
            ) : base(0, CardType.Skill, CardRarity.Rare, TargetType.None) {
        }

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (IsUpgraded&&card==this)
            {
                await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), 1, Owner);
            }
        }

        protected override PileType GetResultPileType()
        {
            PileType type=base.GetResultPileType();
            if (type==PileType.Discard)
            {
                return TargetSelectCards.Count>0 ? PileType.Draw : type;
            }
            return type;
        }

        public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel source)
        {
            if (card==this&&card.Pile?.Type==PileType.Draw&&TargetIndex!=null)
            {
                List<CardModel> cards = (List<CardModel>)AccessTools.Field(typeof(CardPile),"_cards").GetValue(card.Pile);
                if (cards != null)
                {
                    cards.Remove(this);
                    TargetIndex=Math.Clamp(TargetIndex.Value,0,cards.Count);
                    cards.Insert(TargetIndex.Value,this);
                }
            }
            return Task.CompletedTask;
        }
        public List<CardModel> TargetSelectCards =>
            Owner.PlayerCombatState?.DrawPile.Cards.Where(c => c is not GensoIchijo).ToList()??[];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            TargetIndex=null;
            CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1)
            {
                Cancelable = false
            };
            CardModel c = (await CardSelectCmd.FromSimpleGrid(choiceContext,TargetSelectCards, Owner, prefs))
                .FirstOrDefault();
            if (c!=null)
            {
                TargetIndex = Owner.PlayerCombatState.DrawPile.Cards.IndexOf(c);
                await CardPileCmd.Add(c, PileType.Hand, source: this);
            }
        }
    }
}
