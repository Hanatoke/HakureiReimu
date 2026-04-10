using BaseLib.Patches.Content;
using HakureiReimu.HakureiReimuMod.PersistCard.Node;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Extensions
{
    public static class PersistCardTableExtensions
    {
        public static AbstractPersistCardTable PersistCardTable(this PlayerCombatState state,PileType pileType)
        {
            return CustomPiles.GetCustomPile(state,pileType) as AbstractPersistCardTable;
        }

        public static NPersistCardTable PersistCardTable(this NCreature creature, AbstractPersistCardTable table)
        {
            if (table == null)
            {
                return null;
            }
            NPersistCardTable t = creature.GetNodeOrNull<NPersistCardTable>("%"+table.GetType().Name);
            if (t == null)
            {
                t = NPersistCardTable.Create(creature,table,LocalContext.IsMe(creature.Entity));
                creature.AddChildSafely(t);
                t.Name = table.GetType().Name;
                t.UniqueNameInOwner = true;
                t.Owner = creature;
                // table.NTable = t;
            }
            return t;
        }

        public static void ClearPersistCardTable(this NCreature creature)
        {
            foreach (Godot.Node child in creature.GetChildren())
            {
                if (child is NPersistCardTable table)
                {
                    table.Clear();
                }
            }
        }

        public static AbstractPersistCardSlot GetInstanceSlot(this CardModel card)
        {
            if (card is IPersistCard p)
            {
                return p.InstanceSlot;
            }
            if (card.Enchantment is IPersistCard e)
            {
                return e.InstanceSlot;
            }
            if (card.Affliction is IPersistCard a)
            {
                return a.InstanceSlot;
            }
            //TODO:未来可能会有的复数Modify
            return new AbstractPersistCardSlot(card, 0);
        }
    }
}