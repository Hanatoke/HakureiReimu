using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class PowerHelper
    {
        public static readonly HashSet<Type> Mutually = [
            typeof(ArtifactPower),
            typeof(BarricadePower),
            typeof(BufferPower),
            typeof(CurlUpPower),
            typeof(HighVoltagePower),
            typeof(IntangiblePower),
            typeof(PlatingPower),
            typeof(RegenPower),
            typeof(RitualPower),
            typeof(ThornsPower),
            typeof(VigorPower),
            typeof(StrengthPower),
            typeof(DexterityPower),
        ];
        public static readonly HashSet<Type> CanModify = [
            typeof(EnragePower),
            typeof(FlutterPower),
            typeof(HardenedShellPower),
            typeof(HardToKillPower),
            // typeof(SandpitPower),
            typeof(EscapeArtistPower),
            typeof(PainfulStabsPower),
            typeof(PaperCutsPower),
            typeof(PersonalHivePower),
            typeof(RavenousPower),
            typeof(SkittishPower),
            typeof(SlipperyPower),
            typeof(SlowPower),
            typeof(SuckPower),
            typeof(TerritorialPower),
            typeof(ThieveryPower),
        ];
        public static readonly HashSet<Type> LessIsMore = [
            typeof(EscapeArtistPower),
            typeof(HardenedShellPower),
            typeof(HardToKillPower),
            // typeof(SandpitPower),
        ];
        public static readonly HashSet<Type> DontBlock = [
            typeof(FlutterPower),
            typeof(EscapeArtistPower),
            typeof(SandpitPower),
            typeof(SwipePower),
            typeof(MinionPower),
            typeof(BurrowedPower)
        ];

        public static HashSet<Type> Add(this HashSet<Type> a, HashSet<Type> b)
        {
            HashSet<Type> result = new(a);
            result.UnionWith(b);
            return result;
        }
        public static HashSet<Type> Sub(this HashSet<Type> a, HashSet<Type> b)
        {
            HashSet<Type> result = new(a);
            result.ExceptWith(b);
            return result;
        }
        public static HashSet<Type> And(this HashSet<Type> a, HashSet<Type> b)
        {
            HashSet<Type> result = new(a);
            result.IntersectWith(b);
            return result;
        }
    }
}