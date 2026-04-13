using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.Interface.Counter.Hook;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class CounterManager
    {
        public static readonly Dictionary<ICounter, Func<Task>> Later = new();
        public static bool InMonsterMove { get; internal set; } = false;
        public static bool AlreadyForceAttackAnimation { get; internal set; }=false;
        public static bool InInvokeCounter { get; internal set; }= false;
        public static List<CardModel> GetAllCounterCards(this Player player)
        {
            AbstractPersistCardTable table = player.PlayerCombatState?.PersistCardTable(CounterCardTable.PileType);
            return table == null ? [] : table.Cards.Where(c=>c.HasCounter()).ToList();
        }
        public static bool HasCounter(this CardModel card)
        {
            if (card is ICounter) return true;
            if (card.Enchantment is ICounter) return true;
            if (card.Affliction is ICounter) return true;
            return false;
        }
        public static IEnumerable<ICounter> GetCountersForCard(this CardModel card)
        {
            if (card is ICounter counter)
            {
                yield return counter;
            }
            if (card.Enchantment is ICounter e)
            {
                yield return e;
            }
            if (card.Affliction is ICounter a)
            {
                yield return a;
            }
            //TODO:未来版本可能添加的复数Modifier
        }
        public static void AddToLater(ICounter counter, Func<Task> func)
        {
            Later.TryAdd(counter, func);
        }

        public static void CancelLater(ICounter counter)
        {
            if (Later.ContainsKey(counter))
            {
                Later[counter] = null;
            }
        }
        public static async Task RunLater()
        {
            foreach (Func<Task> task in Later.Values)
            {
                if (task!=null)
                {
                    await task();
                }
            }
            Later.Clear();
            AlreadyForceAttackAnimation = false;
        }

        public static async Task ForceAttackAnimation(AttackCommand command)
        {
            if (!AlreadyForceAttackAnimation&&command.Attacker!=null)
            {
                AlreadyForceAttackAnimation=true;
                Traverse traverse = Traverse.Create(command);
                if (traverse.Field<string>("_attackerAnimName").Value!=null&&traverse.Field<bool>("_shouldPlayAnimation").Value)
                {
                    await CreatureCmd.TriggerAnim(traverse.Field<Creature>("_visualAttacker").Value ?? command.Attacker
                        , traverse.Field<string>("_attackerAnimName").Value,
                        traverse.Field<float>("_attackerAnimDelay").Value);
                }
            }
        }

        public static async Task BeforeCounter(CombatState state,ICounter counter,Creature target)
        {
            foreach (AbstractModel l in state.IterateHookListeners())
            {
                if (l is ICounterListener c)
                {
                    await c.BeforeCounter(state,counter,target);
                }
            }
        }

        public static async Task AfterCounter(CombatState state,ICounter counter,Creature target)
        {
            foreach (AbstractModel l in state.IterateHookListeners())
            {
                if (l is ICounterListener c)
                {
                    await c.AfterCounter(state,counter,target);
                }
            }
        }
    }
}