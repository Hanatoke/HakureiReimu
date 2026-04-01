using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.PersistCard
{
    public abstract class AbstractPersistCardTable: CustomPile
    {
        protected Dictionary<CardModel, AbstractPersistCardSlot> Slots = new();
        public AbstractPersistCardTable(PileType pileType) : base(pileType)
        {
            base.CardRemoved+=RemoveSlot;
        }

        public new virtual bool IsCombatPile => true;
        public virtual bool AlwaysShowsDynamicVarPreview => true;
        public override bool CardShouldBeVisible(CardModel card)
        {
            return false;
        }
        public override Vector2 GetTargetPosition(CardModel model, Vector2 size)
        {
           return Vector2.Zero;
        }

        public virtual void AddSlot(CardModel card,AbstractPersistCardSlot slot)
        {
            Slots.TryAdd(card, slot);
        }
        public virtual void RemoveSlot(CardModel card)
        {
            Slots.Remove(card);
        }

        public virtual AbstractPersistCardSlot GetSlot(CardModel card)
        {
            if (Slots.TryGetValue(card, out AbstractPersistCardSlot slot))
            {
                return slot;
            }
            return null;
        }
    }
}