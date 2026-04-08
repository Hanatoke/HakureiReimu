using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HakureiReimu.HakureiReimuMod.Node
{
    public partial class NHakureiReimuEnergyCounter :NEnergyCounter
    {
        public AnimationPlayer AnimationPlayer;
        public override void _Ready()
        {
            this.AnimationPlayer = this.GetNode<AnimationPlayer>("%AnimationPlayer");
            base._Ready();
        }
        public override void _EnterTree()
        {
            base._EnterTree();
            Player player = AccessTools.Field(typeof(NEnergyCounter),"_player").GetValue(this) as Player;
            if (player != null)player.PlayerCombatState.EnergyChanged += this.OnEnergyChangedNew;
        }

        public void OnEnergyChangedNew(int oldEnergy, int newEnergy)
        {
            newEnergy=Math.Clamp(newEnergy,0,100);
            AnimationPlayer.SpeedScale = newEnergy>0?newEnergy*0.5f:0.25f;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Player player = AccessTools.Field(typeof(NEnergyCounter),"_player").GetValue(this) as Player;
            if (player != null)player.PlayerCombatState.EnergyChanged -= this.OnEnergyChangedNew;
        }
    }
}