using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    [GlobalClass]
    public partial class NNeedle :Node2D
    {
        public static readonly string Path = "needle.tscn".ScenePath();
        public Node2D Visual;
        public Node2D BackBufferCopy;
        public GpuParticles2D Wave;

        public static NNeedle Create(float scale=1)
        {
            NNeedle needle=PreloadManager.Cache.GetScene(Path).Instantiate<NNeedle>();
            needle.Scale=Vector2.One * scale;
            return needle;
        }

        public override void _Ready()
        {
            Visual = GetNode<Node2D>("Visual");
            BackBufferCopy = GetNode<Node2D>("BackBufferCopy");
            Wave = BackBufferCopy.GetNode<GpuParticles2D>("Wave");
        }

        public override void _Process(double delta)
        {
            // ParticleProcessMaterial material = Wave.ProcessMaterial as ParticleProcessMaterial;
            // material.Scale = GlobalScale;
            ShaderMaterial shader=Wave.Material as ShaderMaterial;
            Vector2 dir = Vector2.Right.Rotated(GlobalRotation);
            shader.SetShaderParameter("direction", dir);
        }

        public void OnHit()
        {
            if (!IsInstanceValid(this))return;
            Visual.Visible = false;
            Wave.Emitting = false;
        }
    }
}