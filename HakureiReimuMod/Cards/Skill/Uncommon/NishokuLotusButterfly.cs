using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using HakureiReimu.HakureiReimuMod.PersistCard.Node;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class NishokuLotusButterfly : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new BlockVar(13,ValueProp.Move),
                new IntVar("Count",1),
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(Counter)];

        public NishokuLotusButterfly(
            ) : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        }
        public override bool GainsBlock => true;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            
            int count = DynamicVars["Count"].IntValue;
            AbstractPersistCardTable table = Owner.PlayerCombatState.PersistCardTable(CounterCardTable.PileType);
            if (table == null) return;
            List<CardModel> cards = table.Cards.Where(c=>c.HasCounter()).ToList();
            NPersistCardTable nt = NCombatRoom.Instance?.GetCreatureNode(Owner.Creature)
                ?.PersistCardTable(table);
            foreach (CardModel card in cards)
            {
                if (table.GetSlot(card) is { } slot)
                {
                    await PersistCardCmd.IncreaseCount(slot, count);
                    //VFX Only
                    try
                    {
                        if (!GodotObject.IsInstanceValid(nt))continue;
                        NPersistCardHolder holder=nt.GetCardHolder(card);
                        if (holder==null||!GodotObject.IsInstanceValid(holder))continue;
                        holder.Flash(Colors.Gold);
                    }
                    catch (Exception) 
                    {
                        // ignored
                    }
                }
            }
        }

        protected override void OnUpgrade()
        {
            this.DynamicVars.Block.UpgradeValueBy(3);
        }
    }
}
