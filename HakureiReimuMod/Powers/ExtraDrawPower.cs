using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Patches;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
        public HashSet<PlayerChoiceContext> InProgress=[];
        public HashSet<PlayerChoiceContext> Context = new();

        public class IgnoreExtraDrawContext : BlockingPlayerChoiceContext;

        protected override void DeepCloneFields()
        {
            base.DeepCloneFields();
            InProgress = InProgress.ToHashSet();
            Context = Context.ToHashSet();
        }

        public virtual Task BeforeDrawCardStart(PlayerChoiceContext choiceContext, decimal count, Player player, bool fromHandDraw)
        {
            if (player==Owner.Player)
            {
                InProgress.Add(choiceContext);
            }
            return Task.CompletedTask;
        }

        public virtual async Task AfterDrawCardFinish(PlayerChoiceContext choiceContext, decimal count, Player player, bool fromHandDraw)
        {
            if (player==null||player!=Owner.Player)return;
            InProgress.Remove(choiceContext);
            if (choiceContext is IgnoreExtraDrawContext)return;
            if (Context.Remove(choiceContext))
            {
                if (Context.Count<=0)
                {
                    Flash();
                    await CardPileCmd.Draw(new IgnoreExtraDrawContext(), Amount, player, fromHandDraw);
                }
            }
        }
        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Owner == Owner.Player && InProgress.Contains(choiceContext)) 
            {
                if (choiceContext is IgnoreExtraDrawContext)
                {
                    await PowerCmd.Decrement(this);
                }
                else 
                {
                    Context.Add(choiceContext);
                }
            }
        }
        public override Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player.Creature==Owner)
            {
                InProgress.Clear();
                Context.Clear();
            }
            return Task.CompletedTask;
        }
    }
}