using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Core;
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
    public class BoundarySensor:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Uncommon;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(AbstractCard.Counter)
        ];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
        public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
        {
            if (player==Owner&&player.Creature.CombatState?.RoundNumber==1&&player.PlayerCombatState!=null)
            {
                int num = DynamicVars.Cards.IntValue;
                if (num<=0)return;
                List<CardModel> toMove = [];
                foreach (CardModel card in player.PlayerCombatState.DrawPile.Cards)
                {
                    if (card.HasCounter())
                    {
                        toMove.Add(card);
                        num--;
                        if (num<=0)break;
                    }
                }
                if (toMove.Count<=0)return;
                Flash();
                await CardPileCmd.Add(toMove, PileType.Hand, source: this);
            }
        }
    }
}