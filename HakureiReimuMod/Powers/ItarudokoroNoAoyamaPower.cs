using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Patches;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class ItarudokoroNoAoyamaPower : AbstractPower,IPersistCardSubscriber
    {
        public static readonly string ID = nameof(ItarudokoroNoAoyamaPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public async Task OnStartPersistCard(AbstractPersistCardSlot slot)
        {
            if (slot.Card!=null&&slot.Card.Owner.Creature==Owner&&slot.Card.HasCounter())
            {
                Flash();
                await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), Amount, slot.Card.Owner);
            }
        }
    }
}