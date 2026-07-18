using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class FortuneDraw : AbstractCard
    {
        private LocString _tip;
        public LocString Tip => _tip ??= new LocString("cards", this.Id.Entry + ".tip");
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [new HoverTip(TitleLocString, Tip)];

        public FortuneDraw(
            ) : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly) {
        }
        protected static readonly FieldInfo PowerField = AccessTools.Field(typeof(Creature),"_powers");
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target is not {IsPlayer:true})return;
            Player player = cardPlay.Target.Player;
            //生成所有可用卡牌
            List<CardModel> cards = player.Character.CardPool
                .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).Where(c =>
                    c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare && c.CanBeGeneratedInCombat && c is not FortuneDraw)
                .Select(c =>
                {
                    try
                    {
                        c = (CardModel)c.MutableClone();
                        c.Owner = player;
                        if (IsUpgraded)
                        {
                            c.UpgradeInternal();
                        }
                        return c;
                    }catch { return null; }
                }).Where(c=>c!=null).ToList();
            //分析
            cards = new CardAnalyzer(CombatState,player,cards).Analyze()
                .GetResultsByBest(player.RunState.Rng.CombatCardGeneration,1);
            //生成
            await CardPileCmd.AddGeneratedCardsToCombat(cards.Select(c =>
            {
                c = player.Creature.CombatState.CreateCard(c.CanonicalInstance, player);
                if (this.IsUpgraded)
                {
                    CardCmd.Upgrade(c);
                }
                return c;
            }).ToList(),PileType.Hand,Owner);
        }
    }
}
