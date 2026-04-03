using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Mover
{
    public class Curve2DMover :IMover
    {
        public Curve2D Curve2D;
        private float _curveLength;

        public Curve2DMover(Vector2 startPos, Vector2 targetPos,params Vector2[] controlPoints)
        {
            Curve2D = new Curve2D();
            Curve2D.AddPoint(startPos);
            foreach (Vector2 t in controlPoints)
            {
                Curve2D.AddPoint(t);
            }
            Curve2D.AddPoint(targetPos);
            Curve2D.BakeInterval = 0.5f;
            _curveLength = Curve2D.GetBakedLength();
        }
        public Vector2 CurrentPosition(FlyingVFX node, float time, float delta)
        {
            float distance =time * _curveLength;
            return Curve2D.SampleBaked(distance);
        }
    }
}