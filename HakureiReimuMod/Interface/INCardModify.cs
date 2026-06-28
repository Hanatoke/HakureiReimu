using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HakureiReimu.HakureiReimuMod.Interface
{
    public interface INCardModify
    {
        void OnReload(NCard card, List<Action<NCard>> recoverAction, List<Godot.Node> needRemove);
    }
}