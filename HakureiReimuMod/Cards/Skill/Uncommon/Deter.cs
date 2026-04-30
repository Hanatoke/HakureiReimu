using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class Deter : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => 
            [
                HoverTipFactory.FromPower<WeakPower>(),HoverTipFactory.FromPower<VulnerablePower>(),
                HoverTipFactory.Static(StaticHoverTip.Stun)
            ];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<WeakPower>(1)];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public Deter(
            ) : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies) {
        }

        private static List<LocString> _talk;
        public List<LocString> Talk
        {
            get {
                if (_talk!=null)return _talk;
                List<LocString> s = [];
                int i = 1;
                while (LocString.GetIfExists("cards",this.Id.Entry+".talk."+i) is { } e)
                {
                    s.Add(e);
                    i++;
                }
                return _talk=s;
            }
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_scream");
            foreach (Creature enemy in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<WeakPower>(enemy, DynamicVars.Weak.IntValue, Owner.Creature, this);
                await PowerCmd.Apply<VulnerablePower>(enemy, DynamicVars.Weak.IntValue, Owner.Creature, this);
                if (enemy.Powers.Any(p=>p is MinionPower))
                {
                    await CreatureCmd.Stun(enemy);
                }
                if (Talk.Count>0)
                {
                    TalkCmd.Play(Talk[GD.RandRange(0,Talk.Count-1)],enemy,vfxColor:VfxColor.White);
                }
            }
        }
        protected override void OnUpgrade() {
            AddKeyword(CardKeyword.Innate);
        }
    }
}
