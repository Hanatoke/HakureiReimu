using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class YinYangScattering : AbstractCard,IYinYangOrbListener
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar(8, ValueProp.Move),
        ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.ShotB;
        public YinYangScattering(
            ) : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            VFX(Owner.Creature,CombatState.HittableEnemies);
            await Cmd.Wait(0.5f);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState)
                .WithHitFx(VfxCmd.bluntPath)
                .Execute(choiceContext);
        }

        // protected override PileType GetResultPileType()
        // {
        //     PileType pileType = base.GetResultPileType();
        //     if (pileType==PileType.Discard&&ReturnToHand)
        //     {
        //         ReturnToHand = false;
        //         pileType = PileType.Hand;
        //     }
        //     return pileType;
        // }

        public async Task AfterEvokeOrb(PlayerChoiceContext choiceContext, YinYangOrb orb, Player player, Creature target,
            CardModel cardSource,IEnumerable<DamageResult> results)
        {
            if (cardSource==this)
            {
                foreach (DamageResult result in results)
                {
                    if (result.TotalDamage>0)
                    {
                        DamageVar damageVar = DynamicVars.Damage;
                        damageVar.BaseValue += result.TotalDamage;
                    }
                }

                if (Pile is not {Type:PileType.Hand})
                {
                    await CardPileCmd.Add(this, PileType.Hand);
                }
            }
        }

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }

        public void VFX(Creature source,IEnumerable<Creature> targets)
        {
            NCreature s=NCombatRoom.Instance?.GetCreatureNode(source);
            if (s==null)return;
            float height = -NCombatRoom.Instance.CombatVfxContainer.GetViewportRect().Size.Y;
            foreach (Creature target in targets)
            {
                NCreature t=NCombatRoom.Instance?.GetCreatureNode(target);
                if (t==null)continue;
                _ = VFXSingle(s,t,height);
            }
        }

        public async Task VFXSingle(NCreature source, NCreature target,float heightOffset)
        {
            int n = GD.RandRange(3, 6);
            for (var i = 0; i < n; i++)
            {
                Vector2 offsetSource = new((float)GD.RandRange(-200f, 200f), heightOffset);
                Vector2 offsetTarget =new((float)GD.RandRange(-100f, 100f), 0);
                float scale = (float)GD.RandRange(1.5, 2);
                FlyingVFX vfx = FlyingVFX.Create(new LineMover(source.VfxSpawnPosition + offsetSource,
                    target.GlobalPosition + offsetTarget));
                vfx.AddChildSafely(NYinYangOrbFlying.Create(scale));
                vfx.Duration = 0.5f;
                vfx.OnHit = () =>
                {
                    NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                    FlyingVFXCmd.AddVFXOnTarget(NMissileImpact.Create(scale*0.25f),vfx.GlobalPosition);
                };
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                await Cmd.Wait((float)GD.RandRange(0.05f, 0.25f));
            }
        }
    }
}
