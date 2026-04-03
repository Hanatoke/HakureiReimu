using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Mover
{
    public interface IMover
    {
        Vector2 CurrentPosition(FlyingVFX node, float time, float delta);
        bool IsHit(FlyingVFX node, float time, float delta) => time > 1;
    }
}