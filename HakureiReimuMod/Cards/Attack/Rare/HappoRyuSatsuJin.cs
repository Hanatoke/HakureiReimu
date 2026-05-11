using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class HappoRyuSatsuJin : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(8, ValueProp.Move)
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromKeyword(Counter),
        ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.AttackCloseRound;
        public HappoRyuSatsuJin(
            ) : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = CombatState.IterateHookListeners()
                .Where(a => CanInvoke(a, out ICounter _)).OfType<CardModel>().Where(c => c.Type == CardType.Attack).Count();
            var list=await Vfx(Owner.Creature, cardPlay.Target, n);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            foreach (AbstractModel abstractModel in CombatState.IterateHookListeners())
            {
                if (CanInvoke(abstractModel, out ICounter counter))
                {
                    await CounterCmd.InvokeCounter(CombatState, counter, cardPlay.Target, true, true);
                }
            }
            _ = Launch(list, cardPlay.Target);
        }

        public virtual bool CanInvoke(AbstractModel abstractModel, out ICounter counter)
        {
            if (abstractModel is ICounter{IsCounterEnable:true} c&&c.CounterOwner==Owner.Creature)
            {
                counter = c;
                return true;
            }
            counter = null;
            return false;
        }

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(4);
        }

        protected static readonly float TriangleHeight = 400;
        protected static readonly int TriangleCount = 2;
        protected static readonly (Vector2, Color, float)[] Triangle = [
            (((new Vector2(0,-TriangleHeight).Rotated(Mathf.DegToRad(-120))+new Vector2(0,-TriangleHeight))/2),new Color("ffff88"),1f),
            (new Vector2(0,-TriangleHeight),new Color("f80000"),1f),
            (((new Vector2(0,-TriangleHeight).Rotated(Mathf.DegToRad(120))+new Vector2(0,-TriangleHeight))/2),new Color("ffff88"),1f),
        ];
        protected static readonly int OutCount = 5;
        protected static readonly string Path = "bullet_yellow.tscn".ScenePath();

        protected async Task<List<List<FlyingVFX>>> Vfx(Creature source, Creature target, int count)
        {
            NCreature s = source.GetCreatureNode();
            Control container = source.GetVfxContainer();
            if (s == null || container == null) return [];
            const float span = 50;
            const float rotate = 60;
            Vector2 sourcePosition = s.VfxSpawnPosition;
            List<List<FlyingVFX>> list = [];
            //内部
            if (count>0)
            {
                var value = Triangle[0];
                list.Add(await Circle(value.Item1.Length()*0.9f,new Color("f80000"),value.Item3));
                await Cmd.CustomScaledWait(0.1f,0.2f);
                count--;
            }
            //三角
            for (int i = 0; i < Math.Min(count,TriangleCount); i++)
            {
                List<FlyingVFX> vfxs = [];
                for (var j = 0; j < 3; j++)
                {
                    float a = i * rotate + (rotate / 2) + j * 120f;
                    for (var n = 0; n < Triangle.Length-1; n++)
                    {
                        var left = Triangle[n];
                        var right = Triangle[n+1];
                        float length=left.Item1.DistanceTo(right.Item1);
                        int num = Mathf.FloorToInt(length / span);
                        for (var k = 0; k < num; k++)
                        {
                            float weight = (float)k / (num-1);
                            Create(left.Item1.Lerp(right.Item1, weight), left.Item2.Lerp(right.Item2, weight),
                                Mathf.Lerp(left.Item3, right.Item3, weight), vfxs, a);
                        }
                    }
                }
                list.Add(vfxs);
                await Cmd.CustomScaledWait(0.1f,0.2f);
            }
            count -= TriangleCount;
            //外部
            
            for (var i = 0; i < Math.Min(count,OutCount); i++)
            {
                var value = Triangle[1];
                float r = value.Item1.Length() * 1.1f+i*span;
                float weight = (float)i / (OutCount - 1);
                Color color = Color.FromHsv((value.Item2.H + Mathf.Lerp(0, 1, weight)) % 1, value.Item2.S,
                    value.Item2.V);
                list.Add(await Circle(r,color,value.Item3,1));
                await Cmd.CustomScaledWait(0.1f,0.2f);
            }
            return list;
            void Create(Vector2 offset, Color color, float scale, List<FlyingVFX> vfxs, float angle)
            {
                FlyingVFX vfx = FlyingVFX.Create(null);
                vfx.Modulate = new Color(1, 1, 1, 0);
                vfx.Duration = 10;
                Sprite2D sprite2D = PreloadManager.Cache.GetScene(Path).Instantiate<Sprite2D>();
                sprite2D.Modulate = color;
                sprite2D.Scale = Vector2.One*scale;
                vfx.AddChildSafely(sprite2D);
                container.AddChildSafely(vfx);
                vfx.GlobalPosition = sourcePosition + offset.Rotated(Mathf.DegToRad(angle));
                vfx.GlobalRotationDegrees = angle;
                vfxs.Add(vfx);
                vfx.CreateTween().TweenProperty(vfx, "modulate:a", 1, 0.25f).SetEase(Tween.EaseType.InOut);
            }

            async Task<List<FlyingVFX>> Circle(float radius, Color color, float scale, float density=1)
            {
                float per = Mathf.Pi * 2 * radius;
                int num = Mathf.FloorToInt(per / span * density);
                List<FlyingVFX> vfxs = [];
                for (var i = 0; i < num; i++)
                {
                    float weight = (float)i / (num-1);
                    float a = Mathf.Lerp(0, 360, weight);
                    Create(new Vector2(0, -radius), color, scale, vfxs, a);
                }
                return vfxs;
            }
        }

        protected async Task Launch(List<List<FlyingVFX>> list, Creature target)
        {
            NCreature t = target.GetCreatureNode();
            Control container = target.GetVfxContainer();
            if (t == null || container == null)
            {
                list.SelectMany(r=>r).ToList().ForEach(v=>v.QueueFreeSafely());
                return;
            }
            foreach (List<FlyingVFX> tuplese in list)
            {
                foreach (FlyingVFX vfx in tuplese)
                {
                    vfx.Duration = 0;
                }
                await Cmd.CustomScaledWait(0.1f,0.2f);
            }
        }
    }
}
