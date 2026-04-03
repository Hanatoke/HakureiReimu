using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Mover
{
    public class Curve3DMover :IMover
    {
        public Curve3D Curve3D;
        private float _curveLength;

        public Curve3DMover(Vector2 startPos, Vector2 targetPos,params Vector3[] controlPoints)
        {
            Curve3D = new Curve3D();
            Curve3D.AddPoint(new Vector3(startPos.X, startPos.Y, 0));
            foreach (Vector3 t in controlPoints)
            {
                Curve3D.AddPoint(t);
            }
            Curve3D.AddPoint(new Vector3(targetPos.X, targetPos.Y, 0));
            _curveLength = Curve3D.GetBakedLength();
        }
        public Vector2 CurrentPosition(FlyingVFX node, float time, float delta)
        {
            float distance = time * _curveLength;

            Vector3 p = Curve3D.SampleBaked(distance);

            // 👉 投影到2D
            Vector2 pos = new(p.X, p.Y);

            // 👉 深度
            float depth = p.Z;

            // 缩放（远小近大）
            float scale = 1.0f + depth * 0.002f;
            node.Scale = new Vector2(scale, scale);

            // 层级（谁近谁在前）
            node.ZIndex = (int)(-depth);
            return pos;
        }
    }
}