using System;
using System.Linq;
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
		public GpuParticles2D ColorCore;
		public GpuParticles2D WhiteCore;
		public Trail TrailOuter;
		public Trail TrailInner;
		protected bool IsDuplicated;

		public static NDanmaku Create(float scale=1f,Color? color=null,int? trailLength=null,bool glow=true)
		{
			NDanmaku d = PreloadManager.Cache.GetScene(Path).Instantiate<NDanmaku>();
			color ??= Color.FromHsv((float)GD.RandRange(0, 1f), 1, 1);
			if (Math.Abs(scale - 1) > 0.001f)
			{
				d.SetScale(scale);
			}
			d.SetColor(color.Value);
			d.SetTrailLength(trailLength??30);
			if (!glow)d.SetGlow(false);
			return d;
		}

		public override void _Ready()
		{
			Visual = GetNode<Node2D>("Visual");
			ColorAble = Visual.GetNode<Node2D>("ColorAble");
			Fixed = Visual.GetNode<Node2D>("Fixed");
			ColorCore = ColorAble.GetNode<GpuParticles2D>("Core");
			WhiteCore = Fixed.GetNode<GpuParticles2D>("Core");
			Trails=ColorAble.GetNode<Node2D>("Trails");
			TrailOuter = Trails.GetNode<Trail>("TrailOuter");
			TrailInner = Trails.GetNode<Trail>("TrailInner");
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
			if (ColorAble==null||Fixed==null)
			{
				_Ready();
			}
			this.Scale = Vector2.One * scale;
			if (!IsDuplicated)
			{
				IsDuplicated=true;
				ColorCore.ProcessMaterial = (ParticleProcessMaterial)ColorCore.ProcessMaterial.Duplicate();
				WhiteCore.ProcessMaterial = (ParticleProcessMaterial)WhiteCore.ProcessMaterial.Duplicate();
			}
			((ParticleProcessMaterial)ColorCore.ProcessMaterial).Scale=this.Scale;
			((ParticleProcessMaterial)WhiteCore.ProcessMaterial).Scale=this.Scale;
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
		public void SetGlow(bool glow)
		{
			if (ColorAble == null || Fixed == null)
			{
				_Ready();
			}

			foreach (GpuParticles2D p in ColorAble.GetChildren().OfType<GpuParticles2D>())
			{
				if (p!=ColorCore)
				{
					p.Visible=glow;
				}
			}
			foreach (GpuParticles2D p in Fixed.GetChildren().OfType<GpuParticles2D>())
			{
				if (p!=WhiteCore)
				{
					p.Visible=glow;
				}
			}
			
		}
	}
}
