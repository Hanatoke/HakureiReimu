using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Common;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class MoriyaGohei:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Miracle>();
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar( 1)];
        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
        {
            if (player!=Owner) return;
            Flash();
            List<CardModel> cards = [];
            for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            {
                cards.Add(combatState.CreateCard<Miracle>(Owner));
            }
            await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, true);
        }
    }
}