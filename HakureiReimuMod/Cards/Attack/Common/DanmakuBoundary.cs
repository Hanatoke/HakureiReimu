using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class DanmakuBoundary : EffectFollowCard<NDanmaku> {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CounterVar(2),new DamageVar(6,ValueProp.Move)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack,Immediate,CardKeyword.Exhaust];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellFastA;

        public DanmakuBoundary(
            ) : base(1, CardType.Attack, CardRarity.Token, TargetType.AllEnemies) {
        }

        public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count,CombatState state)
        {
            if (count == 0) return [];
            if (CombatManager.Instance.IsOverOrEnding) return [];
            List<CardModel> cards = new();
            for (var i = 0; i < count; i++)
            {
                cards.Add(state.CreateCard<DanmakuBoundary>(owner));
            }
            await CardPileCmd.AddGeneratedCardsToCombat(cards,PileType.Hand,true);
            return cards;
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }

        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.Attack;
        protected override NDanmaku CreateInstance => NDanmaku.Create((float)GD.RandRange(0.4, 0.5), glow: false);
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            if (target is not { IsHittable: true }) return;
            RunAnimation(Character.HakureiReimu.Animation.ShotA);
            await Flash(instant);
            if (UseEffect() is {} danmaku)
            {
                (Vector2,float,Color) set=(danmaku.GlobalPosition,danmaku.Scale.X,danmaku.ColorAble.Modulate);
                danmaku.QueueFreeSafely();
                FlyingVFXCmd.DanmakuLineToTarget(set.Item1, target, scale:set.Item2, duration: 0.3f, color: set.Item3);
                if (!instant)
                {
                    await Cmd.Wait(ActivateThisTurn switch
                    {
                        1=>0.3f,
                        2=>0.2f,
                        3=>0.1f,
                        _=>0,
                    });
                }
                if (!cost)CreateEffect();
            }
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
