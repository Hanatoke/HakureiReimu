using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class WoodenFish:AbstractRelic,ICounter
    {
        public override RelicRarity Rarity => RelicRarity.Rare;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(AbstractCard.Counter),
            HoverTipFactory.FromKeyword(AbstractCard.Attack),
        ];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
        public Creature CounterOwner => Owner.Creature;
        public override async Task AfterAttack(AttackCommand command)
        {
            if (command.Attacker is { IsMonster: true } &&
                command.DamageProps.IsCardOrMonsterMove_())
            {
                await InvokeCounter(command.Attacker);
            }
        }
        public virtual async Task InvokeCounter(Creature target)
        {
            if (Owner.PlayerCombatState==null)
            {
                MainFile.Logger.Warn("尝试发动不在战斗中的反制? "+this.GetType().Name);
                return;
            }
            if (CounterManager.InInvokeCounter)return;
            if (CounterManager.InMonsterMove)
            {
                CounterManager.AddToLater(this,async  () => await CounterCmd.InvokeCounter(Owner.Creature.CombatState,this,target));
            }
            else
            {
                await CounterCmd.InvokeCounter(Owner.Creature.CombatState, this, target);
            }
        }
        public async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            if (Owner.PlayerCombatState == null)return;
            Flash();
            await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), DynamicVars.Cards.BaseValue, Owner);
        }
    }
}