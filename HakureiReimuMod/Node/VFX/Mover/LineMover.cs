using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Mover
{
    public class LineMover :IMover
    {
        public Vector2 StartPos;
        public Vector2 TargetPos;

        public LineMover(Vector2 startPos, Vector2 targetPos)
        {
            StartPos = startPos;
            TargetPos = targetPos;
        }
        public Vector2 CurrentPosition(FlyingVFX node, float time, float delta)
        {
            return StartPos.Lerp(TargetPos,time);
        }
    }
}