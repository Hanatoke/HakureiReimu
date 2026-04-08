using Godot;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Mover
{
    public class SteeringMover : IMover
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Speed;
        public float Acceleration;
        public float TurnSpeed;
        public float MaxSpeed;

        public Vector2 Target;

        public SteeringMover(Vector2 startPos, Vector2 targetPos,Vector2 startVelocity, float turnSpeed = 10f,float acceleration = 4000)
        {
            Position = startPos;
            Target = targetPos;
            Speed = startVelocity.Length();
            TurnSpeed = turnSpeed;
            Acceleration = acceleration;
            MaxSpeed = Speed+acceleration * 2;
            Velocity = startVelocity.Length()>=0.001f?startVelocity:(targetPos-startPos).Normalized();
        }
        public bool IsHit(FlyingVFX node, float time, float delta)
        {
            return node.GlobalPosition.DistanceTo(Target) < 20f;
        }
        public Vector2 CurrentPosition(FlyingVFX node, float time, float delta)
        {

            Vector2 toTarget = (Target - Position).Normalized();
            Vector2 currentDir = Velocity.Normalized();

            // 👉 限制最大转角（更真实）
            float maxTurn = TurnSpeed * delta;

            float angle = currentDir.AngleTo(toTarget);
            angle = Mathf.Clamp(angle, -maxTurn, maxTurn);

            Vector2 newDir = currentDir.Rotated(angle).Normalized();

            // 👉 吸附（越近越准）
            float dist = Position.DistanceTo(Target);
            float factor = Mathf.SmoothStep(0f, 1f, 1f - dist / 400f);

            newDir = newDir.Lerp(toTarget, factor).Normalized();
            Speed += Acceleration * delta;
            if(Speed > MaxSpeed)Speed=MaxSpeed;
            Velocity = newDir * Speed;

            Position += Velocity * delta;

            return Position;
        }
    }
}