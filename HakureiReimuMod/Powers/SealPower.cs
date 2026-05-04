using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class SealPower:AbstractPower
    {
        public static readonly string ID = nameof(SealPower);
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public int NeedToCost=0;
        public static decimal MultiplayerScaling(int playerCount)=>2m/(1+playerCount);

        public override LocString Description
        {
            get
            {
                LocString locString = base.Description;
                bool isMultiplayerRun=RunManager.Instance.IsInProgress&&!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer;
                locString.Add("IsMultiplayerRun",isMultiplayerRun);
                if (isMultiplayerRun)
                {
                    int count=1;
                    if (CombatManager.Instance.DebugOnlyGetState() is {} state)
                    {
                        count = state.RunState.Players.Count(p=>p.Creature.IsAlive);
                    }
                    else if (RunManager.Instance.DebugOnlyGetState() is { } runState)
                    {
                        count = runState.Players.Count;
                    }
                    locString.Add("Scaling",(MultiplayerScaling(count)*100).ToString("F1"));
                }
                return locString;
            }
        }

        public void ModifyDamage(ref decimal amount,ValueProp props, Creature dealer, Creature target)
        {
            if (amount>0&&dealer==Owner&&target!=Owner)
            {
                decimal n=Math.Min(amount, this.Amount);
                amount=Math.Max(0,amount-n);
                NeedToCost = Math.Max(NeedToCost, (int)n);
            }
        }

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props,
            Creature target, CardModel cardSource)
        {
            if (dealer==Owner&&NeedToCost>0&&Owner.IsAlive)
            {
                int n = NeedToCost;
                NeedToCost = 0;
                Flash();
                await PowerCmd.ModifyAmount(this,-n,null,null);
                foreach (Creature p in CombatState.GetOpponentsOf(Owner).ToList())
                {
                    if (p.HasPower<KujiKoshinHoPower>())
                    {
                        if (Owner.Monster is {IsPerformingMove:true})
                        {
                            KujiKoshinHoPower power = p.GetPower<KujiKoshinHoPower>();
                            power?.TryAddToLater(Owner,n);
                        }
                        else
                        {
                            await PowerCmd.Apply<SealPower>(Owner,n, Owner, null);
                        }
                        return;
                    }
                }
            }
        }
    }
}