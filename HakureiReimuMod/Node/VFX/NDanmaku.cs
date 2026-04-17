using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
	[GlobalClass]
	public partial class NDanmaku :Node2D
	{
		public static readonly string Path = "danmaku.tscn".ScenePath();
		public Node2D Visual;
		public Node2D ColorAble;
		public Node2D Fixed;
		public Node2D Trails;
		public GpuParticles2D ColorTrails;
		public GpuParticles2D WhiteTrails;
		public Trail TrailOuter;
		public Trail TrailInner;

		public static NDanmaku Create(float scale=1f,Color? color=null,int? trailLength=null)
		{
			NDanmaku d = PreloadManager.Cache.GetScene(Path).Instantiate<NDanmaku>();
			color ??= Color.FromHsv((float)GD.RandRange(0, 1f), 1, 1);
			d.SetColor(color.Value);
			d.Scale=Vector2.One * scale;
			d.SetTrailLength(trailLength??30);
			return d;
		}

		public override void _Ready()
		{
			Visual = GetNode<Node2D>("Visual");
			ColorAble = Visual.GetNode<Node2D>("ColorAble");
			Fixed = Visual.GetNode<Node2D>("Fixed");
			ColorTrails = ColorAble.GetNode<GpuParticles2D>("Core");
			WhiteTrails = Fixed.GetNode<GpuParticles2D>("Core");
			Trails=ColorAble.GetNode<Node2D>("Trails");
			TrailOuter = Trails.GetNode<Trail>("TrailOuter");
			TrailInner = Trails.GetNode<Trail>("TrailInner");
		}

		public override void _Process(double delta)
		{
			ParticleProcessMaterial material = ColorTrails.ProcessMaterial as ParticleProcessMaterial;
			material.Scale=GlobalScale;
			material = WhiteTrails.ProcessMaterial as ParticleProcessMaterial;
			material.Scale=GlobalScale;
		}


		public void SetColor(Color color)
		{
			if (Visual==null)
			{
				_Ready();
			}
			ColorAble.Modulate = color;
		}

		public void SetTrailLength(int length)
		{
			if (TrailInner == null || TrailOuter == null)
			{
				_Ready();
			}
			TrailInner.MaxSegments=length;
			TrailOuter.MaxSegments=length;
		}
	}
}
