using System;
using System.Collections.Generic;
using System.Linq;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Rare;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using ParryPower = HakureiReimu.HakureiReimuMod.Powers.ParryPower;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class CardAnalyzer
    {
        public ICombatState CombatState { get;}
        public Player Owner { get;}
        public List<CardModel> Cards { get;}
        public Dictionary<CardModel,int> Weights{get;protected set;}
        public Func<CardModel, int> Modifier;
        public bool VerifyResource = true;
        public bool UseSpecial = true;
        protected IRunState RunState=>CombatState.RunState;
        protected PlayerCombatState PlayerCombatState => Owner.PlayerCombatState;
        public int EnemyAttackDamage {get;protected set;}

        public CardAnalyzer(ICombatState combatState, Player owner, List<CardModel> cards)
        {
            CombatState = combatState;
            Owner = owner;
            Cards = cards;
        }

        public CardAnalyzer Analyze(Func<CardModel, int> modifier=null)
        {
            this.Modifier = modifier??this.Modifier;
            EnemyAttackDamage = CalculateEnemyAttackDamage();
            Weights = new Dictionary<CardModel, int>();
            foreach (CardModel card in Cards)
            {
                Weights.TryAdd(card, CalculateWeight(card));
            }
            return this;
        }

        public List<CardModel> GetResultsByBest(Rng rng, int count, bool reverse = false)
        {
            var groups = reverse
                ? Weights.GroupBy(x => x.Value).OrderBy(g => g.Key)
                : Weights.GroupBy(x => x.Value).OrderByDescending(g => g.Key);

            return groups
                .SelectMany(g => g.OrderBy(_ => rng.NextInt()))
                .Select(x => x.Key)
                .Take(count)
                .ToList();
        }

        public List<CardModel> GetResultsByMost(Rng rng, int count, bool reverse = false)
        {
            var groups = reverse
                ? Weights.GroupBy(x => x.Value).OrderBy(g => g.Key)
                : Weights.GroupBy(x => x.Value).OrderByDescending(g => g.Key);
            
            return groups.Select(g =>
            {
                var list = g.ToList();
                return list[rng.NextInt(list.Count)].Key;
            })
            .Take(count)
            .ToList();
        }
        public int CalculateWeight(CardModel card)
        {
            int weight = 0;
            try
            {
                card.UpgradePreviewType = CardUpgradePreviewType.Combat;
                // //可以打出的
                // if (!card.CanPlay())
                // {
                //     if (card.Keywords.Contains(CardKeyword.Retain)&&!PlayerCombatState.HasEnoughResourcesFor(card,out UnplayableReason _))
                //     {
                //         weight -= 30*(card.EnergyCost.GetResolved()-PlayerCombatState.Energy);
                //         weight -= 10 * (card.CurrentStarCost - PlayerCombatState.Stars);
                //     }
                //     else
                //     {
                //         return -100;
                //     }
                // }
                if (!CanCardPlay(card, ref weight)) return -100;
                
                if (card.Keywords.Contains(CardKeyword.Exhaust)||card.Type==CardType.Power) weight += 10;
                
                if (card.Type==CardType.Attack)
                {
                    weight += TryAttack(card);
                }
                if (card.GainsBlock)
                {
                    weight += TryDefense(card);
                }
                weight += TryBuff(card);
                
                if (!card.CanBeGeneratedInCombat) weight = (int) (weight*1.1m);
                if (UseSpecial)
                {
                    weight += Special(card);
                }
                if (Modifier!=null)
                {
                    weight += Modifier.Invoke(card);
                }
                weight = TryRarity(card, weight);
            }
            catch (Exception e)
            {
                HakureiReimuMain.Logger.Info("Card Analyzer Skip By Exception:"+card.Title);
                // HakureiReimuMain.Logger.Info(e.ToString());
                return -100;
            }
            // HakureiReimuMain.Logger.Info("{"+card.Id.Entry+"}"+card.Title+":"+weight);
            return weight;
        }

        public bool CanCardPlay(CardModel card,ref int weight)
        {
            if (card.CanPlay())return true;
            if (PlayerCombatState.HasEnoughResourcesFor(card, out UnplayableReason _)) return false;
            if (!VerifyResource) return true;
            if (card.Keywords.Contains(CardKeyword.Retain))
            {
                weight -= 30*Math.Max(0, card.EnergyCost.GetResolved() - PlayerCombatState.Energy);
                weight -= 10 * Math.Max(0, card.CurrentStarCost - PlayerCombatState.Stars);
                return true;
            }
            
            return false;
        }

        public int CalculateEnemyAttackDamage()
        {
            int enemyAttackDamage = -Owner.Creature.Block;
            enemyAttackDamage -= Owner.Creature.GetPowerAmount<PlatingPower>();//覆甲
            AbstractPersistCardTable table = Owner.PlayerCombatState.PersistCardTable(CounterCardTable.PileType);
            if (table!=null)
            {
                decimal parry = Owner.Creature.GetPowerAmount<ParryPower>();//招架
                foreach (CardModel card in table.Cards)
                {
                    if (card.GainsBlock)
                    {
                        enemyAttackDamage -= (int)(CalculateCardBlock(card)+parry);
                    }
                }
            }
            foreach (Creature t in CombatState.HittableEnemies)
            {
                if (t.IsMonster&&t.Monster is { IntendsToAttack: true } monster)
                {
                    enemyAttackDamage += Math.Max(0, monster.NextMove.Intents.OfType<AttackIntent>()
                        .Select(a => (int)CalculateIntentDamage(a,t,Owner.Creature)).Sum()-t.GetPowerAmount<SealPower>());
                }
            }
            return Math.Max(0, enemyAttackDamage);
        }

        public int TryRarity(CardModel card,int baseValue)
        {
            return card.Rarity switch
            {
                CardRarity.Common => baseValue,
                CardRarity.Uncommon => (int)(baseValue*1.05m),
                CardRarity.Rare => (int)(baseValue*1.1m),
                _ => baseValue
            };
        }

        public int TryAttack(CardModel card)
        {
            int result = 0;
            if (card.DynamicVars.TryGetValue(DamageVar.defaultName, out DynamicVar damageVar)) { }
            else if (card.DynamicVars.TryGetValue(CalculatedDamageVar.defaultName, out damageVar)) { }
            if (damageVar == null) return 0;
            int hitCount = 1;
            if (card.TargetType is TargetType.AnyEnemy or TargetType.AllEnemies)
            {
                hitCount = Math.Max(0, CalculateAttackCount(card));
            }
            else if (card.TargetType is TargetType.RandomEnemy)
            {
                return TryRandomAttack(card);
            }
            List<Creature> targets = CombatState.HittableEnemies.ToList();
            if (targets.Count<=0) return 0;
            List<int> weights=new ();
            foreach (Creature t in targets)
            {
                int needToKill = t.CurrentHp;
                if (!card.Keywords.Contains(AbstractCard.IgnoreDefense)) needToKill += t.Block;
                decimal damage = 0;
                ValueProp prop=ValueProp.Move;
                if (damageVar is DamageVar d)
                {
                    damage = d.BaseValue;
                    prop = d.Props;
                }
                else if (damageVar is CalculatedDamageVar c)
                {
                    damage = c.Calculate(t);
                    prop = c.Props;
                }
                if (card.Tags.Contains(CardTag.OstyAttack)&& PlayerCombatState.GetPet<Osty>() is not { IsAlive: true })
                {
                    damage = 0;
                }
                if (damage<=0) break;
                damage = Hook.ModifyDamage(RunState, CombatState, t, Owner.Creature, damage, prop, card,
                    ModifyDamageHookType.All, CardPreviewMode.Normal, out IEnumerable<AbstractModel> _);
                damage *= hitCount > 0 ? hitCount : Math.Max(0, CalculateAttackCount(card, t));
                if (damage>=needToKill)
                {
                    int w = 30;
                    if (t.IsMonster && t.Monster.IntendsToAttack)
                    {
                        w+= t.Monster.NextMove.Intents.OfType<AttackIntent>()
                            .Select(a => (int)CalculateIntentDamage(a,t,Owner.Creature)).Sum();
                    }
                    if (!card.CanBeGeneratedInCombat) w += 20;
                    weights.Add(w);
                }
                else if (needToKill>0)
                {
                    weights.Add((int)(15m*damage/Math.Min(10000,needToKill)));
                }
            }
            if (weights.Count>0)
            {
                //是aoe?
                result += card.TargetType == TargetType.AllEnemies ? weights.Sum() : weights.Max();
            }
            return result;
        }

        public int TryRandomAttack(CardModel card)
        {
            if (card.DynamicVars.TryGetValue(DamageVar.defaultName, out DynamicVar damageVar)) { }
            else if (card.DynamicVars.TryGetValue(CalculatedDamageVar.defaultName, out damageVar)) { }
            if (damageVar == null) return 0;
            int hitCount = Math.Max(0, CalculateAttackCount(card));
            if (hitCount <= 0) return 0;
            int needCount = 0;
            List<Creature> targets = CombatState.HittableEnemies.ToList();
            if (targets.Count<=0) return 0;
            foreach (Creature t in targets)
            {
                int needToKill = t.CurrentHp;
                if (!card.Keywords.Contains(AbstractCard.IgnoreDefense)) needToKill += t.Block;
                decimal damage = 0;
                ValueProp prop=ValueProp.Move;
                if (damageVar is DamageVar d)
                {
                    damage = d.BaseValue;
                    prop = d.Props;
                }
                else if (damageVar is CalculatedDamageVar c)
                {
                    damage = c.Calculate(t);
                    prop = c.Props;
                }
                if (card.Tags.Contains(CardTag.OstyAttack)&& PlayerCombatState.GetPet<Osty>() is not { IsAlive: true })
                {
                    damage = 0;
                }
                if (damage<=0) break;
                damage = Hook.ModifyDamage(RunState, CombatState, t, Owner.Creature, damage, prop, card,
                    ModifyDamageHookType.All, CardPreviewMode.Normal, out IEnumerable<AbstractModel> _);
                needCount += (int)Math.Ceiling(needToKill / damage);
                //总数不够提前结束
                if (needCount > hitCount) return 0;
            }
            if (needCount<=hitCount)
            {
                return targets.Select(t =>
                {
                    int w = 30;
                    if (t.IsMonster && t.Monster.IntendsToAttack)
                    {
                        w += t.Monster.NextMove.Intents.OfType<AttackIntent>()
                            .Select(a => (int)CalculateIntentDamage(a, t, Owner.Creature)).Sum();
                    }
                    if (!card.CanBeGeneratedInCombat) w += 20;
                    return w;
                }).Sum();
            }
            return 0;
        }

        public int TryDefense(CardModel card)
        {
            decimal block = CalculateCardBlock(card);
            if (block<=0) return 0;
            return (int)Math.Min(block,EnemyAttackDamage);
        }

        public int TryBuff(CardModel card)
        {
            int result = 0;
            if (card.Type==CardType.Power)
            {
                result -= EnemyAttackDamage;
                int rate = CombatState.HittableEnemies.Select(e=>e.CurrentHp).Max();
                rate = (int)Math.Log10(rate) * 3;
                rate = Math.Min(15, rate);
                rate =(int)(rate* 4m / (CombatState.RoundNumber + 3));
                result += Math.Min(PlayerCombatState.Energy,card.EnergyCost.GetResolved())*rate;
            }
            else
            {
                // if (EnemyAttackDamage <= 0 && !card.CanBeGeneratedInCombat) result += 10;
                if (card.DynamicVars.TryGetValue(EnergyVar.defaultName, out DynamicVar energyVar) && card.IsGainEnergyCard())
                {
                    // HakureiReimuMain.Logger.Info("{"+card.Id.Entry+"}"+card.Title+":"+card.IsGainEnergyCard());
                    int energyNeed=PlayerCombatState.Hand.Cards.Select(c=>c.EnergyCost.GetResolved()).Sum()-PlayerCombatState.Energy;
                    result += Math.Min(energyNeed, energyVar.IntValue-card.EnergyCost.GetResolved()) * 3;
                }

                if (card.DynamicVars.TryGetValue(CardsVar.defaultName, out DynamicVar cardsVar) && card.IsDrawCard()) 
                {
                    // HakureiReimuMain.Logger.Info("{"+card.Id.Entry+"}"+card.Title+":"+card.IsDrawCard());
                    int canDraw = CardPile.MaxCardsInHand - PlayerCombatState.Hand.Cards.Count;
                    canDraw = Math.Min(canDraw,PlayerCombatState.DrawPile.Cards.Count+PlayerCombatState.DiscardPile.Cards.Count);
                    result += Math.Min(cardsVar.IntValue,
                        canDraw) * 3;
                }
            }
            return result;
        }
        //--------------------------------------------------------------------------------------------------------------
        public static decimal CalculateIntentDamage(AttackIntent intent,Creature owner,Creature target)
        {
            decimal d = intent.DamageCalc?.Invoke() ?? 0;
            d = Hook.ModifyDamage(target.CombatState.RunState, target.CombatState, target, owner, d, ValueProp.Move, null,
                ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
            return d * intent.Repeats;
        }

        public static int CalculateAttackCount(CardModel card, Creature target=null)
        {
            if (card.DynamicVars.TryGetValue("CalculatedHits", out DynamicVar dynamicVar)&&dynamicVar is CalculatedVar c)
            {
                return (int)c.Calculate(target);
            }

            if (card.DynamicVars.TryGetValue(RepeatVar.defaultName, out DynamicVar repeatVar))
            {
                return repeatVar.IntValue;
            }

            if (card.EnergyCost.CostsX)
            {
                int count = Hook.ModifyXValue(card.CombatState,card,card.Owner.PlayerCombatState.Energy);
                // HakureiReimuMain.Logger.Info("{"+card.Id.Entry+"}"+card.Title+":"+count);
                if (card is HeavenlyDrill d && count >= d.DynamicVars.Energy.IntValue) count *= 2;
                
                return count;
            }
            //Special
            switch (card.Id.Entry)
            {
                case "STARDUST":
                    return Hook.ModifyXValue(card.CombatState, card, card.Owner.PlayerCombatState.Stars);
            }
            return 1;
        }

        public static decimal CalculateCardBlock(CardModel card)
        {
            if (card.CombatState==null) return 0;
            if (card.DynamicVars.TryGetValue(BlockVar.defaultName, out DynamicVar blockVar)){}
            else if (card.DynamicVars.TryGetValue(CalculatedBlockVar.defaultName, out blockVar)){}
            if (blockVar == null) return 0;
            decimal block=0;ValueProp prop=ValueProp.Move;
            if (blockVar is BlockVar b)
            {
                block = b.BaseValue;
                prop = b.Props;
            }else if (blockVar is CalculatedBlockVar c)
            {
                block = c.Calculate(null);
                prop = c.Props;
            }
            if (block<=0) return 0;
            block = Hook.ModifyBlock(card.CombatState, card.Owner.Creature, block, prop, card, new CardPlay()
                {
                    Card =  card,PlayCount = 1,PlayIndex = 1,Resources = new ResourceInfo(){EnergySpent = card.EnergyCost.GetResolved(),EnergyValue = card.Owner.PlayerCombatState.Energy,StarsSpent = card.CurrentStarCost,StarValue = card.Owner.PlayerCombatState.Stars},
                    IsAutoPlay =  false,ResultPile = PileType.Discard,Target = null
                },
                out IEnumerable<AbstractModel> _);
            return block;
        }
        //---------------------------------------------------------------------------------------------------------------

        public int Special(CardModel card)
        {
            switch (card)
            {
                case HelpFromFriends:
                    return -1000;
                case Reboot://重启
                    return -PlayerCombatState.Hand.Cards.Count*3;
                case GlimpseBeyond://彼岸一瞥
                    return -100;
                case Offering://祭品
                    return -(1 - (Owner.Creature.CurrentHp / Owner.Creature.MaxHp)) * 10;
                case IceLance://冰之长枪
                    return -100;
                case Scrawl://潦草
                    return Math.Min(CardPile.MaxCardsInHand - PlayerCombatState.Hand.Cards.Count,PlayerCombatState.DrawPile.Cards.Count+PlayerCombatState.DiscardPile.Cards.Count) * 3;
                case Compact://压缩
                    return PlayerCombatState.Hand.Cards.Count(c => c.Type == CardType.Status) * 3;
                case SecondWind://重振
                    return PlayerCombatState.Hand.Cards.Count(c =>
                        (c.Type == CardType.Status||c.Type==CardType.Curse) || (c.Rarity == CardRarity.Basic&&c.Type!=CardType.Attack)) * 3;
                case Stoke:
                    return PlayerCombatState.Hand.Cards.Count(c =>
                        (c.Type == CardType.Status||c.Type==CardType.Curse) || c.Rarity == CardRarity.Basic) * 3;
                case FlakCannon://散射炮
                    return (PlayerCombatState.DiscardPile.Cards.Count(c => c.Type == CardType.Status) +
                                 PlayerCombatState.Hand.Cards.Count(c => c.Type == CardType.Status) +
                                 PlayerCombatState.DrawPile.Cards.Count(c => c.Type == CardType.Status)) * 2;
                case HiddenGem://味觉宝石
                    return PlayerCombatState.DrawPile.Cards.Count(c=>c.Type == CardType.Power||c.Rarity==CardRarity.Rare)*2;
                case Alchemize://炼药
                    return Owner.PotionSlots.Count(s => s == null) * 5;
                case NotYet://包扎
                    return (1 - Owner.Creature.CurrentHp / Owner.Creature.MaxHp) * 5;
                case PiercingWail://尖啸
                    int n = card.DynamicVars.FirstOrDefault().Value.IntValue;
                    int num = 0;
                    foreach (Creature t in CombatState.HittableEnemies.Where(e=>!e.HasPower<ArtifactPower>()))
                    {
                        if (t.IsMonster && t.Monster.IntendsToAttack)
                        {
                            num+= t.Monster.NextMove.Intents.OfType<AttackIntent>()
                                .Select(a=>a.Repeats).Sum();
                        }
                    }
                    return Math.Min(EnemyAttackDamage, n * num);
                case Cruelty:
                    return Math.Min(5,
                        CombatState.HittableEnemies.Select(e => e.GetPowerAmount<VulnerablePower>()).Max());
                case Tracking://跟踪
                    return Math.Min(5,
                        CombatState.HittableEnemies.Select(e => e.GetPowerAmount<WeakPower>()).Max());
                case Expose://暴露
                    return CombatState.HittableEnemies.Select(e => e.GetPowerAmount<ArtifactPower>()).Max();
                case Purity://净化
                    return -100;
            }

            switch (card.Id.Entry)
            {
                case "BLADE_DANCE"://刀舞
                    return -100;
                case "MARISAMOD-STARLIT_POTION"://星彩药剂
                    return -100;
                case "MARISAMOD-TREASURE_HUNTER"://宝物猎手
                    return RunState.CurrentRoom is CombatRoom { RoomType: RoomType.Elite or RoomType.Boss } ? 0 : -100;
                case "HAKUREIREIMU-STRENGTH"://强化
                    return -15;
                case "HAKUREIREIMU-NO_INTERVAL_BOUNDARY"://无检索结界
                    return -100;
                case "HAKUREIREIMU-DREAM_INNATE"://梦想天生
                    return EnemyAttackDamage;
                case "HAKUREIREIMU-FANTASY_MOON"://幻想之月
                    return EnemyAttackDamage / 2;
                case "HAKUREIREIMU-DIVINE_MIGHT"://神威
                    return Owner.Creature.Powers.Count(p => p.TypeForCurrentAmount == PowerType.Debuff) * 5;
                case "HAKUREIREIMU-CELESTIAL_FLIGHT"://天人飞翔
                    return -10 + Owner.PlayerCombatState.DrawPile.Cards.Count - EnemyAttackDamage;
                case "HAKUREIREIMU-REPEAT_CAST"://复诵
                    return Math.Min(CardPile.MaxCardsInHand - PlayerCombatState.Hand.Cards.Count,(card as RepeatCast)?.CardPlaysThisTurn.Count ?? 0)*3;
            }
            return 0;
        }
    }
}