using System;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HakureiReimu.HakureiReimuMod.Node
{
	[GlobalClass]
	public partial class HakureiReimuVisuals :NCreatureVisuals
	{
		public AnimationTree AnimationTree{get;private set;}
		public AnimationNodeStateMachinePlayback Playback{get;private set;}

		public override void _Ready()
		{
			base._Ready();
			AnimationTree = GetCurrentBody().GetNode<AnimationTree>("AnimationTree");
			Playback = (AnimationNodeStateMachinePlayback)AnimationTree?.Get("parameters/playback");

			SpireField<Godot.Node, Func<string[], bool?>> spireField =
				(SpireField<Godot.Node, Func<string[], bool?>>)AccessTools
					.Field(typeof(CustomAnimation), "_animHandler").GetValue(null);
			spireField[this] = (_ => null);
		}
	}
}
