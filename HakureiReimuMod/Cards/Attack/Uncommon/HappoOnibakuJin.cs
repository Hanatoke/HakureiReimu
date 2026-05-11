using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class HappoOnibakuJin : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new DamageVar(6,ValueProp.Move),
                new CalculationBaseVar(0),
                new CalculationExtraVar(1),
                new CalculatedVar("CalculatedTimes").WithMultiplier((c,_)=>c.Owner.GetAllCounterCards().Count)
            ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(Counter)
        ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.ShotA;
        public HappoOnibakuJin(
            ) : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = (int)((CalculatedVar)DynamicVars["CalculatedTimes"]).Calculate(cardPlay.Target);
            await Vfx(Owner.Creature, cardPlay.Target, n);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitCount(n)
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(2);
        }

        protected static readonly int InnerCount = 8;
        protected static readonly int OutCount = 8;
        protected static readonly (Vector2, Color,float)[] Inner = [
            (new Vector2(-75,-150),new Color("ffff88"),1),
            (new Vector2(-40,-200),new Color("f80000"),1),
            (new Vector2(0,-250),new Color("ffff88"),1),
            (new Vector2(0,-150),new Color("f80000"),1),
            (new Vector2(40,-200),new Color("f80000"),1),
            (new Vector2(75,-150),new Color("ffff88"),1),
        ];

        protected static readonly (Vector2, Color, float) InnerDanmaku = (new Vector2(0, -175), new Color("f80000"),0.8f);
        protected static readonly (Vector2, Color,float)[] Out = [
            (new Vector2(0,-325),new Color("f80000"),1),
            (new Vector2(-80,-325),new Color("ffff88"),1),
            (new Vector2(-150,-400),new Color("ffff88"),1),
            (new Vector2(-75,-400),new Color("f80000"),1),
            (new Vector2(0,-400),new Color("ffff88"),1),
            (new Vector2(75,-400),new Color("f80000"),1),
            (new Vector2(80,-325),new Color("ffff88"),1),
            (new Vector2(150,-400),new Color("ffff88"),1),
        ];
        protected static readonly (Vector2, Color, float) OutDanmaku = (new Vector2(0,-400),new Color("ffff88"),1f);

        protected static readonly string Path = "bullet_yellow.tscn".ScenePath();
        public async Task Vfx(Creature source, Creature target, int count)
        {
            NCreature s = source.GetCreatureNode();
            NCreature t = target.GetCreatureNode();
            Control container = target.GetVfxContainer();
            if (s==null||t == null || container == null) return;
            float rotate = 360f / InnerCount;
            Vector2 sourcePosition = s.VfxSpawnPosition;
            Vector2 targetPosition = t.VfxSpawnPosition;
            List<(List<FlyingVFX>,FlyingVFX)> list = [];
            //内环
            for (int i = 0; i < Math.Min(count,InnerCount); i++)
            {
                float a = i * rotate + (rotate / 2);
                List<FlyingVFX> vfxs = [];
                foreach (var value in Inner)
                {
                    CreateSign(value, vfxs,a);
                }
                list.Add((vfxs,CreateDanmaku(InnerDanmaku,a)));
                await Cmd.CustomScaledWait(0.05f,0.1f);
            }
            //外环
            rotate = 360f / OutCount;
            for (var i = InnerCount; i < Math.Min(count,InnerCount+OutCount); i++)
            {
                float a = i * rotate - (rotate / 2);
                List<FlyingVFX> vfxs = [];
                foreach (var value in Out)
                {
                    CreateSign(value, vfxs,a);
                }
                list.Add((vfxs,CreateDanmaku(OutDanmaku,a)));
                await Cmd.CustomScaledWait(0.05f,0.1f);
            }

            void CreateSign((Vector2, Color, float) value, List<FlyingVFX> vfxs, float angle)
            {
                FlyingVFX vfx = FlyingVFX.Create(null);
                vfx.Modulate = new Color(1, 1, 1, 0);
                vfx.Duration = 10;
                Sprite2D sprite2D = PreloadManager.Cache.GetScene(Path).Instantiate<Sprite2D>();
                sprite2D.Modulate = value.Item2;
                sprite2D.Scale = Vector2.One*value.Item3;
                vfx.AddChildSafely(sprite2D);
                container.AddChildSafely(vfx);
                vfx.GlobalPosition = sourcePosition + value.Item1.Rotated(Mathf.DegToRad(angle));
                vfx.GlobalRotationDegrees = angle;
                vfxs.Add(vfx);
                vfx.CreateTween().TweenProperty(vfx, "modulate:a", 1, 0.25f).SetEase(Tween.EaseType.InOut);
            }
            FlyingVFX CreateDanmaku((Vector2, Color, float) value,float angle)
            {
                FlyingVFX vfx = FlyingVFX.Create(null);
                vfx.Modulate = new Color(1, 1, 1, 0);
                vfx.Duration = 10;
                vfx.OnHit = () =>
                {
                    NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                    FlyingVFXCmd.AddVFXOnTarget(NDanmakuImpact.Create(value.Item3, value.Item2), vfx.GlobalPosition);
                };
                NDanmaku danmaku = NDanmaku.Create(value.Item3,value.Item2);
                vfx.AddChildSafely(danmaku);
                container.AddChildSafely(vfx);
                vfx.GlobalPosition = sourcePosition + value.Item1.Rotated(Mathf.DegToRad(angle));
                vfx.GlobalRotationDegrees = angle;
                vfx.CreateTween().TweenProperty(vfx, "modulate:a", 1, 0.25f).SetEase(Tween.EaseType.InOut);
                return vfx;
            }
            await Cmd.Wait(0.25f);
            //发射
            Task first = null;
            _ = Run();
            async Task Run()
            {
                foreach (var tuplese in list)
                {
                    FlyingVFX vfx = tuplese.Item2;
                    first ??= vfx.HitTask;
                    Vector2 v=(targetPosition-vfx.GlobalPosition).Normalized()*2000;
                    vfx.Reset(new SteeringMover(vfx.GlobalPosition,targetPosition,v,720,8000),3);
                    await Cmd.CustomScaledWait(0.1f,0.2f);
                    foreach (var f in tuplese.Item1)
                    {
                        f.Duration = 0;
                    }
                }
            }
            
            if (first!=null)
            {
                await first;
            }
        }
    }
}
