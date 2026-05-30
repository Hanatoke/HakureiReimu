using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class HelpFromFriends : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => 
            [
                CardKeyword.Exhaust,
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            new HoverTip(TitleLocString,Tip),
            HoverTipFactory.FromCard<Procrastinate>(IsUpgraded)
        ];

        private LocString _tip;
        public LocString Tip => _tip ??= new LocString("cards", Id.Entry + ".tip");

        public HelpFromFriends(
            ) : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<CardModel> choose = GenerateChoose(RunState.Rng.CombatCardGeneration)
                .Select(c => CombatState.CreateCard(c.CanonicalInstance, Owner)).ToList();
            if (IsUpgraded)choose.ForEach(c=>CardCmd.Upgrade(c));
            CardModel c = (choose.Count > 0
                ? (await CardSelectCmd.FromChooseACardScreen(choiceContext, choose, Owner, true))
                : null);
            if (c == null)
            {
                c = CombatState.CreateCard(ModelDb.Card<Procrastinate>(), Owner);
                if (IsUpgraded)CardCmd.Upgrade(c);
            }

            await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, Owner);
        }

        public List<CardModel> GenerateChoose(Rng rng,int count=3)
        {
            List<CardModel> allCards = ModelDb.AllCards.Where(c =>
                (c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare) &&
                Owner.Character.CardPool != c.Pool&& !c.Keywords.Contains(CardKeyword.Unplayable)).Select(c =>
            {
                c = (CardModel)c.MutableClone();
                c.Owner = this.Owner;
                if (IsUpgraded)
                {
                    c.UpgradeInternal();
                }
                return c;
            }).ToList();
            return new CardAnalyzer(CombatState, Owner, allCards).Analyze().GetResultsByBest(rng, count);
        }
    }
}
