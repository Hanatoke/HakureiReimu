using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

// ReSharper disable ClassNeverInstantiated.Global

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Rare {
    public class RainbowDragonYinYangOrbs : AbstractCard {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromOrb<YinYangOrb>(),
            HoverTipFactory.FromCard<RainbowDragonYinYangOrbsFireWater>(),
            HoverTipFactory.FromCard<RainbowDragonYinYangOrbsStormMountain>(),
            HoverTipFactory.FromCard<RainbowDragonYinYangOrbsWindThunder>(),
        ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongB;
        public RainbowDragonYinYangOrbs(
            ) : base(2, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<CardModel> choose = GetChoose(Owner,CombatState);
            if (choose.Count <= 0)return;
            if (choose.Count == 1)
            {
                await ChooseWrapper(choose[0], choiceContext, cardPlay);
                return;
            }
            CardModel card = await CardSelectCmd.FromChooseACardScreen(choiceContext, choose, Owner);
            if (card != null)
            {
                await ChooseWrapper(card, choiceContext, cardPlay);
            }
        }

        public async Task ChooseWrapper(CardModel card,PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (AccessTools.Method(typeof(CardModel), "OnPlay")?.Invoke(card, [choiceContext, cardPlay]) is Task invoke)
            {
                await invoke;
            }
            // await CardCmd.AutoPlay(choiceContext, card, Owner.Creature);
        }

        public static List<CardModel> GetChoose(Player  player,CombatState state)
        {
            List<CardModel> choose = [];
            if (!player.Creature.HasPower<RainbowDragonYinYangOrbsFireWaterPower>())
            {
                choose.Add(state.CreateCard<RainbowDragonYinYangOrbsFireWater>(player));
            }
            if (!player.Creature.HasPower<RainbowDragonYinYangOrbsStormMountainPower>())
            {
                choose.Add(state.CreateCard<RainbowDragonYinYangOrbsStormMountain>(player));
            }
            if (!player.Creature.HasPower<RainbowDragonYinYangOrbsWindThunderPower>())
            {
                choose.Add(state.CreateCard<RainbowDragonYinYangOrbsWindThunder>(player));
            }
            return choose;
        }
        protected override void OnUpgrade() 
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public class RainbowDragonYinYangOrbsFireWater : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromOrb<YinYangOrb>(), HoverTipFactory.Static(StaticHoverTip.Block)
        ];
        public override bool CanBeGeneratedInCombat =>false;
        public RainbowDragonYinYangOrbsFireWater(
        ) : base(-1, CardType.Power, CardRarity.Token, TargetType.Self,false) {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<RainbowDragonYinYangOrbsFireWaterPower>(Owner.Creature, 1, Owner.Creature, this);
        }
    }

    public class RainbowDragonYinYangOrbsStormMountain : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromOrb<YinYangOrb>(), HoverTipFactory.Static(StaticHoverTip.Block)
        ];
        public override bool CanBeGeneratedInCombat =>false;
        public RainbowDragonYinYangOrbsStormMountain(
        ) : base(-1, CardType.Power, CardRarity.Token, TargetType.Self,false) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<RainbowDragonYinYangOrbsStormMountainPower>(Owner.Creature, 1, Owner.Creature, this);
        }
    }

    public class RainbowDragonYinYangOrbsWindThunder : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>(),];
        public override bool CanBeGeneratedInCombat =>false;
        public RainbowDragonYinYangOrbsWindThunder(
        ) : base(-1, CardType.Power, CardRarity.Token, TargetType.Self,false) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<RainbowDragonYinYangOrbsWindThunderPower>(Owner.Creature, 1, Owner.Creature, this);
        }
    }
}
