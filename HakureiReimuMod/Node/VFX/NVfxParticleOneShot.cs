using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class NVfxParticleOneShot :Node2D
    {
        [Export]
        public float Lifetime = 1;
        protected static void StartVfx(Godot.Node node)
        {
            if (node is GpuParticles2D particles2D)
            {
                particles2D.Emitting = true;
            }
            foreach (Godot.Node child in node.GetChildren())
            {
                StartVfx(child);
            }
        }

        public override void _Ready()
        {
            StartVfx(this);
            this.GetTree().CreateTimer(this.Lifetime).Connect(SceneTreeTimer.SignalName.Timeout, Callable.From(this.AfterExpired));
        }
        private void AfterExpired()
        {
            if (!IsInstanceValid(this))
                return;
            this.QueueFreeSafely();
        }
    }
}