using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Patches;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class ExtraDrawPower : AbstractPower,CardPileCmdPatch.IDrawCardListener
    {
        public static readonly string ID = nameof(ExtraDrawPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public readonly Dictionary<Player,HashSet<PlayerChoiceContext>> Context = new();
        public class IgnoreExtraDrawContext:BlockingPlayerChoiceContext{}
        public async Task AfterDrawCardFinish(PlayerChoiceContext choiceContext, decimal count, Player player, bool fromHandDraw)
        {
            if (player!=Owner.Player)return;
            if (choiceContext is IgnoreExtraDrawContext)return;
            if (Context.TryGetValue(player,out HashSet<PlayerChoiceContext> set))
            {
                set.Remove(choiceContext);
            }
            if (!Context.ContainsKey(player)||Context[player].Count<=0)
            {
                Flash();
                await CardPileCmd.Draw(new IgnoreExtraDrawContext(),Amount,player);
            }
        }

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Owner==Owner.Player)
            {
                if (choiceContext is IgnoreExtraDrawContext)
                {
                    await PowerCmd.Decrement(this);
                }
                else
                {
                    if (Context.TryGetValue(Owner.Player, out HashSet<PlayerChoiceContext> ctx))
                    {
                        ctx.Add(choiceContext);
                    }
                    else
                    {
                        Context.Add(Owner.Player,[choiceContext]);
                    }
                }
            }
        }
    }
}