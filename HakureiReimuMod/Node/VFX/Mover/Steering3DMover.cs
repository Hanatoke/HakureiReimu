using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Mover
{
    public class Steering3DMover : IMover
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public float Acceleration;
        public float TurnSpeed;

        public Vector3 Target;
        public float MaxSpeed;
        // 👉 深度参数
        public float DepthScale = 0.02f; // Z 对缩放影响
        public float DepthInfluence = 1f; // Z 对转向影响
        public Steering3DMover(
            Vector2 startPos,
            Vector2 targetPos,
            Vector3 startVelocity,
            float startZ = 0f,
            float targetZ = 0f,
            float turnSpeed = 5f,
            float acceleration = 0f)
        {
            Position = new Vector3(startPos.X, startPos.Y, startZ);
            Target = new Vector3(targetPos.X, targetPos.Y, targetZ);

            TurnSpeed = turnSpeed;
            Acceleration = acceleration;
            

            Vector2 v2 = startVelocity.Length() > 0.001f
                ? new Vector2(startVelocity.X, startVelocity.Y)
                : (targetPos - startPos).Normalized();
            MaxSpeed = v2.Length()+Acceleration * 2;
            Velocity = new Vector3(v2.X, v2.Y, 0);
        }

        public bool IsHit(FlyingVFX node, float time, float delta)
        {
            return Position.DistanceTo(Target) < 10+Velocity.Length()/50f;
        }

        public Vector2 CurrentPosition(FlyingVFX node, float time, float delta)
        {
            // 🎯 方向计算（3D）
            Vector3 toTarget = (Target - Position).Normalized();

            Vector3 currentDir = Velocity.Length() > 0.001f
                ? Velocity.Normalized()
                : toTarget;

            // 👉 限制转向（3D版：用lerp代替角度）
            float turnFactor = Mathf.Clamp(TurnSpeed * delta, 0f, 1f);

            Vector3 newDir = currentDir.Lerp(toTarget, turnFactor).Normalized();

            // 👉 吸附（越近越准）
            float dist = Position.DistanceTo(Target);
            float factor = Mathf.SmoothStep(0f, 1f, 1f - dist / 400f);

            newDir = newDir.Lerp(toTarget, factor).Normalized();

            // =========================
            // 🚀 速度控制
            // =========================
            float speed = Velocity.Length();
            speed += Acceleration * delta;
            if (speed > MaxSpeed)
            {
                speed = MaxSpeed;
            }
            Velocity = newDir * speed;

            // 📍 更新位置
            Position += Velocity * delta;

            // 🎬 投影到2D
            Vector2 screenPos = new(Position.X, Position.Y);

            // 👉 深度缩放（远小近大）
            float scale = 1f + Position.Z * DepthScale;
            node.Scale = new Vector2(scale, scale);

            // 👉 层级（Z越小越前）
            node.ZIndex = (int)(-Position.Z);

            return screenPos;
        }
    }
}