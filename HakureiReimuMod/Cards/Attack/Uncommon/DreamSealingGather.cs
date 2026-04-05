using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class DreamSealingGather : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6,ValueProp.Move),new RepeatVar(3)];
        
        public DreamSealingGather(
            ) : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            NCreature player = NCombatRoom.Instance?.GetCreatureNode(Owner.Creature);
            NCreature target = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
            if (player!=null&&target!=null)
            {
                for (var i = 0; i < DynamicVars.Repeat.IntValue; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        Vector3 v = new Vector3(
                            (float)GD.RandRange(0, 1),
                            (float)GD.RandRange(-0.5, 1),
                            (float)GD.RandRange(-0.5,0.5)
                        ).Normalized() * 1000;
                        if (player.VfxSpawnPosition.X<target.VfxSpawnPosition.X)
                        {
                            v.X *= -1;
                        }
                        FlyingVFX vfx=FlyingVFX.Create(new Steering3DMover(player.VfxSpawnPosition,target.VfxSpawnPosition,v,turnSpeed:10,acceleration:2000));
                        NDanmaku danmaku = PreloadManager.Cache.GetScene("res://HakureiReimu/scenes/danmaku.tscn")
                            .Instantiate<NDanmaku>();
                        vfx.AddChildSafely(danmaku);
                        danmaku.SetColor(Color.FromHsv((float)GD.RandRange(0,1f),1,1));
                        danmaku.SetScale(1f);
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                        await Cmd.Wait(0.1f);
                    }
                }
            }
            

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Repeat.IntValue)
                .FromCard(this).Targeting(cardPlay.Target)
                .Execute(choiceContext);
            EnergyCost.AddThisCombat(-1);
        }
        protected override void OnUpgrade() {
            DynamicVars.Repeat.UpgradeValueBy(1);
        }
    }
}
