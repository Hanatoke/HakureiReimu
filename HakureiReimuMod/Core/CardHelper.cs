using System;
using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class CardHelper
    {
        private static readonly MethodInfo DrawMethod = typeof(CardPileCmd).GetMethod(nameof(CardPileCmd.Draw),
            types: [typeof(PlayerChoiceContext), typeof(decimal), typeof(Player), typeof(bool)]);
        private static readonly Dictionary<Type, bool> DrawCards = new();
        public static bool IsDrawCard(this CardModel card)
        {
            if (DrawCards.TryGetValue(card.GetType(), out bool result))
            {
                return result;
            }
            bool f=false;
            MethodInfo info = card.GetType().GetMethod("OnPlay",BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                f = info.HasCall(DrawMethod);
            }
            DrawCards[card.GetType()] = f;
            return f;
        }
        //-------------------------------------------------------------------------------------------------------------
        private static readonly MethodInfo GainEnergyMethod = typeof(PlayerCmd).GetMethod(nameof(PlayerCmd.GainEnergy)
            ,BindingFlags.Static|BindingFlags.Public);
        private static readonly Dictionary<Type, bool> GainEnergyCards = new();
        public static bool IsGainEnergyCard(this CardModel card)
        {
            if (GainEnergyCards.TryGetValue(card.GetType(), out bool result))
            {
                return result;
            }
            bool f=false;
            MethodInfo info = card.GetType().GetMethod("OnPlay",BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                f = info.HasCall(GainEnergyMethod);
            }
            GainEnergyCards[card.GetType()] = f;
            return f;
        }
        //-------------------------------------------------------------------
        private static readonly MethodInfo ExhaustMethod =
            typeof(CardCmd).GetMethod(nameof(CardCmd.Exhaust), BindingFlags.Static | BindingFlags.Public);
        private static readonly Dictionary<Type, bool> ExhaustCards = new();
        public static bool IsExhaustCard(this CardModel card)
        {
            if (ExhaustCards.TryGetValue(card.GetType(), out bool result))
            {
                return result;
            }
            bool f=false;
            MethodInfo info = card.GetType().GetMethod("OnPlay",BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                f = info.HasCall(ExhaustMethod);
            }
            ExhaustCards[card.GetType()] = f;
            return f;
        }
        //---------------------------------------------------------------------------------
        private static readonly MethodInfo DiscardMethod =
            typeof(CardCmd).GetMethod(nameof(CardCmd.Discard), BindingFlags.Static | BindingFlags.Public,
                [typeof(PlayerChoiceContext), typeof(CardModel)]);
        private static readonly Dictionary<Type, bool> DiscardCards = new();
        public static bool IsDiscardCard(this CardModel card)
        {
            if (DiscardCards.TryGetValue(card.GetType(), out bool result))
            {
                return result;
            }
            bool f=false;
            MethodInfo info = card.GetType().GetMethod("OnPlay",BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                f = info.HasCall(DiscardMethod);
            }
            DiscardCards[card.GetType()] = f;
            return f;
        }
        static CardHelper()
        {
            if (DrawMethod == null) HakureiReimuMain.Logger.Error(nameof(DrawMethod) + " is not find");
            if (GainEnergyMethod == null)HakureiReimuMain.Logger.Error(nameof(GainEnergyMethod) + " is not find");
            if (ExhaustMethod == null)HakureiReimuMain.Logger.Error(nameof(ExhaustMethod) + " is not find");
            if (DiscardMethod == null)HakureiReimuMain.Logger.Error(nameof(DiscardMethod) + " is not find");
        }
    }
}