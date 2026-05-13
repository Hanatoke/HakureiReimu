using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Hooks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class DreamInnate : EffectFollowCard<NDanmaku>,IHealAmountModifier {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [All,Immediate,CardKeyword.Exhaust];

        public DreamInnate(
            ) : base(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }
        public override int Duration => 0;
        public override AbstractPersistCardSlot InstanceSlot => Slot = new NoCountCardSlot(this);

        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.All;
        public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
        {
            if (side==Owner.Creature.Side&&InPersisting&&Slot!=null)
            {
                await PersistCardCmd.StopPersistCard(Slot);
            }
        }
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            RunAnimation(Character.HakureiReimu.Animation.Guard);
            _ = EffectFlash();
            await Flash(instant);
        }

        public override async Task InvokeCounter(Creature target, CounterType byType)
        {
            if (!IsInCombat)
            {
                HakureiReimuMain.Logger.Warn("尝试发动不在战斗中的反制卡? "+this.GetType().Name);
                return;
            }
            if (CounterManager.InInvokeCounter)return;
            if (CounterManager.InMonsterMove)
            {
                CounterManager.AddToLater(this,async  () => await CounterCmd.InvokeCounter(CombatState,this,target));
            }
            else
            {
                await CounterCmd.InvokeCounter(CombatState, this, target);
            }
        }

        public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
        {
            if (InPersisting&&CheckAttack(attack))
            {
                return -999999999;
            }
            return hitCount;
        }
        public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel cardSource,
            CardPlay cardPlay)
        {
            if (InPersisting&&target is {IsMonster:true,Side:CombatSide.Enemy})
            {
                return 0;
            }
            return 1;
        }
        public override async Task AfterModifyingBlockAmount(decimal modifiedAmount, CardModel cardSource, CardPlay cardPlay)
        {
            await InvokeCounter(null, CounterType.Buff);
        }

        // public override decimal ModifyHealAmount(Creature creature, decimal amount)
        // {
        //     if (InPersisting&&creature is {IsMonster:true})
        //     {
        //         return creature.IsDead ? 1 : 0;
        //     }
        //     return amount;
        // }
        public decimal ModifyHealMultiplicative(Creature creature, decimal amount)
        {
            if (InPersisting && creature is { IsMonster: true,Side:CombatSide.Enemy})
            {
                if (creature.IsDead)
                {
                    Flash();
                    CreatureCmd.Kill(creature);
                }
                return 0;
            }
            return 1;
        }

        public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature applier,
            out decimal modifiedAmount)
        {
            if (InPersisting&&CheckPower(canonicalPower,amount,applier,target,out CounterType _)&&!PowerHelper.DontBlock.Contains(canonicalPower.GetType()))
            {
                modifiedAmount = 0;
                return true;
            }
            modifiedAmount = amount;
            return false;
        }

        public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
        {
            await InvokeCounter(null, CounterType.All);
        }

        public override async Task AfterCardGeneratedForCombat(CardModel card, Player creator)
        {
            if (InPersisting&&creator==null&&card.Type is CardType.Curse or CardType.Status)
            {
                await InvokeCounter(null, CounterType.Debuff);
                if (card.Pile is not { IsCombatPile: true })return;
                await CardPileCmd.RemoveFromCombat(card);
            }
        }
        //-----------------------------------------------------------------------
        protected override NDanmaku CreateEffectInstance => null;
        protected static readonly float Radius = 200;
        protected static readonly float RotateSpeed = 60;
        public override void CreateEffect(int amount)
        {
            if (FollowDanmakuManager.DanmakuNodes.Any(k =>
                    k.Key.Owner == Owner && k.Key is DreamInnate && k.Value.Count > 0)) return;
            const int count = 7;
            CanvasItemMaterial material = new ();
            material.BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
            for (var i = 0; i < count; i++)
            {
                float weight = (float)i / (count);
                Color c = Color.FromHsv(weight, 1, 1);
                NDanmaku danmaku = NDanmaku.Create(1.5f,c,1,false);
                Node2D node2D = PreloadManager.Cache.GetScene(YinYangOrb.ScenePath).Instantiate<Node2D>();
                danmaku.AddChildSafely(node2D);
                node2D.Scale = Vector2.One * 1.5f;
                node2D.Modulate = c;
                node2D.Material = material;
                FollowVFX vfx = this.AddFollow(danmaku,Radius,Radius,RotateSpeed,0,true);
                vfx.Revolution = Mathf.Lerp(0, 360f, weight);
                vfx.OrbitalRotation = 0;
                vfx.SelfRotationSpeed = RotateSpeed*2;
            }
        }

        public async Task EffectFlash()
        {
            List<Node2D> list = this.GetFollows();
            foreach (Node2D node2D in list)
            {
                if (node2D is FollowVFX vfx)
                {
                    if (Math.Abs(vfx.A - Radius) > 5||Math.Abs(vfx.B - Radius) > 5)return;
                    vfx.AtStart = true;
                    vfx.A = Radius * 0.5f;
                    vfx.B = Radius * 0.5f;
                    vfx.RevolutionSpeed = RotateSpeed *1.5f;
                }
            }
            await Cmd.CustomScaledWait(0.5f, 0.75f);
            foreach (Node2D node2D in list)
            {
                if (node2D is FollowVFX vfx)
                {
                    vfx.AtStart = true;
                    vfx.A = Radius;
                    vfx.B = Radius;
                    vfx.RevolutionSpeed = RotateSpeed;
                }
            }
        }

        public override void ClearEffects()
        {
            List<Node2D> list = this.GetFollows();
            foreach (Node2D node2D in list)
            {
                if (node2D is FollowVFX vfx)
                {
                    vfx.AtStart = true;
                    vfx.A = 0;
                    vfx.B = 0;
                }
                node2D.GetTree().CreateTimer(0.5f).Connect(SceneTreeTimer.SignalName.Timeout,
                    Callable.From(node2D.QueueFreeSafely));
            }
            list.Clear();
        }

        public override Task AfterModifyPersistCount(AbstractPersistCardSlot slot, int result)
        {
            return Task.CompletedTask;
        }
    }
}
