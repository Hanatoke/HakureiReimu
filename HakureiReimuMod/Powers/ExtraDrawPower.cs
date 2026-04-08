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
        public readonly HashSet<PlayerChoiceContext> Context = new();
        public async Task AfterDrawCardFinish(PlayerChoiceContext choiceContext, decimal count, Player player, bool fromHandDraw)
        {
            if (player==Owner.Player)
            {
                if (Context.Contains(choiceContext))
                {
                    Context.Remove(choiceContext);
                    return;
                }

                BlockingPlayerChoiceContext ctx = new BlockingPlayerChoiceContext();
                Context.Add(ctx);
                Flash();
                await CardPileCmd.Draw(ctx,Amount,player);
            }
        }

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Owner==Owner.Player)
            {
                if (Context.Contains(choiceContext))
                {
                    await PowerCmd.Decrement(this);
                }
            }
        }
    }
}