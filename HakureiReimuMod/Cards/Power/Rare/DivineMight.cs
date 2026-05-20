using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Rare {
    public class DivineMight : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<ArtifactPower>(1)
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromPower<ArtifactPower>()
        ];
        public DivineMight(
            ) : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            foreach (PowerModel p in Owner.Creature.Powers.ToList())
            {
                if (p.TypeForCurrentAmount==PowerType.Debuff&&p.IsVisible)
                {
                    VfxPower(p,Owner.Creature);
                    await PowerCmd.Remove(p);
                    await Cmd.Wait(0.1f);
                }
            }
            await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature,
                DynamicVars[nameof(ArtifactPower)].BaseValue, Owner.Creature, this);
        }
        protected override void OnUpgrade() 
        {
            EnergyCost.UpgradeBy(-1);
        }

        public void VfxPower(PowerModel power,Creature source)
        {
            NCreature s = source.GetCreatureNode();
            Control container = source.GetVfxContainer();
            if (s==null||container==null||power==null)return;
            Vector2 offset = Vector2.Up.Rotated((float)Mathf.DegToRad(GD.RandRange(-120f,120f)))*GD.RandRange(150,250);
            Vector2 position = s.VfxSpawnPosition + offset;
            float scale = (float)GD.RandRange(0.8f, 1.2f);
            FlyingVFXCmd.AddVFXOnTarget(NDanmakuImpact.Create(scale),position, container);
            FlyingVFXCmd.AddVFXOnTarget(NShatter.Create(power.BigIcon,scale*0.5f),position, container);
        }
    }
}
