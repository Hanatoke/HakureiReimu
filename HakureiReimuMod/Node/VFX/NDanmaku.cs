using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
	[GlobalClass]
	public partial class NDanmaku :Node2D
	{
		public Node2D Visual;
		public Node2D ColorAble;
		public Node2D Fixed;
		public GpuParticles2D ColorTrails;
		public GpuParticles2D WhiteTrails;

		public override void _Ready()
		{
			Visual = GetNode<Node2D>("Visual");
			ColorAble = Visual.GetNode<Node2D>("ColorAble");
			Fixed = Visual.GetNode<Node2D>("Fixed");
			ColorTrails = ColorAble.GetNode<GpuParticles2D>("Trails");
			WhiteTrails = Fixed.GetNode<GpuParticles2D>("Trails");
		}


		public void SetColor(Color color)
		{
			if (Visual==null)
			{
				_Ready();
			}
			ColorAble.Modulate = color;
		}
		public void SetScale(float scale)
		{
			if (Visual==null)
			{
				_Ready();
			}
			this.Scale=Vector2.One * scale;
			ParticleProcessMaterial material = ColorTrails.ProcessMaterial as ParticleProcessMaterial;
			material.Scale=Vector2.One * scale;
			material = WhiteTrails.ProcessMaterial as ParticleProcessMaterial;
			material.Scale=Vector2.One * scale;
		}
	}
}
