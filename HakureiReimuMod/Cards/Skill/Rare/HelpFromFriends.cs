using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class HelpFromFriends : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => 
            [
                CardKeyword.Exhaust,
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromCard<Procrastinate>(IsUpgraded)
        ];

        public HelpFromFriends(
            ) : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<CardModel> choose = GenerateChoose(RunState.Rng.CombatCardGeneration)
                .Select(c => CombatState.CreateCard(c.CanonicalInstance, Owner)).ToList();
            if (IsUpgraded)choose.ForEach(c=>CardCmd.Upgrade(c));
            CardModel c = (choose.Count > 0
                ? (await CardSelectCmd.FromChooseACardScreen(choiceContext, choose, Owner, true))
                : null);
            if (c == null)
            {
                c = CombatState.CreateCard(ModelDb.Card<Procrastinate>(), Owner);
                if (IsUpgraded)CardCmd.Upgrade(c);
            }

            await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, Owner);
        }

        public List<CardModel> GenerateChoose(Rng rng,int count=3)
        {
            List<CardModel> allCards = ModelDb.AllCards.Where(c =>
                (c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare) &&
                Owner.Character.CardPool != c.Pool&& !c.Keywords.Contains(CardKeyword.Unplayable)).Select(c =>
            {
                c = (CardModel)c.MutableClone();
                c.Owner = this.Owner;
                if (IsUpgraded)
                {
                    c.UpgradeInternal();
                }
                return c;
            }).ToList();
            int enemyAttackDamage = CalculateEnemyAttackDamage();
            Dictionary<CardModel,int> weights = new();
            allCards.ForEach(c=>weights.TryAdd(c,CalculateWeight(c,enemyAttackDamage)));
            var sorted = weights
                .GroupBy(x => x.Value)
                .OrderByDescending(g => g.Key)
                .SelectMany(g => g
                    .OrderBy(_ => rng.NextInt())).ToList();
            for (var i = 0; i < Math.Min(20,sorted.Count); i++)
            {
                KeyValuePair<CardModel, int> pair = sorted[i];
                HakureiReimuMain.Logger.Info(pair.Key.Title+":"+pair.Value+":{"+pair.Key.Id.Entry+"}");
            }
            return sorted.Select(x => x.Key).Take(count).ToList();
            // return weights
            //     .GroupBy(x => x.Value)
            //     .OrderByDescending(g => g.Key)
            //     .Select(g =>
            //     {
            //         var list = g.ToList();
            //         return list[rng.NextInt(list.Count)].Key;
            //     })
            //     .Take(count)
            //     .ToList();
        }

        public int CalculateWeight(CardModel card,int enemyAttackDamage)
        {
            int weight = 0;
            try
            {
                card.UpgradePreviewType = CardUpgradePreviewType.Combat;
                //可以打出的
                if (!card.CanPlay())
                {
                    if (card.Keywords.Contains(CardKeyword.Retain)&&!Owner.PlayerCombatState.HasEnoughResourcesFor(card,out UnplayableReason _))
                    {
                        weight -= 30*(card.EnergyCost.GetResolved()-Owner.PlayerCombatState.Energy);
                        weight -= 10 * (card.CurrentStarCost - Owner.PlayerCombatState.Stars);
                    }
                    else
                    {
                        return -100;
                    }
                }
                if (card.Keywords.Contains(CardKeyword.Exhaust)||card.Type==CardType.Power) weight += 10;
                
                if (card.Type==CardType.Attack)
                {
                    weight += TryAttack(card);
                }
                if (card.GainsBlock)
                {
                    weight += TryDefense(card,enemyAttackDamage);
                }
                weight += TryBuff(card,enemyAttackDamage);
                
                if (!card.CanBeGeneratedInCombat) weight = (int) (weight*1.1m);
                weight += Special(card,enemyAttackDamage);
                weight = TryRarity(card, weight);
            }
            catch (Exception e)
            {
                // HakureiReimuMain.Logger.Info("Skip:"+card.Title);
                // HakureiReimuMain.Logger.Info(e.ToString());
                return -100;
            }
            // HakureiReimuMain.Logger.Info(""+card.Title+":"+weight);
            return weight;
        }

        public int CalculateEnemyAttackDamage()
        {
            int enemyAttackDamage = -Owner.Creature.Block-Owner.Creature.GetPowerAmount<PlatingPower>();
            foreach (Creature t in CombatState.HittableEnemies)
            {
                if (t.IsMonster&&t.Monster is { IntendsToAttack: true } monster)
                {
                    enemyAttackDamage += monster.NextMove.Intents.OfType<AttackIntent>()
                        .Select(a => (int)CalculateIntentDamage(a,t,Owner.Creature)).Sum();
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
            if (card.TargetType is TargetType.AnyEnemy or TargetType.AllEnemies&&card.DynamicVars.TryGetValue(RepeatVar.defaultName, out DynamicVar repeatVar))
            {
                hitCount = Math.Max(1, repeatVar.IntValue);
            }
            List<Creature> targets = CombatState.HittableEnemies.ToList();
            List<int> weights=new ();
            foreach (Creature t in targets)
            {
                int needToKill = Math.Min(10000,t.CurrentHp);
                if (!card.Keywords.Contains(IgnoreDefense)) needToKill += t.Block;
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
                if (card.Tags.Contains(CardTag.OstyAttack)&& Owner.PlayerCombatState.GetPet<Osty>() is not { IsAlive: true })
                {
                    damage = 0;
                }
                if (damage<=0) break;
                damage = Hook.ModifyDamage(RunState, CombatState, t, Owner.Creature, damage, prop, card,
                    ModifyDamageHookType.All, CardPreviewMode.Normal, out IEnumerable<AbstractModel> _);
                damage *= hitCount;
                if (damage>=needToKill)
                {
                    int w = 30;
                    if (t.IsMonster && t.Monster.IntendsToAttack)
                    {
                        w+= t.Monster.NextMove.Intents.OfType<AttackIntent>()
                            .Select(a => (int)CalculateIntentDamage(a,t,Owner.Creature)).Sum();
                    }
                    if (!card.CanBeGeneratedInCombat) w += 10;
                    weights.Add(w);
                }
                else if (needToKill>0)
                {
                    weights.Add((int)(15m*damage/needToKill));
                }
            }
            if (weights.Count>0)
            {
                //是aoe?
                result += card.TargetType == TargetType.AllEnemies ? weights.Sum() : weights.Max();
            }
            return result;
        }

        public int TryDefense(CardModel card,int enemyAttackDamage)
        {
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
            block = Hook.ModifyBlock(CombatState, Owner.Creature, block, prop, card, new CardPlay()
                {
                    Card =  card,PlayCount = 1,PlayIndex = 1,Resources = new ResourceInfo(){EnergySpent = card.EnergyCost.GetResolved(),EnergyValue = Owner.PlayerCombatState.Energy,StarsSpent = card.CurrentStarCost,StarValue = Owner.PlayerCombatState.Stars},
                    IsAutoPlay =  false,ResultPile = PileType.Discard,Target = null
                },
                out IEnumerable<AbstractModel> _);
            if (block<=0) return 0;
            return (int)Math.Min(block,enemyAttackDamage);
        }

        public int TryBuff(CardModel card,int enemyAttackDamage)
        {
            int result = 0;
            if (card.Type==CardType.Power)
            {
                result -= enemyAttackDamage;
                int rate = CombatState.HittableEnemies.Select(e=>e.CurrentHp).Max();
                rate = (int)Math.Log10(rate) * 3;
                rate = Math.Min(15, rate);
                rate =(int)(rate* 4m / (CombatState.RoundNumber + 3));
                result += Math.Min(Owner.PlayerCombatState.Energy,card.EnergyCost.GetResolved())*rate;
            }
            else
            {
                if (enemyAttackDamage <= 0 && !CanBeGeneratedInCombat) result += 10;
                if (card.DynamicVars.TryGetValue(EnergyVar.defaultName, out DynamicVar energyVar))
                {
                    int energyNeed=Owner.PlayerCombatState.Hand.Cards.Select(c=>c.EnergyCost.GetResolved()).Sum()-Owner.PlayerCombatState.Energy;
                    result += Math.Min(energyNeed, energyVar.IntValue-card.EnergyCost.GetResolved()) * 3;
                }
                if (card.DynamicVars.TryGetValue(CardsVar.defaultName, out DynamicVar cardsVar))
                {
                    int canDraw = CardPile.MaxCardsInHand - Owner.PlayerCombatState.Hand.Cards.Count;
                    canDraw = Math.Min(canDraw,Owner.PlayerCombatState.DrawPile.Cards.Count+Owner.PlayerCombatState.DiscardPile.Cards.Count);
                    result += Math.Min(cardsVar.IntValue,
                        canDraw) * 3;
                }
            }
            return result;
        }
        public static decimal CalculateIntentDamage(AttackIntent intent,Creature owner,Creature target)
        {
            decimal d = intent.DamageCalc?.Invoke() ?? 0;
            d = Hook.ModifyDamage(target.CombatState.RunState, target.CombatState, target, owner, d, ValueProp.Move, null,
                ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
            return d * intent.Repeats;
        }

        public int Special(CardModel card,int enemyAttackDamage)
        {
            switch (card)
            {
                case HelpFromFriends:
                    return -1000;
                case Reboot://重启
                    return -Owner.PlayerCombatState.Hand.Cards.Count*3;
                case GlimpseBeyond://彼岸一瞥
                    return -100;
                case Offering://祭品
                    return -(1 - (Owner.Creature.CurrentHp / Owner.Creature.MaxHp)) * 10;
                case IceLance://冰之长枪
                    return -100;
                case Scrawl://潦草
                    return Math.Min(CardPile.MaxCardsInHand - Owner.PlayerCombatState.Hand.Cards.Count,Owner.PlayerCombatState.DrawPile.Cards.Count+Owner.PlayerCombatState.DiscardPile.Cards.Count) * 3;
                case Compact://压缩
                    return Owner.PlayerCombatState.Hand.Cards.Count(c => c.Type == CardType.Status) * 3;
                case SecondWind://重振
                    return Owner.PlayerCombatState.Hand.Cards.Count(c =>
                        (c.Type == CardType.Status||c.Type==CardType.Curse) || (c.Rarity == CardRarity.Basic&&c.Type!=CardType.Attack)) * 3;
                case FlakCannon://散射炮
                    return (Owner.PlayerCombatState.DiscardPile.Cards.Count(c => c.Type == CardType.Status) +
                                 Owner.PlayerCombatState.Hand.Cards.Count(c => c.Type == CardType.Status) +
                                 Owner.PlayerCombatState.DrawPile.Cards.Count(c => c.Type == CardType.Status)) * 2;
                case HiddenGem://味觉宝石
                    return Owner.PlayerCombatState.DrawPile.Cards.Count(c=>c.Type == CardType.Power||c.Rarity==CardRarity.Rare)*2;
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
                    return Math.Min(enemyAttackDamage, n * num);
            }
            
            return 0;
        }
        
    }
}
