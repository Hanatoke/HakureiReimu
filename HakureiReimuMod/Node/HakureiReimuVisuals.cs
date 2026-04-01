using Godot;
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
			AnimationTree = Body.GetNode<AnimationTree>("AnimationTree");
			Playback = (AnimationNodeStateMachinePlayback)AnimationTree?.Get("parameters/playback");
		}
	}
}
