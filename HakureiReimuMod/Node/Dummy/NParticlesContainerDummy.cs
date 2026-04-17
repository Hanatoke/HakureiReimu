using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace HakureiReimu.HakureiReimuMod.Node.Dummy
{
    public partial class NParticlesContainerDummy :NParticlesContainer
    {
        public override void _Ready()
        {
            GpuParticles2D[] gpuParticles2Ds = GetChildren().OfType<GpuParticles2D>().ToArray();
            AccessTools.Field(typeof(NParticlesContainer), "_particles").SetValue(this,new Godot.Collections.Array<GpuParticles2D>(gpuParticles2Ds));
        }
    }
}