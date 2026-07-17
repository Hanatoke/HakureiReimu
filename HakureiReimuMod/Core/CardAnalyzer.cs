using System;
using System.Collections.Generic;
using System.Linq;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Rare;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class CardAnalyzer
    {
        public ICombatState CombatState { get;}
        public Player Owner { get;}
        public List<CardModel> Cards { get;}
        public Dictionary<CardModel,int> Weights{get;protected set;}
        public Func<CardAnalyzer,CardModel, int> Modifier;
        public bool VerifyResource = true;//验证费用足够
        public bool UseSpecial = true;//使用特判
        public WeightSetting Setting = new ();
        protected IRunState RunState=>CombatState.RunState;
        protected PlayerCombatState PlayerCombatState => Owner.PlayerCombatState;
        public int EnemyAttackDamageTotal {get;protected set;}
        public Dictionary<Creature,int> EnemiesAttackDamage {get;protected set;}
        public Dictionary<Creature,int> EnemiesAttackCount {get;protected set;}
        public int SelfEnergyNeed {get;protected set;}
        public int SelfCardLack {get;protected set;}
        public bool SelfCanDraw {get;protected set;}
        public decimal SelfHealthRate {get;protected set;}
        public Dictionary<Type,int> EnemiesPowerMax {get;protected set;}
        public Dictionary<Type,int> EnemiesPowerSum {get;protected set;}
        
        public struct WeightSetting
        {
            public int ExhaustAndPowerWeight = 5;//消耗卡和能力牌
            public decimal ExtraRewardsMulti = 1.1m;//局外收益
            public decimal RarityCommonMulti = 1m;//普通牌
            public decimal RarityUncommonMulti = 1.05m;//罕见牌
            public decimal RarityRareMulti = 1.1m;//稀有牌
            public int KillEnemiesWeight = 30;//击杀敌人
            public int FatalWeight = 20;//斩杀收益
            public decimal DamageReductionMulti = 1m;//伤害规避
            public int PowerBuffWeight = 15;//能力强化
            public int DrawCardWeight  = 2;//抽牌
            public int GainEnergyWeight = 3;//加费
            public decimal CardCostMulti = 0.5m;//卡牌自身费用
            public decimal OtherSpecialMulti = 1m;
            public WeightSetting()
            {
            }

            public override string ToString()
            {
                return
                    $"{{{nameof(ExhaustAndPowerWeight)}: {ExhaustAndPowerWeight}, {nameof(ExtraRewardsMulti)}: {ExtraRewardsMulti}, {nameof(RarityCommonMulti)}: {RarityCommonMulti}, {nameof(RarityUncommonMulti)}: {RarityUncommonMulti}, {nameof(RarityRareMulti)}: {RarityRareMulti}, {nameof(KillEnemiesWeight)}: {KillEnemiesWeight}, {nameof(FatalWeight)}: {FatalWeight}, {nameof(DamageReductionMulti)}: {DamageReductionMulti}, {nameof(PowerBuffWeight)}: {PowerBuffWeight}, {nameof(DrawCardWeight)}: {DrawCardWeight}, {nameof(GainEnergyWeight)}: {GainEnergyWeight}, {nameof(CardCostMulti)}: {CardCostMulti}, {nameof(OtherSpecialMulti)}: {OtherSpecialMulti}}}";
            }
        }

        public override string ToString()
        {
            return
                $"{nameof(Cards)}: {Cards.Count}, {nameof(Modifier)}: {Modifier}, {nameof(VerifyResource)}: {VerifyResource}, {nameof(UseSpecial)}: {UseSpecial}, {nameof(Setting)}: {Setting}, {nameof(EnemyAttackDamageTotal)}: {EnemyAttackDamageTotal}, {nameof(EnemiesAttackDamage)}: {PrintCollection(EnemiesAttackDamage.Values)}, {nameof(SelfEnergyNeed)}: {SelfEnergyNeed}, {nameof(SelfCardLack)}: {SelfCardLack}, {nameof(SelfHealthRate)}: {SelfHealthRate}, {nameof(EnemiesPowerMax)}: {PrintCollection(EnemiesPowerMax.Values)}, {nameof(EnemiesPowerSum)}: {PrintCollection(EnemiesPowerSum.Values)}";
        }
        protected string PrintCollection<T>(IEnumerable<T> collection)=>"["+string.Join(",",collection)+"]";

        public void PrintWeight(int maxCount=30,bool reverse=false)
        {
            var groups = reverse
                ? Weights.GroupBy(x => x.Value).OrderBy(g => g.Key)
                : Weights.GroupBy(x => x.Value).OrderByDescending(g => g.Key);
            foreach (var keyValuePair in groups.SelectMany(g => g.ToList()))
            {
                HakureiReimuMain.Logger.Info("{"+keyValuePair.Key.Id.Entry+"}"+keyValuePair.Key.Title+":"+keyValuePair.Value);
                if (maxCount--<0)break;
            }
        }

        public CardAnalyzer(ICombatState combatState, Player owner, List<CardModel> cards)
        {
            CombatState = combatState;
            Owner = owner;
            Cards = cards;
        }

        public CardAnalyzer Analyze(Func<CardAnalyzer,CardModel, int> modifier=null)
        {
            this.Modifier = modifier??this.Modifier;
            Precompute();
            Weights = new Dictionary<CardModel, int>();
            foreach (CardModel card in Cards)
            {
                Weights.TryAdd(card, CalculateWeight(card));
            }
            HakureiReimuMain.Logger.Info("Card Analyzer Finish By Param:");
            HakureiReimuMain.Logger.Info(this.ToString());
            // PrintWeight();
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

        public void Precompute()
        {
            EnemiesAttackDamage = new Dictionary<Creature, int>();
            EnemiesAttackCount = new Dictionary<Creature, int>();
            SelfEnergyNeed = CalculateEnergyNeed();
            SelfCardLack = CalculateCardLack();
            SelfCanDraw = Hook.ShouldDraw(CombatState, Owner, false, out AbstractModel _);
            SelfHealthRate = (decimal)Owner.Creature.CurrentHp / Owner.Creature.MaxHp;
            EnemiesPowerMax = new Dictionary<Type, int>();
            EnemiesPowerSum = new Dictionary<Type, int>();
            foreach (Creature t in CombatState.HittableEnemies)
            {
                if (t.IsMonster && t.Monster is { IntendsToAttack: true } monster &&
                    monster.NextMove.Intents.OfType<AttackIntent>().ToList() is { Count: > 0 } intents)
                {
                    EnemiesAttackDamage[t] = Math.Max(0,
                        intents.Select(a => (int)CalculateIntentDamage(a, t, Owner.Creature)).Sum()
                        - t.GetPowerAmount<SealPower>());
                    EnemiesAttackCount[t]=intents.Select(a=>a.Repeats).Sum();
                }
                else
                {
                    EnemiesAttackDamage[t] = 0;
                    EnemiesAttackCount[t] = 0;
                }
                
                foreach (PowerModel p in t.Powers)
                {
                    Type type = p.GetType();
                    
                    if (EnemiesPowerMax.TryGetValue(type, out int max))
                    {
                        EnemiesPowerMax[type] = Math.Max(max, p.Amount);
                    }
                    else
                    {
                        EnemiesPowerMax[type] = p.Amount;
                    }

                    if (EnemiesPowerSum.TryGetValue(type, out int sum))
                    {
                        EnemiesPowerSum[type] = sum + p.Amount;
                    }
                    else
                    {
                        EnemiesPowerSum[type] = p.Amount;
                    }
                }
            }
            EnemyAttackDamageTotal = CalculateEnemyAttackDamageTotal();
            
        }
        public int GetEnemiesPowerMax<T>() where T : PowerModel=>EnemiesPowerMax.GetValueOrDefault(typeof(T),0);
        public int GetEnemiesPowerSum<T>() where T : PowerModel=>EnemiesPowerSum.GetValueOrDefault(typeof(T),0);
        public int CalculateEnemyAttackDamageTotal()
        {
            //梦想天生
            if (CombatState.Players.Any(p =>
                    p.PlayerCombatState.PersistCardTable(CounterCardTable.PileType)?.Cards.Any(c => c is DreamInnate) == true))
            {
                return 0;
            }
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
                        enemyAttackDamage -= (int)(CalculateCardBlock(card) + parry);
                    }
                }
            }

            if (EnemiesAttackDamage.Count>0)
            {
                enemyAttackDamage += EnemiesAttackDamage.Values.Sum();
            }

            return (int)(Math.Max(0, enemyAttackDamage) * Setting.DamageReductionMulti);
        }

        public int CalculateEnergyNeed()
        {
            return PlayerCombatState.Hand.Cards.Select(c => c.EnergyCost.GetResolved()).Sum()
                   - PlayerCombatState.Energy;
        }

        public int CalculateCardLack()
        {
            return CardPile.MaxCardsInHand - PlayerCombatState.Hand.Cards.Count;
        }
        //-------------------------------------------------------------------------
        public int CalculateWeight(CardModel card)
        {
            int weight = 0;
            try
            {
                card.UpgradePreviewType = CardUpgradePreviewType.Combat;
                
                if (!CanCardPlay(card, ref weight)) return -100;
                
                if (card.Keywords.Contains(CardKeyword.Exhaust)||card.Type==CardType.Power) weight += Setting.ExhaustAndPowerWeight;
                
                if (card.Type==CardType.Attack)
                {
                    weight += TryAttack(card);
                }
                if (card.GainsBlock)
                {
                    weight += TryDefense(card);
                }
                if (card.Type==CardType.Power)
                {
                    weight += TryBuff(card);
                }
                else
                {
                    weight += TryGainEnergy(card,CalculateCardGainEnergy(card));
                    weight += TryDrawCard(card,CalculateCardDrawCount(card));
                    weight += TryCardCost(card);
                }
                
                if (!card.CanBeGeneratedInCombat) weight = (int) (weight*Setting.ExtraRewardsMulti);
                if (UseSpecial)
                {
                    weight += (int)(Special(card) * Setting.OtherSpecialMulti);
                }
                if (Modifier!=null)
                {
                    weight += Modifier.Invoke(this,card);
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
        
        public int TryRarity(CardModel card,int baseValue)
        {
            return card.Rarity switch
            {
                CardRarity.Common => (int)(baseValue*Setting.RarityCommonMulti),
                CardRarity.Uncommon => (int)(baseValue*Setting.RarityUncommonMulti),
                CardRarity.Rare => (int)(baseValue*Setting.RarityRareMulti),
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
                    null,ModifyDamageHookType.All, CardPreviewMode.Normal, out IEnumerable<AbstractModel> _);
                damage *= hitCount > 0 ? hitCount : Math.Max(0, CalculateAttackCount(card, t));
                if (damage>=needToKill)
                {
                    int w = Setting.KillEnemiesWeight;
                    
                    w += (int)(EnemiesAttackDamage.GetValueOrDefault(t, 0) * Setting.DamageReductionMulti);
                    
                    if (!card.CanBeGeneratedInCombat) w += Setting.FatalWeight;
                    weights.Add(w);
                }
                else if (needToKill>0)
                {
                    weights.Add((int)(Setting.KillEnemiesWeight *0.5m * damage/needToKill));
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
                    null,ModifyDamageHookType.All, CardPreviewMode.Normal, out IEnumerable<AbstractModel> _);
                needCount += (int)Math.Ceiling(needToKill / damage);
                //总数不够提前结束
                if (needCount > hitCount) return 0;
            }
            if (needCount<=hitCount)
            {
                return targets.Select(t =>
                {
                    int w = Setting.KillEnemiesWeight;
                    
                    w += (int)(EnemiesAttackDamage.GetValueOrDefault(t, 0) * Setting.DamageReductionMulti);
                    
                    if (!card.CanBeGeneratedInCombat) w += Setting.FatalWeight;
                    return w;
                }).Sum();
            }
            return 0;
        }

        public int TryDefense(CardModel card)
        {
            decimal block = CalculateCardBlock(card);
            if (block<=0) return 0;
            return (int)Math.Min(block, EnemyAttackDamageTotal);
        }

        public int TryBuff(CardModel card)
        {
            int result = 0;
            if (CombatState.HittableEnemies.Count <= 0) return 20;
            
            result -= EnemyAttackDamageTotal;
            decimal rate = CombatState.HittableEnemies.Select(e=>e.CurrentHp).Max();
            rate = (int)Math.Log10((int)rate) * Setting.PowerBuffWeight/5m;
            rate = Math.Min(Setting.PowerBuffWeight, rate);
            rate = Math.Floor(rate * 4m / (CombatState.RoundNumber + 3));
            result += Math.Min(PlayerCombatState.Energy,card.EnergyCost.GetResolved())*(int)rate;
            
            return result;
        }

        public int TryGainEnergy(CardModel card,int amount,bool ignoreModify=false)
        {
            if (!ignoreModify)
            {
                amount = (int)Hook.ModifyEnergyGain(CombatState, Owner, amount, out IEnumerable<AbstractModel> _);
            }
            if (amount <= 0) return 0;
            int result = 0;
            
            result += Math.Min(SelfEnergyNeed, amount) * Setting.GainEnergyWeight;
            
            return result;
        }

        public int TryDrawCard(CardModel card,int amount,bool ignoreVerify=false)
        {
            if (!ignoreVerify && !SelfCanDraw) return 0;
            if (amount <= 0) return 0;
            int result = 0;

            int canDraw = Math.Min(SelfCardLack,PlayerCombatState.DrawPile.Cards.Count+PlayerCombatState.DiscardPile.Cards.Count);
            result += Math.Min(amount, canDraw) * Setting.DrawCardWeight;
            
            return result;
        }

        public int TryCardCost(CardModel card)
        {
            int result = 0;
            
            result -= (card.EnergyCost.CostsX ? Math.Min(4,PlayerCombatState.Energy) : card.EnergyCost.GetResolved()) * 3;
            result -= (card.HasStarCostX ? Math.Min(10,PlayerCombatState.Stars) : card.CurrentStarCost);
            
            return (int)(result * Setting.CardCostMulti);
        }
        //--------------------------------------------------------------------------------------------------------------
        public static decimal CalculateIntentDamage(AttackIntent intent,Creature owner,Creature target)
        {
            decimal d = intent.DamageCalc?.Invoke() ?? 0;
            d = Hook.ModifyDamage(target.CombatState.RunState, target.CombatState, target, owner, d, ValueProp.Move, null,
                null,ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
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
                    Player = card.Owner,
                    Card =  card,PlayCount = 1,PlayIndex = 1,Resources = new ResourceInfo(){EnergySpent = card.EnergyCost.GetResolved(),EnergyValue = card.Owner.PlayerCombatState.Energy,StarsSpent = card.CurrentStarCost,StarValue = card.Owner.PlayerCombatState.Stars},
                    IsAutoPlay =  false,ResultPile = PileType.Discard,Target = null
                },
                out IEnumerable<AbstractModel> _);
            return block;
        }

        public static int CalculateCardGainEnergy(CardModel card)
        {
            if (card.DynamicVars.TryGetValue(EnergyVar.defaultName, out DynamicVar energyVar) && card.IsGainEnergyCard())
            {
                return energyVar.IntValue;
            }
            return 0;
        }

        public static int CalculateCardDrawCount(CardModel card)
        {
            if (card.DynamicVars.TryGetValue(CardsVar.defaultName, out DynamicVar cardsVar) && card.IsDrawCard()) 
            {
                return cardsVar.IntValue;
            }
            return 0;
        }
        //---------------------------------------------------------------------------------------------------------------

        public int Special(CardModel card)
        {
            switch (card)
            {
                case Reboot://重启
                    return -PlayerCombatState.Hand.Cards.Count*Setting.DrawCardWeight;
                case GlimpseBeyond://彼岸一瞥
                    return -100;
                case Offering://祭品
                    return (int)((SelfHealthRate - 1) * 15);
                case IceLance://冰之长枪
                    return -100;
                case Scrawl://潦草
                    return TryDrawCard(card,SelfCardLack);
                case Compact://压缩
                    return PlayerCombatState.Hand.Cards.Count(c => c.Type == CardType.Status) * 3;
                case SecondWind://重振
                    return PlayerCombatState.Hand.Cards.Count(c =>
                        (c.Type == CardType.Status||c.Type==CardType.Curse) || (c.Rarity == CardRarity.Basic&&c.Type!=CardType.Attack)) * 3;
                case Stoke://添彩
                    return PlayerCombatState.Hand.Cards.Count(c =>
                        (c.Type == CardType.Status||c.Type==CardType.Curse) || c.Rarity == CardRarity.Basic) * 3;
                case FlakCannon://散射炮
                    return (PlayerCombatState.DiscardPile.Cards.Count(c => c.Type == CardType.Status) +
                                 PlayerCombatState.Hand.Cards.Count(c => c.Type == CardType.Status) +
                                 PlayerCombatState.DrawPile.Cards.Count(c => c.Type == CardType.Status)) * 2;
                case HiddenGem://味觉宝石
                    if (PlayerCombatState.DrawPile.Cards.Count<=0) return -100;
                    return (int)(((decimal)PlayerCombatState.DrawPile.Cards.Count(c=>c.Type == CardType.Power||c.Rarity==CardRarity.Rare)/PlayerCombatState.DrawPile.Cards.Count)*30m);
                case Alchemize://炼药
                    return Owner.PotionSlots.Count(s => s == null) * 5;
                case NotYet://包扎
                    return (int)((1 - SelfHealthRate) * 10);
                case PiercingWail://尖啸
                    int n = card.DynamicVars.FirstOrDefault().Value.IntValue;
                    int num = 0;
                    foreach (Creature t in CombatState.HittableEnemies.Where(e=>!e.HasPower<ArtifactPower>()))
                    {
                        num += EnemiesAttackCount.GetValueOrDefault(t, 0) *
                               Math.Min(n, EnemiesAttackDamage.GetValueOrDefault(t, 0));
                    }
                    return (int)Math.Min(EnemyAttackDamageTotal, num * Setting.DamageReductionMulti);
                case Cruelty:
                    return Math.Min(5,GetEnemiesPowerMax<VulnerablePower>());
                case Tracking://跟踪
                    return Math.Min(5,GetEnemiesPowerMax<WeakPower>());
                case Expose://暴露
                    return Math.Min(15, GetEnemiesPowerMax<ArtifactPower>());
                case Purity://净化
                    return -100;
                case DecisionsDecisions://抉择抉择
                    return 15;
                case DoubleEnergy://双倍能量
                    int amount = PlayerCombatState.Energy - card.EnergyCost.GetResolved();
                    if (PlayerCombatState.Hand.Cards.Any(c=>c.EnergyCost.CostsX))
                    {
                        return Math.Min(10, amount) * Setting.GainEnergyWeight;
                    }
                    return TryGainEnergy(card, amount);
                case Colossus://巨像
                    return CombatState.HittableEnemies.Where(e => e.HasPower<VulnerablePower>())
                        .Select(e => (int)(EnemiesAttackDamage.GetValueOrDefault(e, 0) / 2m *Setting.DamageReductionMulti)).Sum();
                case Eidolon://幻景
                    return PlayerCombatState.Hand.Cards.Count < 9
                        ? 0
                        : EnemyAttackDamageTotal - EnemiesAttackCount.Values.Sum() -
                          PlayerCombatState.Hand.Cards.Count * Setting.DrawCardWeight;
                case CreativeAi://创造性ai
                    return RunState.CurrentRoom is CombatRoom { RoomType: RoomType.Boss } && CombatState.RoundNumber <= 3 ? 10 : 0;
                case Barricade://壁垒
                    return Owner.Creature.Block > 80 && !Owner.Creature.HasPower<BarricadePower>() ? 10 : 0;
            }

            switch (card.Id.Entry)
            {
                case "BUNDLE_OF_JOY"://新生之喜
                    return SelfCanDraw ? 0 : TryDrawCard(card, card.DynamicVars.Cards.IntValue, true);
                case "FIGHT_THROUGH"://强撑
                    return -10;
                case "DOMINATE"://主宰
                    return Math.Min(20, GetEnemiesPowerMax<VulnerablePower>());
                case "PACTS_END"://契约终结
                    return PlayerCombatState.ExhaustPile.Cards.Count >= card.DynamicVars.Cards.IntValue ? 0 : -100;
                case "PANIC_BUTTON"://应急按钮
                    return -10;
                case "THE_GAMBIT"://孤独一掷
                    decimal amount = CalculateCardBlock(card);
                    if (amount < EnemyAttackDamageTotal) return -100;
                    return (int)(-20 * SelfHealthRate);
                case "BLADE_DANCE"://刀舞
                    return -100;
                case "ABRASIVE"://磨蚀
                    return -100;
                case "MARISAMOD-STARLIT_POTION"://星彩药剂
                    return -100;
                case "MARISAMOD-METEORIC_SHOWER":
                    return -100;
                case "MARISAMOD-TREASURE_HUNTER"://宝物猎手
                    return RunState.CurrentRoom is CombatRoom { RoomType: RoomType.Elite or RoomType.Boss } ? 0 : -100;
                case "HAKUREIREIMU-STRENGTH"://强化
                    return -15;
                case "HAKUREIREIMU-NO_INTERVAL_BOUNDARY"://无检索结界
                    return -100;
                case "HAKUREIREIMU-DREAM_INNATE"://梦想天生
                    return EnemyAttackDamageTotal;
                case "HAKUREIREIMU-FANTASY_MOON"://幻想之月
                    return EnemyAttackDamageTotal / 2;
                case "HAKUREIREIMU-DIVINE_MIGHT"://神威
                    return Owner.Creature.Powers.Count(p => p.TypeForCurrentAmount == PowerType.Debuff) * 5;
                case "HAKUREIREIMU-CELESTIAL_FLIGHT"://天人飞翔
                    return -10 + Owner.PlayerCombatState.DrawPile.Cards.Count - EnemyAttackDamageTotal;
                case "HAKUREIREIMU-REPEAT_CAST"://复诵
                    return Math.Min(SelfCardLack,(card as RepeatCast)?.CardPlaysThisTurn.Count ?? 0)*Setting.DrawCardWeight;
                case "HAKUREIREIMU-HELP_FROM_FRIENDS"://友人之助
                    return 20;
            }
            return 0;
        }
    }
}