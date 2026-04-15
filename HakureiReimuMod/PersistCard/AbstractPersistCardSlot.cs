using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using HakureiReimu.HakureiReimuMod.PersistCard.Node;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.PersistCard
{
    public class AbstractPersistCardSlot
    {
        public static readonly Color PositiveColor = new Color(0F, 1F, 0F);
        public static readonly Color NegativeColor = new Color(1F, 0F, 0F);
        public static readonly Color NeutralColor = new Color(1F, 1F, 1F);
        public virtual CardModel Card { get; protected set; }
        protected int _count;
        public virtual AbstractPersistCardTable Table=>Card.Pile as AbstractPersistCardTable;

        public virtual NPersistCardTable NTable =>
            NCombatRoom.Instance?.GetCreatureNode(Card.Owner.Creature)?.PersistCardTable(Table);

        public AbstractPersistCardSlot(CardModel card, int count)
        {
            Card = card;
            _count = count;
        }

        public virtual int Count
        {
            get=>_count;
            set
            {
                if (value>_count)
                {
                    FlashCount(PositiveColor);
                    PlaySfx(true);
                }
                else if(value<_count)
                {
                    FlashCount(NegativeColor);
                    PlaySfx(false);
                }
                _count = value;
                UpdateDisplayCount();
            }
        }

        public virtual void FlashCount(Color color)
        {
            NTable?.GetCardHolder(Card)?.FlashCount(color);
        }

        public virtual bool ShouldDisplayCount => Count != 0;

        public virtual void UpdateDisplayCount()
        {
            NTable?.GetCardHolder(Card)?.SetCount(Count,ShouldDisplayCount);
        }

        public virtual async Task OnStart()
        {
            await Cmd.Wait(0.25f);
            PlaySfx(true);
            NTable?.GetCardHolder(Card)?.Flash(NeutralColor);
        }
        public virtual Task OnEnd()
        {
            PlaySfx(false);
            return Task.CompletedTask;
        }

        public virtual void PlaySfx(bool polarity)
        {
            if (polarity)
            {
                SfxCmd.Play("event:/sfx/buff");
            }
            else
            {
                SfxCmd.Play("event:/sfx/debuff");
            }
        }

        public override string ToString()
        {
            return $"{nameof(Card)}: {Card}, {nameof(Table)}: {Table}, {nameof(CombatState)}: {Card?.CombatState}";
        }
    }
}